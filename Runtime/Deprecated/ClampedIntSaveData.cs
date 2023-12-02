﻿using MobX.Utilities;
using UnityEngine;

namespace MobX.Mediator.Deprecated
{
    public class ClampedIntSaveData : IntSaveData
    {
        [SerializeField] private int minValue;
        [SerializeField] private int maxValue = 1;

        public override void SetValue(int newValue)
        {
            base.SetValue(newValue.Clamp(minValue, maxValue));
        }
    }
}