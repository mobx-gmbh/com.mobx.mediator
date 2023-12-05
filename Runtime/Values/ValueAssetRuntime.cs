﻿using JetBrains.Annotations;
using MobX.Inspector;
using MobX.Mediator.Callbacks;
using MobX.Mediator.Events;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobX.Mediator.Values
{
    public abstract class ValueAssetRuntime<TValue> : ValueAssetRW<TValue>, IValueAsset<TValue>
    {
        private readonly Broadcast<TValue> _changed = new();
        [NonSerialized] private TValue _value;

        [Line]
        [OnValueChanged(nameof(UpdateValue))]
        [SerializeField] private TValue defaultValue;

        [ShowInInspector]
        public override TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        [PublicAPI]
        public override event Action<TValue> Changed
        {
            add => _changed.Add(value);
            remove => _changed.Remove(value);
        }

        public override void SetValue(TValue value)
        {
            if (EqualityComparer<TValue>.Default.Equals(value, _value))
            {
                return;
            }
            _value = value;
            _changed.Raise(value);
        }

        public override TValue GetValue()
        {
            return _value;
        }

        private void UpdateValue()
        {
            if (Application.isPlaying is false)
            {
                _value = defaultValue;
            }
        }


        #region Initialization & Shutdown

        [CallbackOnInitialization]
        private void Initialize()
        {
            _value = defaultValue;
        }

        [CallbackOnApplicationQuit]
        private void Shutdown()
        {
            _value = defaultValue;
        }

#if UNITY_EDITOR

        [CallbackOnEnterEditMode]
        private void OnEnterEditMode()
        {
            _value = defaultValue;
        }

        [CallbackOnExitEditMode]
        private void OnExitEditMode()
        {
            _value = defaultValue;
        }

#endif

        #endregion
    }
}