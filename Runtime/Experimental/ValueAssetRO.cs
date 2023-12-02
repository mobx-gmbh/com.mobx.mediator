namespace MobX.Mediator.Experimental
{
    /// <summary>
    ///     ReadOnly value asset.
    /// </summary>
    /// <typeparam name="TValue">The type of the contained value</typeparam>
    public abstract class ValueAssetRO<TValue> : MediatorAsset
    {
        public TValue Value => GetValue();

        public abstract TValue GetValue();
    }
}