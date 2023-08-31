﻿using System;
using UnityEngine;

namespace MobX.Mediator.Values
{
    /// <summary>
    ///     Proxy variable that either points to a <see cref="SerializedValueAsset{TValue}" /> or a locally serialized value.
    /// </summary>
    [Serializable]
    public struct VariableRO<T>
    {
        [SerializeField] private bool byReference;
        [SerializeField] private ReadonlyValueAsset<T> reference;
        [SerializeField] private T value;

        /// <summary>
        ///     Access the contained value.
        /// </summary>
        public T Value => byReference ? reference.Value : value;

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator T(VariableRO<T> variableRO)
        {
            return variableRO.Value;
        }
    }
}
