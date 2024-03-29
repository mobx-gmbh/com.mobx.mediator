﻿using MobX.Mediator.Callbacks;
using MobX.Utilities.Types;
using UnityEngine;

namespace MobX.Mediator.Registry
{
    /// <summary>
    ///     Registered assets are always included in a build and loaded during startup.
    ///     They can also be resolved during runtime, using their GUID.
    /// </summary>
    public abstract class RegisteredAsset : ScriptableAsset, IAssetGUID
    {
        [SerializeField] private RuntimeGUID guid;
        public RuntimeGUID GUID => guid;

#if UNITY_EDITOR
        protected override void OnEnable()
        {
            base.OnEnable();
            RuntimeGUID.Create(this, ref guid);
            AssetRegistry.RegisterAsset(this);
        }
#endif
    }
}