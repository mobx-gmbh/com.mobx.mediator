using MobX.Utilities;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using MobX.Utilities.Types;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace MobX.Mediator.Pooling
{
    public abstract class PoolAsset : RuntimeAsset
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


        #region Fields & Properties

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

        public bool IsLoaded { get; private set; }
        private ObjectPool<T> _pool;
        private Transform _parent;
        private Loop _index;

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

        /// <summary>
        ///     Called by the pool when a new instance needs to be created.
        /// </summary>
        private void OnGetInstanceInternal(T instance)
        {
            OnGetInstance(instance);
            Debug.Log("Pool", $"Get instance {instance} from pool {name}");
            if (autoRelease)
            {
                var timer = new Timer(lifeSpanInSeconds);
                _autoReleaseInstances.AddOrUpdate(instance, timer);
            }
        }

        /// <summary>
        ///     Called by the pool when an instance is released back to the pool.
        /// </summary>
        private void OnReleaseInstanceInternal(T instance)
        {
            if (autoRelease)
            {
                _autoReleaseInstances.Remove(instance);
            }
            OnReleaseInstance(instance);
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
                    return Instantiate(prefabs[_index++], _parent);
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
                Warmup();
            }
        }

        public void OnEndPlay()
        {
            Dispose();
        }

        #endregion


        #region Warup & Termination

        private IObjectPool<T> GetOrCreatePool()
        {
            if (IsLoaded)
            {
                return _pool;
            }

            Warmup();
            return _pool;
        }

        public void Warmup()
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

            if (!autoRelease)
            {
                EngineCallbacks.RemoveUpdateListener(this);
            }

            if (multiple && selectionMode == SelectionMode.RoundRobin)
            {
                _index = Loop.Create(prefabs);
            }

            var defaultCapacity = initialPoolSize;
            var maxSize = limitPoolSize ? maxPoolSize : 10000;

            _pool ??= new ObjectPool<T>(CreateInstance, OnGetInstanceInternal, OnReleaseInstanceInternal, OnDestroyInstance, collectionCheck, defaultCapacity, maxSize);
            List<T> buffer = ListPool<T>.Get();
            for (var i = 0; i < initialPoolSize; i++)
            {
                buffer.Add(_pool.Get());
            }

            foreach (T element in buffer)
            {
                _pool.Release(element);
            }

            ListPool<T>.Release(buffer);
            IsLoaded = true;
        }

        public void Dispose()
        {
            IsLoaded = false;
            _pool?.Dispose();
            _pool = null;
        }

        #endregion


        #region Object Pool Interface

        public T Get()
        {
            AssertIsPlaying();
            return GetOrCreatePool().Get();
        }

        public PooledObject<T> Get(out T instance)
        {
            AssertIsPlaying();
            return GetOrCreatePool().Get(out instance);
        }

        public void Release(T element)
        {
            AssertIsPlaying();
            if (IsLoaded)
            {
                _pool.Release(element);
            }
        }

        public void Clear()
        {
            AssertIsPlaying();
            if (IsLoaded)
            {
                _pool.Clear();
            }
        }

        public int CountInactive => IsLoaded ? _pool.CountInactive : 0;

        #endregion
    }
}
