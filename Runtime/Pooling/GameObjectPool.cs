using UnityEngine;

namespace MobX.Mediator.Pooling
{
    /// <summary>
    ///     Generic <see cref="GameObject" /> object pool.
    /// </summary>
    public class GameObjectPool : PoolAsset<GameObject>
    {
        protected override void OnReleaseCallback(GameObject item)
        {
            item.SetActive(false);
        }

        protected override void OnGetCallback(GameObject item)
        {
            item.SetActive(true);
        }
    }
}