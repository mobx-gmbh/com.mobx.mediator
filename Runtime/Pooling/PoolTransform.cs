﻿using MobX.Utilities;
using MobX.Utilities.Inspector;
using UnityEngine;

namespace MobX.Mediator.Pooling
{
    internal sealed class PoolTransform : MonoBehaviour
    {
#if UNITY_EDITOR
        [Button]
        private void SelectPoolAsset()
        {
            UnityEditor.Selection.activeObject = _poolAsset;
            UnityEditor.EditorGUIUtility.PingObject(_poolAsset);
        }

        private Object _poolAsset;
#endif

        internal static Transform Create(Object pool)
        {
            var instance = new GameObject(pool.name);

            instance.DontDestroyOnLoad();
#if UNITY_EDITOR
            instance.AddComponent<PoolTransform>()._poolAsset = pool;
#endif
            return instance.transform;
        }
    }
}
