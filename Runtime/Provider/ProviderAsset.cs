using JetBrains.Annotations;
using MobX.Inspector;
using MobX.Mediator.Callbacks;
using MobX.Mediator.Events;
using MobX.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MobX.Mediator.Provider
{
    /// <summary>
    ///     Generic variant of a <see cref="ProviderAsset" />.
    ///     Use this for more control over what can and what cannot block.
    /// </summary>
    /// <typeparam name="T">Instances of this type can be used as lock</typeparam>
    public abstract class ProviderAsset<T> : MediatorAsset where T : IProvider
    {
        #region Inspector

        [Foldout("Options")]
        [Tooltip("When enabled, leaks that occur when exiting playmode will logged to the console")]
        [SerializeField] private bool logLeaks = true;
        [Tooltip("When enabled, leaks that occur when exiting playmode will be cleared automatically")]
        [SerializeField] private bool clearLeaks = true;

        [ReadOnly]
        private readonly HashSet<T> _providers = new(4);
        private readonly IBroadcast _firstAddedEvent = new Broadcast();
        private readonly IBroadcast _lastRemovedEvent = new Broadcast();

        #endregion


        #region Public

        /// <summary>
        ///     Event is invoked when the first provider is added.
        /// </summary>
        [PublicAPI]
        public event Action FirstAdded
        {
            add
            {
                _firstAddedEvent.Add(value);
                if (IsProvided())
                {
                    value();
                }
            }
            remove => _firstAddedEvent.Remove(value);
        }

        /// <summary>
        ///     Event is invoked when all provider were removed.
        /// </summary>
        [PublicAPI]
        public event Action LastRemoved
        {
            add
            {
                _lastRemovedEvent.Add(value);
                if (IsProvided() is false)
                {
                    value();
                }
            }
            remove => _lastRemovedEvent.Remove(value);
        }

        /// <summary>
        ///     Returns true if any object is currently registered as a provider.
        /// </summary>
        [PublicAPI]
        public bool IsProvided()
        {
            return _providers.Any();
        }

        /// <summary>
        ///     Returns true if the passed object is registered.
        /// </summary>
        [PublicAPI]
        public bool IsObjectProviding(T potentialLock)
        {
            return _providers.Contains(potentialLock);
        }

        /// <summary>
        ///     Add a new object to the list of locks. An object can only be added once as a lock!
        /// </summary>
        /// <returns>true if the object was added, false if it was already added</returns>
        [PublicAPI]
        public bool AddProvider(T lockInstance)
        {
            var wasAdded = _providers.Add(lockInstance);
            if (wasAdded && _providers.Count == 1)
            {
                _firstAddedEvent.Raise();
            }
            return wasAdded;
        }

        /// <summary>
        ///     Remove an object from the list of locks. An object can only be added once as a lock!
        /// </summary>
        /// <returns>true if the object was removed, false if it was not an active lock</returns>
        [PublicAPI]
        public bool RemoveProvider(T lockInstance)
        {
            var wasRemoved = _providers.Remove(lockInstance);
            if (wasRemoved && _providers.Count == 0)
            {
                _lastRemovedEvent.Raise();
            }
            return wasRemoved;
        }

        /// <summary>
        ///     Remove all providing objects and release the lock.
        /// </summary>
        /// <param name="discrete">When true, no events are raised.</param>
        /// <returns>the amount of removed provider</returns>
        [PublicAPI]
        public int ReleaseAll(bool discrete = false)
        {
            var count = _providers.Count;
            _providers.Clear();
            if (count > 0 && discrete is false)
            {
                _lastRemovedEvent.Raise();
            }
            return count;
        }

        #endregion


        #region Operator

        public static explicit operator bool(ProviderAsset<T> providerAsset)
        {
            return providerAsset.IsProvided();
        }

        #endregion


        #region Setup

        [CallbackOnEnterEditMode]
        private void OnEnterEditMode()
        {
            if (logLeaks && _providers.Count > 0)
            {
                Debug.LogWarning("Provider Asset!",
                    $"Leak detected in provider collection: {name}\n{_providers.ToCollectionString()}", this);
            }

            if (clearLeaks && _providers.Count > 0)
            {
                ReleaseAll();
            }
        }

        #endregion
    }

    public abstract class ProviderAsset : MediatorAsset
    {
        #region Inspector

        [Foldout("Options")]
        [Tooltip("When enabled, leaks that occur when exiting playmode will logged to the console")]
        [SerializeField] private bool logLeaks = true;
        [Tooltip("When enabled, leaks that occur when exiting playmode will be cleared automatically")]
        [SerializeField] private bool clearLeaks = true;

        [ReadOnly]
        private readonly HashSet<object> _providers = new(4);
        private readonly IBroadcast _firstAddedEvent = new Broadcast();
        private readonly IBroadcast _lastRemovedEvent = new Broadcast();

        #endregion


        #region Public

        /// <summary>
        ///     Event is invoked when the first provider is added.
        /// </summary>
        [PublicAPI]
        public event Action FirstAdded
        {
            add
            {
                _firstAddedEvent.Add(value);
                if (HasProvider())
                {
                    value();
                }
            }
            remove => _firstAddedEvent.Remove(value);
        }

        /// <summary>
        ///     Event is invoked when all provider were removed.
        /// </summary>
        [PublicAPI]
        public event Action LastRemoved
        {
            add
            {
                _lastRemovedEvent.Add(value);
                if (HasProvider() is false)
                {
                    value();
                }
            }
            remove => _lastRemovedEvent.Remove(value);
        }

        /// <summary>
        ///     Returns true if any object is currently registered as a provider.
        /// </summary>
        [PublicAPI]
        public bool HasProvider()
        {
            return _providers.Any();
        }

        /// <summary>
        ///     Returns true if the passed object is registered.
        /// </summary>
        [PublicAPI]
        public bool IsObjectProviding(object potentialLock)
        {
            return _providers.Contains(potentialLock);
        }

        /// <summary>
        ///     Add a new object to the list of locks. An object can only be added once as a lock!
        /// </summary>
        /// <returns>true if the object was added, false if it was already added</returns>
        [PublicAPI]
        public bool AddProvider(object lockInstance)
        {
            var wasAdded = _providers.Add(lockInstance);
            if (wasAdded && _providers.Count == 1)
            {
                _firstAddedEvent.Raise();
            }
            return wasAdded;
        }

        /// <summary>
        ///     Remove an object from the list of locks. An object can only be added once as a lock!
        /// </summary>
        /// <returns>true if the object was removed, false if it was not an active lock</returns>
        [PublicAPI]
        public bool RemoveProvider(object lockInstance)
        {
            var wasRemoved = _providers.Remove(lockInstance);
            if (wasRemoved && _providers.Count == 0)
            {
                _lastRemovedEvent.Raise();
            }
            return wasRemoved;
        }

        /// <summary>
        ///     Remove all providing objects and release the lock.
        /// </summary>
        /// <param name="discrete">When true, no events are raised.</param>
        /// <returns>the amount of removed provider</returns>
        [PublicAPI]
        public int ReleaseAll(bool discrete = false)
        {
            var count = _providers.Count;
            _providers.Clear();
            if (count > 0 && discrete is false)
            {
                _lastRemovedEvent.Raise();
            }
            return count;
        }

        #endregion


        #region Operator

        public static explicit operator bool(ProviderAsset providerAsset)
        {
            return providerAsset.HasProvider();
        }

        #endregion


        #region Setup

        [CallbackOnEnterEditMode]
        private void OnEnterEditMode()
        {
            if (logLeaks && _providers.Count > 0)
            {
                Debug.LogWarning("Provider Asset!",
                    $"Leak detected in provider collection: {name}\n{_providers.ToCollectionString()}", this);
            }

            if (clearLeaks && _providers.Count > 0)
            {
                ReleaseAll();
            }
        }

        #endregion
    }
}