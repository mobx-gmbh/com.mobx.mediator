using MobX.Utilities.Inspector;
using System;

namespace MobX.Mediator.Experimental
{
    public abstract partial class ValueAsset<TValue> : ValueAssetRO<TValue>, IValueAsset<TValue>, IPropertyAsset<TValue>
    {
        #region Properties

        /// <summary>
        ///     Get or set the value of the asset.
        /// </summary>
        [ReadonlyInspector]
        [ConditionalHide(nameof(type), Type.Serialized)]
        public new TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        /// <summary>
        ///     Called when the value was changed.
        /// </summary>
        public event Action<TValue> Changed
        {
            add => _changedEvent.Add(value);
            remove => _changedEvent.Remove(value);
        }

        #endregion


        #region Property Binding

        public void BindGetter(Func<TValue> getter)
        {
            BindGetterInternal(getter);
        }

        public void ReleaseGetter(Func<TValue> getter)
        {
            ReleaseGetterInternal(getter);
        }

        public void BindSetter(Action<TValue> setter)
        {
            BindSetterInternal(setter);
        }

        public void ReleaseSetter(Action<TValue> setter)
        {
            ReleaseSetterInternal(setter);
        }

        #endregion
    }
}