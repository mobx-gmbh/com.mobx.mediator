using System;

namespace MobX.Mediator.Generation
{
    [Flags]
    public enum MediatorTypes
    {
        Everything = -1,
        None = 0,
        ValueAsset = 1 << 0,
        EventAsset = 1 << 1,
        PoolAsset = 1 << 2,
        RequestAsset = 1 << 3,
        ListAsset = 1 << 4,
        ArrayAsset = 1 << 5,
        HashSetAsset = 1 << 6,
        SetAsset = 1 << 7,
        StackAsset = 1 << 8,
        QueueAsset = 1 << 9,
        DictionaryAsset = 1 << 10,
        MapAsset = 1 << 11,

        ValueAssetSerialized = 1 << 12,
        ValueAssetRuntime = 1 << 13,
        ValueAssetPersistent = 1 << 14,
        ValueAssetProperty = 1 << 15,

        CollectionAssets = ListAsset | ArrayAsset | HashSetAsset | SetAsset | StackAsset | QueueAsset |
                           DictionaryAsset | MapAsset,
        CoreTypes = ValueAsset | EventAsset | ListAsset | ArrayAsset
    }
}