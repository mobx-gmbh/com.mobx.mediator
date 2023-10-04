namespace MobX.Mediator.Values
{
    public abstract class ValueAsset<TValue> : ReadonlyValueAsset<TValue>
    {
        public new abstract TValue Value { get; set; }

        public abstract void SetValue(TValue value);
    }
}
