using MobX.Utilities.Inspector;

namespace MobX.Mediator.Values
{
    public abstract class ReadonlyValueAsset<TValue> : ValueAsset
    {
        [ReadonlyInspector]
        public TValue Value => GetValue();

        public abstract TValue GetValue();

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
