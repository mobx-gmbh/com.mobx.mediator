﻿using MobX.Mediator.Callbacks;
using MobX.Utilities.Libraries;
using MobX.Utilities.Types;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace MobX.Mediator.Pooling
{
    public partial class PoolAsset<T>
    {
        #region Runtime Callbacks

        [CallbackOnAfterFirstSceneLoad]
        private void OnAfterFirstSceneLoad()
        {
            if (warmupOnBeginPlay)
            {
                Load();
            }
        }

        [CallbackOnApplicationQuit]
        private void OnQuit()
        {
            Dispose();
        }

        #endregion


        #region Warup And Refresh

        // ReSharper disable Unity.PerformanceAnalysis
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LoadInternal()
        {
            if (State != PoolState.Unloaded)
            {
                return;
            }
            State = PoolState.Loading;

            var buffer = ListPool<T>.Get();

            if (Parent == null)
            {
                Parent = PoolTransform.Create(this);
            }

            if (multiple && selectionMode == SelectionMode.RoundRobin)
            {
                _prefabIndex = Loop.Create(prefabs);
            }

            for (var i = 0; i < initialPoolSize; i++)
            {
                buffer.Add(GetInternal());
            }

            foreach (var element in buffer)
            {
                ReleaseInternal(element);
            }

            ListPool<T>.Release(buffer);
            State = PoolState.Loaded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnloadInternal()
        {
            AssertIsPlaying();

            foreach (var instance in _pool)
            {
                OnDestroyInstance(instance);
            }

            _pool.Clear();
            CountAll = 0;
            if (Parent != null)
            {
                Destroy(Parent.gameObject);
            }
            Parent = null;
            State = PoolState.Unloaded;
        }

        private void ClearInternal()
        {
            foreach (var instance in _pool)
            {
                OnDestroyInstance(instance);
            }
            _pool.Clear();
            CountAll = 0;
        }

        #endregion


        #region Get And Release

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetInternal()
        {
            AssertIsPlaying();

            if (State == PoolState.Unloaded)
            {
                LoadInternal();
            }

            T instance;
            var isPoolEmpty = _pool.Count == 0;
            if (isPoolEmpty)
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
            UpdateInspector();
            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseInternal(T instance)
        {
            AssertIsPlaying();
            if (Gameloop.IsQuitting)
            {
                return;
            }
            if (instance == null)
            {
                Debug.LogError("Pooling", "Released Instance was null!");
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
                Debug.Log("Pooling",
                    $"Pool [{name}] reached its max allowed capacity! Destroying released instance: [{instance}] ");
                OnDestroyInstance(instance);
            }

            UpdateInspector();
        }

        #endregion


        #region Assertions & Obsolete

        /// <summary>
        ///     This method is Obsolete and will throw an InvalidOperationException since we cannot create a new
        ///     <see cref="PooledObject&lt;T&gt;" /> since its constructor is internal.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("This method is Obsolete and will throw an InvalidOperationException")]
        public PooledObject<T> Get(out T instance)
        {
            throw new InvalidOperationException("Invalid method call!");
        }

        [Conditional("UNITY_EDITOR")]
        private static void AssertIsPlaying()
        {
            Assert.IsTrue(Application.isPlaying, "Application Is Not Playing!");
        }

        [Conditional("UNITY_EDITOR")]
        private void UpdateInspector()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        #endregion
    }
}