using UnityEngine;

namespace MobX.Mediator.Pooling
{
    public abstract class IPoolAsset : ScriptableObject
    {
        /// <summary>
        ///     Preload assets to prevent potential frame drops.
        /// </summary>
        public abstract void Warmup();

        /// <summary>
        ///     Clear the elements of the pool.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        ///     Recreate the elements of the pool.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        ///     Destroy the elements of the pool.
        /// </summary>
        public abstract void Dispose();
    }
}
