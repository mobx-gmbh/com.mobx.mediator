using MobX.Utilities;
using UnityEngine;

namespace MobX.Mediator.Pooling
{
    /// <summary>
    ///     Generic <see cref="Component" /> object pool.
    /// </summary>
    public class ComponentPool : PoolAsset<Component>
    {
        protected override void OnReleaseCallback(Component item)
        {
            item.SetActive(false);
        }

        protected override void OnGetCallback(Component item)
        {
            item.SetActive(true);
        }
    }
}