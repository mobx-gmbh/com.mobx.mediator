using MobX.Mediator.Collections;
using MobX.Utilities;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Registry;

namespace MobX.Mediator.Pooling
{
    public class PoolList : ListAsset<PoolAsset>, IOnInitializationCompleted
    {
        public void OnInitializationCompleted()
        {
            foreach (var runtimeAsset in AssetRegistry.RuntimeAssets)
            {
                if (runtimeAsset is PoolAsset poolAsset)
                {
                    this.AddUnique(poolAsset);
                }
            }
        }
    }
}
