using MobX.Mediator.Events;
using MobX.Utilities;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MobX.Mediator.Blocking
{
    public sealed class BlockerAsset : MediatorAsset, IOnEnterEditMode
    {
        #region Inspector

        [Foldout("Options")]
        [Tooltip("When enabled, leaks that occur when exiting playmode will logged to the console")]
        [SerializeField] private bool logLeaks = true;
        [Tooltip("When enabled, leaks that occur when exiting playmode will be cleared automatically")]
        [SerializeField] private bool clearLeaks = true;

        [ReadonlyInspector]
        private readonly HashSet<object> _blocker = new(4);
        private readonly IBroadcast _blockedEvent = new Broadcast();
        private readonly IBroadcast _unblockedEvent = new Broadcast();

        #endregion


        #region Public

        /// <summary>
        ///     Event is invoked when a blocker is added.
        /// </summary>
        public event Action Blocked
        {
            add => _blockedEvent.Add(value);
            remove => _blockedEvent.Remove(value);
        }

        /// <summary>
        ///     Event is invoked when all blocker were removed.
        /// </summary>
        public event Action Unblocked
        {
            add => _unblockedEvent.Add(value);
            remove => _unblockedEvent.Remove(value);
        }

        /// <summary>
        ///     Returns true if any object is currently registered as a blocker.
        /// </summary>
        public bool IsBlocked()
        {
            return _blocker.Any();
        }

        /// <summary>
        ///     Returns true if the passed object is a registered blocker.
        /// </summary>
        public bool IsObjectRegisteredBlocker(object potentialBlocker)
        {
            return _blocker.Contains(potentialBlocker);
        }

        /// <summary>
        ///     Add a new object to the list of blockers. An object can only be added once as a blocker!
        /// </summary>
        /// <returns>true if the object was added, false if it was already added</returns>
        public bool AddBlocker(object blocker)
        {
            var wasAdded = _blocker.Add(blocker);
            if (wasAdded && _blocker.Count == 1)
            {
                _blockedEvent.Raise();
            }
            return wasAdded;
        }

        /// <summary>
        ///     Remove an object from the list of blockers. An object can only be added once as a blocker!
        /// </summary>
        /// <returns>true if the object was removed, false if it was not an active blocker</returns>
        public bool RemoveBlocker(object blocker)
        {
            var wasRemoved = _blocker.Remove(blocker);
            if (wasRemoved && _blocker.Count == 0)
            {
                _unblockedEvent.Raise();
            }
            return wasRemoved;
        }

        /// <summary>
        ///     Remove all blocking objects and release the blocker.
        /// </summary>
        /// <param name="discrete">When true, the the unblocked event is not invoked.</param>
        /// <returns>the amount of removed objects</returns>
        public int ReleaseAll(bool discrete = false)
        {
            var count = _blocker.Count;
            _blocker.Clear();
            if (count > 0 && discrete is false)
            {
                _unblockedEvent.Raise();
            }
            return count;
        }

        #endregion


        #region Operator

        public static explicit operator bool(BlockerAsset blockerAsset)
        {
            return blockerAsset.IsBlocked();
        }

        #endregion


        #region Setup

        [CallbackMethod(Segment.EnteredEditMode)]
        public void OnEnterEditMode()
        {
            if (logLeaks && _blocker.Count > 0)
            {
                Debug.LogWarning("Blocker Asset!",
                    $"Leak detected in blocker collection: {name}\n{_blocker.ToCollectionString()}", this);
            }

            if (clearLeaks && _blocker.Count > 0)
            {
                ReleaseAll();
            }
        }

        #endregion
    }
}