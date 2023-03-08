﻿using MobX.Utilities;
using UnityEngine;

namespace MobX.Mediator.Pooling
{
    /// <summary>
    ///     Generic <see cref="Component" /> object pool.
    /// </summary>
    public abstract class ComponentPool<TComponent> : PoolAsset<TComponent> where TComponent : Component
    {
        protected override void OnReleaseInstance(TComponent instance)
        {
            instance.SetActive(false);
        }

        protected override void OnGetInstance(TComponent instance)
        {
            instance.SetActive(true);
        }
    }
}