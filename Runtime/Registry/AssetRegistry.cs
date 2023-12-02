using MobX.Inspector;
using MobX.Mediator.Callbacks;
using MobX.Mediator.Singleton;
using MobX.Utilities;
using MobX.Utilities.Collections;
using MobX.Utilities.Types;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MobX.Mediator.Registry
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadAttribute]
#endif
    public class AssetRegistry : SingletonAsset<AssetRegistry>
    {
        #region Inspector

        [SerializeField] private Map<string, Object> registry = new();

        #endregion


        #region Public

        /// <summary>
        ///     Collection of every registered asset.
        /// </summary>
        public static IReadOnlyDictionary<string, Object> Registry => Singleton.registry;

        /// <summary>
        ///     Register a unique asset. Registered assets can be resolved using their <see cref="RuntimeGUID" />
        /// </summary>
        public static void Register<T>(T asset) where T : Object, IAssetGUID
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                Singleton.registry.AddOrUpdate(asset.GUID.Value, asset);
            };
#endif
        }

        /// <summary>
        ///     Resolve an asset of type T by its GUID
        /// </summary>
        public static T Resolve<T>(string guid) where T : Object
        {
            return (T) Singleton.registry[guid];
        }

        /// <summary>
        ///     Resolve an asset of type T by its GUID as a UnityEngine.Object
        /// </summary>
        public static Object ResolveObject(string guid)
        {
            return Singleton.registry[guid];
        }

        /// <summary>
        ///     Try Resolve an asset of type T by its GUID
        /// </summary>
        public static bool TryResolve<T>(string guid, out T result) where T : Object
        {
            if (Singleton.registry.TryGetValue(guid, out var instance))
            {
                result = instance as T;
                return result != null;
            }

            result = default(T);
            return false;
        }

        #endregion


        #region Editor

#if UNITY_EDITOR

        [Button]
        [Foldout("Validation")]
        public bool ContainsAsset(Object obj)
        {
            return registry.ContainsValue(obj);
        }

        [Button]
        [Foldout("Validation")]
        public void Validate()
        {
            var assets = registry.ToArray();
            foreach (var (key, value) in assets)
            {
                if (value == null || value is not IAssetGUID)
                {
                    Debug.Log("Asset Registry", "Removing invalid unique asset registry entry!");
                    registry.Remove(key);
                }
            }
        }

        [Button]
        private void ClearAll()
        {
            registry.Clear();
        }

        static AssetRegistry()
        {
            Gameloop.BeforeDeleteAsset += OnBeforeDeleteAsset;
        }

        private static void OnBeforeDeleteAsset(string assetPath, Object asset)
        {
            var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            Singleton.registry.TryRemove(guid);
        }

#endif

        #endregion
    }
}