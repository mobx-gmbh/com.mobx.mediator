namespace MobX.Mediator.Factory
{
    public abstract class FactoryAsset<TProduct> : MediatorAsset
    {
        /// <summary>
        ///     Create a new instance of T
        /// </summary>
        public abstract TProduct Create();
    }

    public abstract class FactoryAsset<TProduct, TContext> : MediatorAsset
    {
        /// <summary>
        ///     Create a new instance of T
        /// </summary>
        public abstract TProduct Create(ref TContext context);
    }
}