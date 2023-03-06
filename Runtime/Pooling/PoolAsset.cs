using MobX.Utilities;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace MobX.Mediator.Pooling
{
    public abstract class PoolAsset<T> : RuntimeAsset, IDisposable, IObjectPool<T>, IOnBeginPlay, IOnEndPlay
        where T : Object
    {
        #region Fields & Properties

        [SerializeField] private T prefab;
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

        public bool IsLoaded { get; private set; }
        private ObjectPool<T> _pool;
        private Transform _parent;

        #endregion


        #region Pool Callbacks

        /// <summary>
        ///     Called by the pool when a new instance needs to be created.
        /// </summary>
        /// <returns>A new instance for the pool</returns>
        protected virtual T CreateFunc()
        {
            return Instantiate(prefab, _parent);
        }

        /// <summary>
        ///     Called by the pool when a new instance needs to be created.
        /// </summary>
        protected virtual void OnGetCallback(T item)
        {
        }

        /// <summary>
        ///     Called by the pool when an instance is released back to the pool.
        /// </summary>
        protected virtual void OnReleaseCallback(T item)
        {
        }

        /// <summary>
        ///     Called by the pool when it is disposed.
        /// </summary>
        protected virtual void OnElementDestroy(T item)
        {
            Destroy(item);
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

            var defaultCapacity = initialPoolSize;
            var maxSize = limitPoolSize ? maxPoolSize : 10000;

            _pool ??= new ObjectPool<T>(CreateFunc, OnGetCallback, OnReleaseCallback, OnElementDestroy, true,
                defaultCapacity, maxSize);
            var buffer = ListPool<T>.Get();
            for (var i = 0; i < initialPoolSize; i++)
            {
                buffer.Add(_pool.Get());
            }

            foreach (var element in buffer)
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

        public PooledObject<T> Get(out T v)
        {
            AssertIsPlaying();
            return GetOrCreatePool().Get(out v);
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