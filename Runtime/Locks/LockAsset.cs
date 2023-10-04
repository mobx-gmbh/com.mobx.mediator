using MobX.Mediator.Events;
using MobX.Utilities;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MobX.Mediator.Locks
{
    public sealed class LockAsset : MediatorAsset
    {
        #region Inspector

        [Foldout("Options")]
        [Tooltip("When enabled, leaks that occur when exiting playmode will logged to the console")]
        [SerializeField] private bool logLeaks = true;
        [Tooltip("When enabled, leaks that occur when exiting playmode will be cleared automatically")]
        [SerializeField] private bool clearLeaks = true;

        [ReadonlyInspector]
        private readonly HashSet<object> _locks = new(4);
        private readonly IBroadcast _lockedEvent = new Broadcast();
        private readonly IBroadcast _unlockedEvent = new Broadcast();

        #endregion


        #region Public

        /// <summary>
        ///     Event is invoked when a locks is added.
        /// </summary>
        public event Action Locked
        {
            add => _lockedEvent.Add(value);
            remove => _lockedEvent.Remove(value);
        }

        /// <summary>
        ///     Event is invoked when all locks were removed.
        /// </summary>
        public event Action Unlocked
        {
            add => _unlockedEvent.Add(value);
            remove => _unlockedEvent.Remove(value);
        }

        /// <summary>
        ///     Returns true if any object is currently registered as a locks.
        /// </summary>
        public bool IsBlocked()
        {
            return _locks.Any();
        }

        /// <summary>
        ///     Returns true if the passed object is a registered locks.
        /// </summary>
        public bool IsObjectRegisteredLocks(object potentialLocks)
        {
            return _locks.Contains(potentialLocks);
        }

        /// <summary>
        ///     Add a new object to the list of locks. An object can only be added once as a locks!
        /// </summary>
        /// <returns>true if the object was added, false if it was already added</returns>
        public bool AddLocks(object lockInstance)
        {
            var wasAdded = _locks.Add(lockInstance);
            if (wasAdded && _locks.Count == 1)
            {
                _lockedEvent.Raise();
            }
            return wasAdded;
        }

        /// <summary>
        ///     Remove an object from the list of locks. An object can only be added once as a locks!
        /// </summary>
        /// <returns>true if the object was removed, false if it was not an active locks</returns>
        public bool RemoveLocks(object lockInstance)
        {
            var wasRemoved = _locks.Remove(lockInstance);
            if (wasRemoved && _locks.Count == 0)
            {
                _unlockedEvent.Raise();
            }
            return wasRemoved;
        }

        /// <summary>
        ///     Remove all blocking objects and release the locks.
        /// </summary>
        /// <param name="discrete">When true, the the unblocked event is not invoked.</param>
        /// <returns>the amount of removed objects</returns>
        public int ReleaseAll(bool discrete = false)
        {
            var count = _locks.Count;
            _locks.Clear();
            if (count > 0 && discrete is false)
            {
                _unlockedEvent.Raise();
            }
            return count;
        }

        #endregion


        #region Operator

        public static explicit operator bool(LockAsset lockAsset)
        {
            return lockAsset.IsBlocked();
        }

        #endregion


        #region Setup

        [CallbackOnEnterEditMode]
        private void OnEnterEditMode()
        {
            if (logLeaks && _locks.Count > 0)
            {
                Debug.LogWarning("Lock Asset!",
                    $"Leak detected in locks collection: {name}\n{_locks.ToCollectionString()}", this);
            }

            if (clearLeaks && _locks.Count > 0)
            {
                ReleaseAll();
            }
        }

        #endregion
    }
}