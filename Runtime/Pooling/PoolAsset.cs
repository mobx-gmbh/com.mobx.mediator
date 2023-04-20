using Cysharp.Threading.Tasks;
using MobX.Utilities;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using MobX.Utilities.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace MobX.Mediator.Pooling
{
    public abstract class PoolAsset : ScriptableObject
    {
        /// <summary>
        ///     Preload assets to prevent potential frame drops.
        /// </summary>
        public abstract void Warmup();

        /// <summary>
        ///     Preload assets to prevent potential frame drops.
        /// </summary>
        public abstract UniTask WarmupAsync(CancellationToken cancellationToken = new());

        /// <summary>
        ///     Refresh assets to prevent potential frame drops.
        /// </summary>
        public abstract UniTask RefreshAsync(Vector3 position, float warmupDurationInSeconds, CancellationToken cancellationToken = new());
    }

    public abstract class PoolAsset<T> : PoolAsset, IDisposable, IObjectPool<T>, IOnBeginPlay, IOnEndPlay
        where T : Object
    {
        #region Type Definitions

        public enum SelectionMode
        {
            RoundRobin,
            Random
        }

        #endregion


        #region Settings

        [Foldout("Pool")]
        [SerializeField] private bool multiple;
        [ConditionalHide(nameof(multiple))]
        [SerializeField] private T prefab;
        [ConditionalShow(nameof(multiple))]
        [SerializeField] private T[] prefabs;
        [ConditionalShow(nameof(multiple))]
        [SerializeField] private SelectionMode selectionMode;

        [Header("Pool Size")]
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private bool limitPoolSize;
        [ConditionalShow(nameof(limitPoolSize))]
        [SerializeField] private int maxPoolSize = 100;

        [Foldout("Optimizations")]
        [Tooltip("When enabled, the pool is created at the start of the game")]
        [SerializeField] private bool warmupOnBeginPlay;
        [SerializeField] private bool autoRelease;
        [ConditionalShow(nameof(autoRelease))]
        [SerializeField] private float lifeSpanInSeconds = 3;

        [Button]
        private void SelectRuntimeObject()
        {
#if UNITY_EDITOR
            if (Parent != null)
            {
                UnityEditor.Selection.activeObject = Parent;
            }
#endif
        }

        #endregion


        #region Fields & Properties

        public T Prefab => prefab;

        [Readonly]
        public PoolState State { get; private set; }
        public int CountAll { get; private set; }
        public Transform Parent { get; private set; }

        public int CountInactive => _pool.Count;
        public int MaxPoolSize => limitPoolSize ? maxPoolSize : 10000;

        private Loop _prefabIndex;

        private readonly List<T> _pool = new();

        #endregion


        #region Pool Callbacks

        /// <summary>
        ///     Called by the pool when a new instance needs to be created.
        /// </summary>
        /// <returns>A new instance for the pool</returns>
        protected virtual T CreateInstance()
        {
            if (!multiple)
            {
                if (prefab == null)
                {
                    Debug.LogError("Pooling", "Prefab for {this} was null! Cannot create new instance!", this);
                }
                return Instantiate(prefab, Parent);
            }

            switch (selectionMode)
            {
                case SelectionMode.RoundRobin:
                    return Instantiate(prefabs[_prefabIndex++], Parent);
                case SelectionMode.Random:
                    return Instantiate(prefabs.RandomItem(), Parent);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Called by the pool when a new instance needs to be created.
        /// </summary>
        protected virtual void OnGetInstance(T instance)
        {
        }

        /// <summary>
        ///     Called by the pool when an instance is released back to the pool.
        /// </summary>
        protected virtual void OnReleaseInstance(T instance)
        {
        }

        /// <summary>
        ///     Called by the pool when it is disposed.
        /// </summary>
        protected virtual void OnDestroyInstance(T instance)
        {
            Destroy(instance);
        }

        #endregion


        #region Runtime Callbacks

        public void OnBeginPlay()
        {
#if UNITY_EDITOR
            if (warmupOnBeginPlay)
            {
                WarmupAsync().Forget();
            }
#endif
        }

        public void OnEndPlay()
        {
            Dispose();
        }

        #endregion


        #region Ctor

        private void OnEnable()
        {
            EngineCallbacks.AddBeginPlayListener(this);
            EngineCallbacks.AddEndPlayListener(this);
        }

        private void OnDisable()
        {
            EngineCallbacks.RemoveBeginPlayListener(this);
            EngineCallbacks.RemoveEndPlayListener(this);
        }

        #endregion


        #region Warup & Termination

        public override void Warmup()
        {
            if (State != PoolState.Unloaded)
            {
                return;
            }
            State = PoolState.Loading;

            List<T> buffer = ListPool<T>.Get();

            if (Parent == null)
            {
                Parent = PoolHook.Create(this);
            }

            if (multiple && selectionMode == SelectionMode.RoundRobin)
            {
                _prefabIndex = Loop.Create(prefabs);
            }

            for (var i = 0; i < initialPoolSize; i++)
            {
                buffer.Add(Get(true));
            }

            foreach (T element in buffer)
            {
                Release(element);
            }

            ListPool<T>.Release(buffer);
            State = PoolState.Loaded;
        }

        public async override UniTask WarmupAsync(CancellationToken cancellationToken = new())
        {
            List<T> buffer = ListPool<T>.Get();
            try
            {
                if (State != PoolState.Unloaded)
                {
                    return;
                }

                State = PoolState.Loading;

                if (Parent == null)
                {
                    Parent = PoolHook.Create(this);
                }

                if (multiple && selectionMode == SelectionMode.RoundRobin)
                {
                    _prefabIndex = Loop.Create(prefabs);
                }

                for (var i = 0; i < initialPoolSize; i++)
                {
                    T instance = Get(true);
                    buffer.Add(instance);
                    await UniTask.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                }

                foreach (T element in buffer)
                {
                    Release(element);
                }

                ListPool<T>.Release(buffer);
                State = PoolState.Loaded;
            }
            catch (OperationCanceledException)
            {
                foreach (T element in buffer)
                {
                    Release(element);
                }

                ListPool<T>.Release(buffer);
                State = PoolState.Unloaded;
            }
        }

        public void Dispose()
        {
            Clear();
            State = PoolState.Unloaded;
        }

        #endregion


        #region Object Pool Interface

        public T Get()
        {
            return Get(false);
        }

        private T Get(bool discrete)
        {
            AssertIsPlaying();

            if (State == PoolState.Unloaded)
            {
                Warmup();
            }

            T instance;
            if (_pool.Count == 0)
            {
                instance = CreateInstance();
                ++CountAll;
            }
            else
            {
                switch (selectionMode)
                {
                    case SelectionMode.RoundRobin:
                        var index = _pool.Count - 1;
                        instance = _pool[index];
                        _pool.RemoveAt(index);
                        break;
                    case SelectionMode.Random:
                        instance = _pool.RandomItemRemove();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (instance == null)
            {
                Debug.LogError("Pooling", $"Requested Instance from {this} was null!", this);
                instance = CreateInstance();
                ++CountAll;
            }

            OnGetInstance(instance);
            if (!discrete && autoRelease)
            {
                ReleaseAsync(instance, lifeSpanInSeconds).Forget();
            }
            return instance;
        }

        public void Release(T instance)
        {
            AssertIsPlaying();
            if (EngineCallbacks.IsQuitting)
            {
                return;
            }
            if (instance == null)
            {
                Debug.Log("Pooling", "Released Instance was null!");
                return;
            }
            if (_pool.Contains(instance))
            {
                Debug.Log("Pooling", $"Released Instance [{instance}] was already released to the pool!", instance);
                return;
            }

            OnReleaseInstance(instance);
            if (CountInactive < MaxPoolSize)
            {
                _pool.Add(instance);
            }
            else
            {
                Debug.Log("Pooling", $"Pool [{name}] reached its max allowed capacity! Destroying released instance: [{instance}] ", LogOption.NoStacktrace);
                OnDestroyInstance(instance);
            }
        }

        public async UniTask ReleaseAsync(T instance, float delayInSeconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayInSeconds));
            Release(instance);
        }

        public void Clear()
        {
            AssertIsPlaying();
            foreach (T instance in _pool)
            {
                OnDestroyInstance(instance);
            }
            _pool.Clear();
            CountAll = 0;
            State = PoolState.Unloaded;
        }

        public PooledObject<T> Get(out T instance)
        {
            throw new InvalidOperationException("Invalid method call!");
        }

        #endregion


        #region Misc

        [Conditional("UNITY_EDITOR")]
        private static void AssertIsPlaying()
        {
            Assert.IsTrue(Application.isPlaying, "Application Is Not Playing!");
        }

        #endregion


        #region Async

        public async override UniTask RefreshAsync(Vector3 position, float warmupDurationInSeconds, CancellationToken cancellationToken = new())
        {
            try
            {
                T instance = Get(true);
                instance.TrySetPosition(position);
                await UniTask.Delay(TimeSpan.FromSeconds(warmupDurationInSeconds), cancellationToken: cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                Release(instance);
            }
            catch (OperationCanceledException)
            {
            }
        }

        #endregion
    }
}
