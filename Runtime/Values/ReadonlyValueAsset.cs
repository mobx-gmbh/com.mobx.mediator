namespace MobX.Mediator.Values
{
    public abstract class ReadonlyValueAsset<TValue> : ValueAsset
    {
        public TValue Value => GetValue();

        public abstract TValue GetValue();
    }
}
