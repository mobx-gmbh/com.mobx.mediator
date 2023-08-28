using MobX.Utilities.Inspector;
using System;

namespace MobX.Mediator.Values
{
    public abstract class ReadonlyValueAsset<TValue> : ValueAsset
    {
        [ReadonlyInspector]
        public TValue Value => GetValue();

        public abstract TValue GetValue();

        public virtual event Action<TValue> Changed;

        public static implicit operator TValue(ReadonlyValueAsset<TValue> valueAsset)
        {
#if UNITY_EDITOR
            if (valueAsset == null)
            {
                return default(TValue);
            }
#endif
            return valueAsset.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
