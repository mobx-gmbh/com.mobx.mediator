using MobX.Utilities;
using UnityEngine.VFX;

namespace MobX.Mediator.Pooling
{
    /// <summary>
    ///     Generic <see cref="VisualEffect" /> object pool.
    /// </summary>
    public class VisualEffectPool : PoolAsset<VisualEffect>
    {
        protected override void OnReleaseCallback(VisualEffect item)
        {
            item.SetActive(false);
        }

        protected override void OnGetCallback(VisualEffect item)
        {
            item.SetActive(true);
        }
    }
}