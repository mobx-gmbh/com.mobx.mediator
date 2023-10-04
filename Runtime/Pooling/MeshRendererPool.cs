using MobX.Utilities;
using UnityEngine;

namespace MobX.Mediator.Pooling
{
    public class MeshRendererPool : PoolAsset<MeshRenderer>
    {
        protected override void OnReleaseInstance(MeshRenderer instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(Parent);
        }

        protected override void OnGetInstance(MeshRenderer instance)
        {
            instance.SetActive(true);
        }
    }
}