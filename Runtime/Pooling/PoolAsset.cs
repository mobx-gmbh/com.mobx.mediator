namespace MobX.Mediator.Pooling
{
    public abstract class PoolAsset : MediatorAsset
    {
        /// <summary>
        ///     Preload assets of the pool.
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

        /// <summary>
        ///     Unload assets of the pool. This will force every element of the pool to be returned to the pool.
        /// </summary>
        public abstract void Unload();
    }
}