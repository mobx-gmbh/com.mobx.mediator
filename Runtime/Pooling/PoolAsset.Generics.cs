﻿using MobX.Utilities;
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
    public abstract partial class PoolAsset<T> : PoolAsset,
        IDisposable,
        IObjectPool<T>,
        IOnAfterFirstSceneLoad,
        IOnQuit
        where T : Object
    {
        #region Settings

        [Foldout("Pool")]
        [SerializeField] private bool multiple;
        [ConditionalHide(nameof(multiple))]
        [SerializeField] private T prefab;
        [ConditionalShow(nameof(multiple))]
        [SerializeField] private T[] prefabs;
        [ConditionalShow(nameof(multiple))]
        [SerializeField] private SelectionMode selectionMode;

        [Header("Initialization")]
        [Tooltip("When enabled, the pool is created at the start of the game")]
        [SerializeField] private bool warmupOnBeginPlay;
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private bool limitPoolSize;
        [ConditionalShow(nameof(limitPoolSize))]
        [SerializeField] private int maxPoolSize = 100;

        #endregion


        #region Properties

        public T Prefab => prefab;

        [ReadonlyInspector]
        public PoolState State { get; private set; }

        public int CountAll { get; private set; }

        public Transform Parent { get; private set; }

        public int CountInactive => _pool.Count;

        public int MaxPoolSize => limitPoolSize ? maxPoolSize : 10000;

        #endregion


        #region Fields

        private Loop _prefabIndex;

        [ReadonlyInspector]
        [Foldout("Debug")]
        private readonly List<T> _pool = new();

        [ReadonlyInspector]
        [Foldout("Debug")]
        private readonly List<T> _activeItems = new();

        private enum SelectionMode
        {
            RoundRobin,
            Random
        }

        #endregion


        #region Public

        public T Get()
        {
            return GetInternal();
        }

        public void Release(T instnace)
        {
            ReleaseInternal(instnace);
        }

        public sealed override void Clear()
        {
            ClearInternal();
        }

        public sealed override void Load()
        {
            LoadInternal();
        }

        public sealed override void ResetPool()
        {
            ResetInternal();
        }

        public sealed override void Dispose()
        {
            UnloadInternal();
        }

        public sealed override void Unload()
        {
            UnloadInternal();
        }

        #endregion


        #region Protected

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
                    Debug.LogError("Pooling", $"Prefab {this} was null! Cannot create new instance!", this);
                }
                return Instantiate(prefab, Parent);
            }

            return selectionMode switch
            {
                SelectionMode.RoundRobin => Instantiate(prefabs[_prefabIndex++], Parent),
                SelectionMode.Random => Instantiate(prefabs.RandomItem(), Parent),
                _ => throw new ArgumentOutOfRangeException()
            };
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


        #region Editor

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
    }
}