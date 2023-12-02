using MobX.Mediator.Callbacks;
using MobX.Mediator.Events;
using Sirenix.OdinInspector;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MobX.Mediator.Values
{
    public abstract class PropertyAsset<T> : ValueAsset<T>
    {
        /// <summary>
        ///     Event, called every time the value changed.
        /// </summary>
        public sealed override event Action<T> Changed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => _changedEvent.Add(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => _changedEvent.Remove(value);
        }

        private Func<T> _getter;
        private Action<T> _setter;
        private readonly Broadcast<T> _changedEvent = new();

        public override T Value
        {
            get
            {
                if (_getter is null)
                {
#if UNITY_EDITOR
                    if (Application.isPlaying is false)
                    {
                        return default(T);
                    }
#endif
                    Debug.LogError("Property Asset", $"The Property Asset: {name} has no getter!");
                    return default(T);
                }
                return _getter();
            }
            set
            {
                if (_setter is null)
                {
                    Debug.LogError("Property Asset", $"The Property Asset: {name} has no setter!");
                    return;
                }
                _setter(value);
#if UNITY_EDITOR
                Repaint();
                _lastValue = value;
#endif
                _changedEvent.Raise(value);
            }
        }

        public void BindGetter(Func<T> getter)
        {
            _getter = getter;
        }

        public void BindSetter(Action<T> setter)
        {
            _setter = setter;
        }

        public void ReleaseGetter(Func<T> getter)
        {
            if (getter == _getter)
            {
                _getter = null;
            }
        }

        public void ReleaseSetter(Action<T> setter)
        {
            if (setter == _setter)
            {
                _setter = null;
            }
        }

        public sealed override void SetValue(T newValue)
        {
            Value = newValue;
        }

        public sealed override T GetValue()
        {
            return Value;
        }

        public void Update()
        {
            _changedEvent.Raise(GetValue());
        }


        #region Editor

#if UNITY_EDITOR

        [Button]
        private void UpdateValue()
        {
            _lastValue = _getter();
        }

        [CallbackOnEnterEditMode]
        private void OnEnterEditMode()
        {
            _lastValue = default(T);
            _getter = null;
            _setter = null;
        }

        [CallbackOnExitEditMode]
        private void OnExitEditMode()
        {
            _lastValue = default(T);
            _getter = null;
            _setter = null;
        }

        [ReadOnly]
        [NonSerialized] private T _lastValue;
#endif

        #endregion
    }
}