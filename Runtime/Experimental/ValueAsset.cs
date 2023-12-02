﻿using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System;

namespace MobX.Mediator.Experimental
{
    /// <summary>
    ///     A flexible readable and writable ValueAsset.
    ///     Instances of this type can be used as a serialized, runtime, persistent or property value.
    /// </summary>
    /// <typeparam name="TValue">The type of the contained value</typeparam>
    public abstract partial class ValueAsset<TValue> : ValueAssetRW<TValue>, IValueAsset<TValue>,
        IPropertyAsset<TValue>, IPersistentDataAsset<TValue>
    {
        #region Properties

        /// <summary>
        ///     Get or set the value of the asset.
        /// </summary>
        [PublicAPI]
        [ShowInInspector]
        [HideIf(nameof(type), Type.Serialized)]
        public override TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        /// <summary>
        ///     Called when the value was changed.
        /// </summary>
        [PublicAPI]
        public event Action<TValue> Changed
        {
            add => _changedEvent.Add(value);
            remove => _changedEvent.Remove(value);
        }

        /// <summary>
        ///     Set this object dirty, saving it and raising changed events.
        /// </summary>
        [PublicAPI]
        public void Save()
        {
            SaveInternal();
        }

        #endregion


        #region Property Binding

        [PublicAPI]
        public void BindGetter(Func<TValue> getter)
        {
            BindGetterInternal(getter);
        }

        [PublicAPI]
        public void ReleaseGetter(Func<TValue> getter)
        {
            ReleaseGetterInternal(getter);
        }

        [PublicAPI]
        public void BindSetter(Action<TValue> setter)
        {
            BindSetterInternal(setter);
        }

        [PublicAPI]
        public void ReleaseSetter(Action<TValue> setter)
        {
            ReleaseSetterInternal(setter);
        }

        #endregion
    }
}