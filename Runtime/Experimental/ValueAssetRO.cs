namespace MobX.Mediator.Experimental
{
    public abstract class ValueAssetRO<TValue> : MediatorAsset
    {
        public TValue Value => GetValue();

        public abstract TValue GetValue();
    }
}