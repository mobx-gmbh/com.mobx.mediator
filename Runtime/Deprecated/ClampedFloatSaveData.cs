﻿using MobX.Utilities;
using UnityEngine;

namespace MobX.Mediator.Deprecated
{
    public class ClampedFloatSaveData : FloatSaveData
    {
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 1f;

        public override void SetValue(float newValue)
        {
            base.SetValue(newValue.Clamp(minValue, maxValue));
        }
    }
}