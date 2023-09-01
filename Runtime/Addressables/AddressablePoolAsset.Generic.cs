using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace MobX.Mediator.Addressables
{
    public partial class AddressablePoolAsset<T> : AddressablePoolAsset where T : Object
    {
        #region Inspctor

        [SerializeField] private AssetReferenceT<T> prefabReference;

        #endregion


        public override void Load()
        {
            if (prefabReference.IsValid())
            {
                return;
            }
            prefabReference.LoadAssetAsync();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override void ResetPool()
        {
            throw new NotImplementedException();
        }

        public override void Unload()
        {
            prefabReference.ReleaseAsset();
        }
    }
}