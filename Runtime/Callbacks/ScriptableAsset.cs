﻿using JetBrains.Annotations;
using MobX.Inspector;
using MobX.Mediator.Utility;
using MobX.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;

namespace MobX.Mediator.Callbacks
{
    /// <summary>
    ///     Abstract base class for <see cref="ScriptableObject" />s that can receive <see cref="Gameloop" /> callbacks.
    ///     Use the <see cref="CallbackMethodAttribute" /> to receive custom callbacks on a target method.
    /// </summary>
    public abstract class ScriptableAsset : ScriptableObject
    {
        [Flags]
        private enum Options
        {
            None = 0,

            /// <summary>
            ///     When enabled, the asset will receive custom runtime and editor callbacks.
            /// </summary>
            ReceiveCallbacks = 1,

            /// <summary>
            ///     When enabled, a developer annotation field is displayed.
            /// </summary>
            Annotation = 2,

            /// <summary>
            ///     When enabled, changes to this asset during runtime are reset when entering edit mode.
            /// </summary>
            ResetRuntimeChanges = 4
        }

        [PropertySpace(0, 8)]
        [Tooltip(AssetOptionsTooltip)]
        [PropertyOrder(-10000)]
        [SerializeField] private Options assetOptions = Options.ReceiveCallbacks;

#pragma warning disable
        [Line(SpaceBefore = 0)]
        [TextArea(0, 6)]
        [UsedImplicitly]
        [ShowIf(nameof(ShowAnnotation))]
        [FormerlySerializedAs("description")]
        [PropertyOrder(-10000)]
        [SerializeField] private string annotation;
#pragma warning restore

        private const string AssetOptionsTooltip =
            "Receive Callbacks: When enabled, the asset will receive custom runtime and editor callbacks." +
            "Annotation: When enabled, a developer annotation field is displayed." +
            "ResetRuntimeChanges: When enabled, changes to this asset during runtime are reset when entering edit mode.";

        private bool ShowAnnotation => assetOptions.HasFlagUnsafe(Options.Annotation);

        [Conditional("UNITY_EDITOR")]
        public void Repaint()
        {
#if UNITY_EDITOR
            if (Gameloop.IsQuitting)
            {
                return;
            }

            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        protected virtual void OnEnable()
        {
            if (assetOptions.HasFlagUnsafe(Options.ReceiveCallbacks))
            {
                Gameloop.Register(this);
            }
        }

        protected virtual void OnDisable()
        {
            Gameloop.Unregister(this);
        }

        protected virtual void OnDestroy()
        {
            Gameloop.Unregister(this);
        }

        /// <summary>
        ///     Reset the asset to its default values.
        /// </summary>
        public void ResetAsset()
        {
            ScriptableAssetUtility.ResetAsset(this);
        }


        #region Editor

#if UNITY_EDITOR

        [NonSerialized] private string _json;

        [CallbackOnEnterPlayMode]
        private void OnEnterPlayMode()
        {
            if (assetOptions.HasFlagUnsafe(Options.ResetRuntimeChanges))
            {
                _json = ScriptableAssetUtility.GetAssetJSon(this);
            }
        }

        [CallbackOnExitPlayMode]
        private void OnExitPlayMode()
        {
            if (assetOptions.HasFlagUnsafe(Options.ResetRuntimeChanges))
            {
                ScriptableAssetUtility.SetAssetJSon(this, _json);
            }
        }

#endif

        #endregion
    }
}