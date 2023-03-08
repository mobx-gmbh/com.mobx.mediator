using MobX.Utilities;
using UnityEngine.VFX;

namespace MobX.Mediator.Pooling
{
    /// <summary>
    ///     Generic <see cref="VisualEffect" /> object pool.
    /// </summary>
    public class VisualEffectPool : PoolAsset<VisualEffect>
    {
        protected override void OnReleaseInstance(VisualEffect instance)
        {
            instance.SetActive(false);
        }

        protected override void OnGetInstance(VisualEffect instance)
        {
            instance.SetActive(true);
        }
    }
}