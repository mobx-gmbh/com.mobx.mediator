using System;

namespace MobX.Mediator.Values
{
    public abstract class ValueAsset<TValue> : ReadonlyValueAsset<TValue>
    {
        public abstract event Action<TValue> Changed;

        new public abstract TValue Value { get; set; }

        public abstract void SetValue(TValue newValue);
    }
}
