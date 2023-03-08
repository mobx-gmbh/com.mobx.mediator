using MobX.Utilities;
using UnityEngine;

namespace MobX.Mediator.Pooling
{
    /// <summary>
    ///     Generic <see cref="Component" /> object pool.
    /// </summary>
    public class ComponentPool : PoolAsset<Component>
    {
        protected override void OnReleaseInstance(Component item)
        {
            item.SetActive(false);
        }

        protected override void OnGetInstance(Component instance)
        {
            instance.SetActive(true);
        }
    }
}
