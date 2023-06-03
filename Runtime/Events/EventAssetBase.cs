﻿using MobX.Utilities.Inspector;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MobX.Mediator.Events
{
    public abstract class EventAssetBase : MediatorAsset
    {
        [Tooltip("When enabled, the event is meant to be used during runtime only.")]
        [SerializeField] private bool runtimeOnly;

        [ConditionalShow(nameof(runtimeOnly))]
        [Tooltip("When enabled, non runtime calls are omitted.")]
        [SerializeField] private bool omitCalls = true;

        [ConditionalShow(nameof(runtimeOnly))]
        [Tooltip("When enabled, leaks are logged when transitioning from play to edit mode.")]
        [SerializeField] private bool logRuntimeLeaks = true;

        [ConditionalShow(nameof(runtimeOnly))]
        [Tooltip("When enabled, leaks are cleared when transitioning from play to edit mode.")]
        [SerializeField] private bool clearRuntimeLeaks = true;

        public bool RuntimeOnly => runtimeOnly;
        public bool OmitCalls => omitCalls;
        public bool LogRuntimeLeaks => logRuntimeLeaks;
        public bool ClearRuntimeLeaks => clearRuntimeLeaks;

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool IsIllegalCall(EventAssetBase target, [CallerMemberName] string callerName = "")
        {
            if (target.RuntimeOnly && Application.isPlaying is false && target.OmitCalls)
            {
                Debug.LogWarning("Event",
                    $"Edit time ({callerName}) method call omitted in runtime only event asset ({target.name})",
                    target);
                return true;
            }

            return false;
        }
#endif
    }
}