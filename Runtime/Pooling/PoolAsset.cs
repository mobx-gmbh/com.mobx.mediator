namespace MobX.Mediator.Pooling
{
    public abstract class PoolAsset : MediatorAsset
    {
        /// <summary>
        ///     Preload assets to prevent potential frame drops.
        /// </summary>
        public abstract void Load();

        /// <summary>
        ///     Clear the elements of the pool.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        ///     Release all active elements back to the pool.
        /// </summary>
        public abstract void ResetPool();
    }
}
