using MobX.Mediator.Collections;
using MobX.Utilities;
using MobX.Utilities.Callbacks;

namespace MobX.Mediator.Pooling
{
    public class PoolList : ListAsset<PoolAsset>
    {
#if UNITY_EDITOR

        [CallbackMethod(Segment.EnteredEditMode)]
        private void AddPoolAssets()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(PoolAsset)}");
            for (var i = 0; i < guids.Length; i++)
            {
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<PoolAsset>(assetPath);
                if (asset != null)
                {
                    this.AddUnique(asset);
                }
            }
        }

#endif
    }
}