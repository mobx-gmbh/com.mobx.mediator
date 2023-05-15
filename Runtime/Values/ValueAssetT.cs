using System;

namespace MobX.Mediator.Values
{
    public abstract class ValueAsset<TValue> : ReadonlyValueAsset<TValue>
    {
        public abstract event Action<TValue> Changed;

        new public abstract TValue Value { get; set; }

        public abstract void SetValue(TValue newValue);

        public static implicit operator TValue(ValueAsset<TValue> valueAsset)
        {
#if UNITY_EDITOR
            if (valueAsset == null)
            {
                return default(TValue);
            }
#endif
            return valueAsset.Value;
        }
    }
}
