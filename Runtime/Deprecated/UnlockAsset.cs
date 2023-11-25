using MobX.Mediator.Callbacks;
using MobX.Mediator.Events;
using MobX.Utilities;
using MobX.Utilities.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MobX.Mediator.Deprecated
{
    public abstract class UnlockAsset<T> : MediatorAsset where T : IKey
    {
        #region Inspector

        [Foldout("Options")]
        [Tooltip("When enabled, leaks that occur when exiting playmode will logged to the console")]
        [SerializeField] private bool logLeaks = true;
        [Tooltip("When enabled, leaks that occur when exiting playmode will be cleared automatically")]
        [SerializeField] private bool clearLeaks = true;

        [ReadonlyInspector]
        private readonly HashSet<T> _keys = new(4);
        private readonly IBroadcast _unlockedEvent = new Broadcast();
        private readonly IBroadcast _lockedEvent = new Broadcast();

        #endregion


        #region Public

        public event Action Locked
        {
            add
            {
                _lockedEvent.Add(value);
                if (IsUnlocked() is false)
                {
                    value();
                }
            }
            remove => _lockedEvent.Remove(value);
        }

        public event Action Unlocked
        {
            add
            {
                _unlockedEvent.Add(value);
                if (IsUnlocked())
                {
                    value();
                }
            }
            remove => _unlockedEvent.Remove(value);
        }

        public bool IsUnlocked()
        {
            return _keys.Any();
        }

        public bool IsObjectRegisteredKey(T key)
        {
            return _keys.Contains(key);
        }

        public bool Unlock(T key)
        {
            var wasAdded = _keys.Add(key);
            if (wasAdded && _keys.Count == 1)
            {
                _unlockedEvent.Raise();
            }
            return wasAdded;
        }

        public bool Lock(T key)
        {
            var wasRemoved = _keys.Remove(key);
            if (wasRemoved && _keys.Count == 0)
            {
                _lockedEvent.Raise();
            }
            return wasRemoved;
        }

        public int ReleaseAll(bool discrete = false)
        {
            var count = _keys.Count;
            _keys.Clear();
            if (count > 0 && discrete is false)
            {
                _lockedEvent.Raise();
            }
            return count;
        }

        #endregion


        #region Operator

        public static explicit operator bool(UnlockAsset<T> lockAsset)
        {
            return lockAsset.IsUnlocked();
        }

        #endregion


        #region Setup

        [CallbackOnEnterEditMode]
        public void OnEnterEditMode()
        {
            if (logLeaks && _keys.Count > 0)
            {
                Debug.LogWarning("Unlock Asset!",
                    $"Leak detected in lock collection: {name}\n{_keys.ToCollectionString()}", this);
            }

            if (clearLeaks && _keys.Count > 0)
            {
                ReleaseAll();
            }
        }

        #endregion
    }
}