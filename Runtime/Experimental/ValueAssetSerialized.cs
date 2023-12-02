using MobX.Inspector;
using UnityEngine;

namespace MobX.Mediator.Experimental
{
    public abstract class ValueAssetSerialized<TValue> : ValueAssetRW<TValue>, IValueAsset<TValue>
    {
        [Line]
        [SerializeField] private TValue value;

        public override TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public override void SetValue(TValue newValue)
        {
            if (Application.isPlaying)
            {
                return;
            }
            value = newValue;
        }

        public override TValue GetValue()
        {
            return value;
        }
    }
}