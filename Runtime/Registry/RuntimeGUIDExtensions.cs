using MobX.Utilities.Types;
using UnityEngine;

namespace MobX.Mediator.Registry
{
    public static class RuntimeGUIDExtensions
    {
        public static T ToAsset<T>(this RuntimeGUID guid) where T : Object
        {
            return AssetRegistry.ResolveAsset<T>(guid);
        }

        public static bool TryToAsset<T>(this RuntimeGUID guid, out T result) where T : Object
        {
            return AssetRegistry.TryResolveAsset(guid, out result);
        }

        public static RuntimeGUID ToGUID(this string value)
        {
            return new RuntimeGUID(value);
        }
    }
}