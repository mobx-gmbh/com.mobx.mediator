using MobX.Mediator.Events;
using MobX.Utilities;
using MobX.Utilities.Inspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobX.Mediator.Context
{
    public abstract class ContextAsset : MediatorAsset
    {
        #region Fields & Inspector

        [SerializeField] [Required] private ContextStack contextStack;

        private readonly IBroadcast _activated = new Broadcast();
        private readonly IBroadcast _deactivated = new Broadcast();

        private static readonly List<ContextAsset> activeContexts = new();

        #endregion


        #region Public

        /// <summary>
        ///     Returns true if the context is active and part of a context stack.
        /// </summary>
        [ReadonlyInspector]
        public bool IsActive => contextStack.Peek() == this;

        /// <summary>
        ///     Raised when the context is activated.
        /// </summary>
        public event Action Activated
        {
            add => _activated.Add(value);
            remove => _activated.Remove(value);
        }

        /// <summary>
        ///     Raised when the context is deactivated.
        /// </summary>
        public event Action Deactivated
        {
            add => _deactivated.Add(value);
            remove => _deactivated.Remove(value);
        }

        [Button]
        [DrawLine]
        public void Enable()
        {
            if (IsActive)
            {
                return;
            }

            if (contextStack.TryPeek(out var activeContext))
            {
                activeContext.DeactivateInternal();
            }
            ActivateInternal();
        }

        [Button]
        public void Disable()
        {
            if (IsActive is false)
            {
                contextStack.Remove(this);
                return;
            }
            DeactivateInternal();
            contextStack.Remove(this);
            activeContexts.Remove(this);
            if (contextStack.TryPeek(out var contextAsset))
            {
                contextAsset.ActivateInternal();
            }
        }

        #endregion


        #region Activation

        private void ActivateInternal()
        {
            contextStack.PushUnique(this);
            activeContexts.AddUnique(this);
            OnActivate();
            _activated.Raise();
        }

        #endregion


        #region Deactivation

        private void DeactivateInternal()
        {
            if (IsActive is false)
            {
                return;
            }
            OnDeactivate();
            _deactivated.Raise();
        }

        #endregion


        #region ToString

        public override string ToString()
        {
            return name;
        }

        #endregion


        #region Abstract

        protected abstract void OnActivate();

        protected abstract void OnDeactivate();

        #endregion
    }
}