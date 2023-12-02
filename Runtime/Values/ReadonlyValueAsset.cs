﻿using Sirenix.OdinInspector;
using System;

namespace MobX.Mediator.Values
{
    public abstract class ReadonlyValueAsset<TValue> : ValueAsset
    {
        [ReadOnly]
        public TValue Value => GetValue();

        public abstract TValue GetValue();

#pragma warning disable
        public virtual event Action<TValue> Changed;
#pragma warning restore

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