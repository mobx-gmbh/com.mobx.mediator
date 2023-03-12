using MobX.Utilities;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using MobX.Utilities.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace MobX.Mediator.Pooling
{
    public abstract class PoolAsset : ScriptableObject
    {
    }

    public abstract class PoolAsset<T> : PoolAsset, IDisposable, IObjectPool<T>, IOnBeginPlay, IOnEndPlay, IOnUpdate
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

        [SerializeField] private T prefab;
        [ConditionalShow(nameof(multiple))]
        [SerializeField] private T[] prefabs;
        [ConditionalShow(nameof(multiple))]
        [SerializeField] private SelectionMode selectionMode;
        [SerializeField] private bool multiple;

        [Header("Pool Size")]
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private bool limitPoolSize;
        [ConditionalShow(nameof(limitPoolSize))]
        [SerializeField] private int maxPoolSize = 100;

        [Header("Optimizations")]
        [Tooltip("When enabled, the pool is created at the start of the game")]
        [SerializeField] private bool warmupOnBeginPlay;
        [Tooltip("When enabled, prefab is not unloaded if marked as unused")]
        [SerializeField] private bool keepInMemory;
        [Tooltip("When enabled, the pool is hidden in the editor hierarchy")]
        [SerializeField] private bool hidePoolInHierarchy;
        [Tooltip("When enabled, an exception is thrown if an instance is release to a the pool twice")]
        [SerializeField] private bool collectionCheck;

        [Header("Auto Reset")]
        [SerializeField] private bool autoRelease;
        [ConditionalShow(nameof(autoRelease))]
        [SerializeField] private float lifeSpanInSeconds = 3;

        #endregion


        #region Fields & Properties

        public bool IsLoaded { get; private set; }
        public int CountAll { get; private set; }

        public int CountInactive => _pool.Count;
        public int MaxPoolSize => limitPoolSize ? maxPoolSize : 10000;

        private Transform _parent;
        private Loop _prefabIndex;

        private readonly List<T> _pool = new List<T>();

        #endregion


        #region Internal Pool Callbacks

        private readonly Dictionary<T, Timer> _autoReleaseInstances = new Dictionary<T, Timer>();

        public void OnUpdate(float deltaTime)
        {
            List<T> releaseBuffer = ListPool<T>.Get();

            foreach (KeyValuePair<T, Timer> autoReleaseInstance in _autoReleaseInstances)
            {
                if (autoReleaseInstance.Value.Expired)
                {
                    releaseBuffer.Add(autoReleaseInstance.Key);
                }
            }

            foreach (T key in releaseBuffer)
            {
                _autoReleaseInstances.Remove(key);
            }

            ListPool<T>.Release(releaseBuffer);
        }

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
                return Instantiate(prefab, _parent);
            }

            switch (selectionMode)
            {
                case SelectionMode.RoundRobin:
                    return Instantiate(prefabs[_prefabIndex++], _parent);
                case SelectionMode.Random:
                    return Instantiate(prefabs.RandomItem(), _parent);
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
            if (warmupOnBeginPlay)
            {
                Load();
            }
        }

        public void OnEndPlay()
        {
            Dispose();
        }

        #endregion


        #region Ctor

        protected PoolAsset()
        {
            EngineCallbacks.AddBeginPlayListener(this);
            EngineCallbacks.AddEndPlayListener(this);
        }

        #endregion


        #region Warup & Termination

        public void Load()
        {
            if (IsLoaded)
            {
                return;
            }

            if (_parent == null)
            {
                _parent = PoolHook.Create(this, hidePoolInHierarchy);
            }

            if (keepInMemory)
            {
                prefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }

            if (autoRelease)
            {
                EngineCallbacks.AddUpdateListener(this);
            }

            if (multiple && selectionMode == SelectionMode.RoundRobin)
            {
                _prefabIndex = Loop.Create(prefabs);
            }

            List<T> buffer = ListPool<T>.Get();
            for (var i = 0; i < initialPoolSize; i++)
            {
                buffer.Add(Get());
            }

            foreach (T element in buffer)
            {
                Release(element);
            }

            ListPool<T>.Release(buffer);
            IsLoaded = true;
        }

        public void Dispose()
        {
            IsLoaded = false;
            Clear();
        }

        #endregion


        #region Object Pool Interface

        public T Get()
        {
            AssertIsPlaying();

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

            OnGetInstance(instance);

            if (autoRelease)
            {
                var timer = new Timer(lifeSpanInSeconds);
                _autoReleaseInstances.AddOrUpdate(instance, timer);
            }
            return instance;
        }

        public PooledObject<T> Get(out T instance)
        {
            throw new InvalidOperationException("Invalid method call!");
        }

        public void Release(T instance)
        {
            AssertIsPlaying();

            if (collectionCheck && _pool.Count > 0)
            {
                for (var index = 0; index < _pool.Count; ++index)
                {
                    if (instance == _pool[index])
                    {
                        throw new InvalidOperationException(
                            "Trying to release an object that has already been released to the pool.");
                    }
                }
            }

            if (autoRelease)
            {
                _autoReleaseInstances.Remove(instance);
            }

            OnReleaseInstance(instance);
            if (CountInactive < MaxPoolSize)
            {
                _pool.Add(instance);
            }
            else
            {
                Debug.Log("Pooling",
                    $"Pool [{name}] reached its max allowed capacity! Destroying released instance: [{instance}] ");
                OnDestroyInstance(instance);
            }
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
            EngineCallbacks.RemoveUpdateListener(this);
        }

        #endregion


        #region Misc

        [Conditional("UNITY_EDITOR")]
        private static void AssertIsPlaying()
        {
            Assert.IsTrue(Application.isPlaying, "Application Is Not Playing!");
        }

        #endregion
    }
}
