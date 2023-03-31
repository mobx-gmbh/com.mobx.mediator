using MobX.Utilities;
using MobX.Utilities.Inspector;
using UnityEngine;

namespace MobX.Mediator.Pooling
{
    sealed class PoolHook : MonoBehaviour
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

        // ReSharper disable Unity.PerformanceAnalysis
        public static Transform Create(Object pool)
        {
            var instance = new GameObject(pool.name);

            instance.DontDestroyOnLoad();
#if UNITY_EDITOR
            instance.AddComponent<PoolHook>()._poolAsset = pool;
#endif
            return instance.transform;
        }
    }
}
