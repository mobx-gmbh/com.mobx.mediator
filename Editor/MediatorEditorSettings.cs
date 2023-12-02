﻿using MobX.Mediator.Generation;
using MobX.Utilities.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mobx.Mediator.Editor
{
    [UnityEditor.FilePathAttribute("ProjectSettings/MediatorEditorSettings.asset",
        UnityEditor.FilePathAttribute.Location.ProjectFolder)]
    [UnityEditor.InitializeOnLoadAttribute]
    public class MediatorEditorSettings : UnityEditor.ScriptableSingleton<MediatorEditorSettings>
    {
        [SerializeField] private string fallbackSuffix = "Mediator";
        [SerializeField] private Map<MediatorType, string> mediatorTypeSuffix = new()
        {
            {MediatorType.None, "Missing"},
            {MediatorType.ValueAsset, "ValueAsset"},
            {MediatorType.EventAsset, "Event"},
            {MediatorType.PoolAsset, "Pool"},
            {MediatorType.RequestAsset, "Request"},
            {MediatorType.ListAsset, "List"},
            {MediatorType.ArrayAsset, "Array"},
            {MediatorType.HashSetAsset, "HashSet"},
            {MediatorType.SetAsset, "Set"},
            {MediatorType.StackAsset, "Stack"},
            {MediatorType.QueueAsset, "Queue"},
            {MediatorType.DictionaryAsset, "Dictionary"},
            {MediatorType.MapAsset, "Map"},
            {MediatorType.ValueAssetSerialized, "SerializedAsset"},
            {MediatorType.ValueAssetRuntime, "RuntimeAsset"},
            {MediatorType.ValueAssetPersistent, "PersistentAsset"},
            {MediatorType.ValueAssetProperty, "PropertyAsset"}
        };

        [SerializeField] private Texture2D fallbackIcon;
        [SerializeField] private Map<MediatorType, Texture2D> mediatorTypeIcons = new();

        public IReadOnlyDictionary<MediatorType, string> MediatorTypeSuffix => mediatorTypeSuffix;
        public string FallbackSuffix => fallbackSuffix;

        public IReadOnlyDictionary<MediatorType, Texture2D> MediatorTypeIcons => mediatorTypeIcons;
        public Texture2D FallbackIcon => fallbackIcon;

        public void SaveSettings()
        {
            Save(true);
        }

        static MediatorEditorSettings()
        {
            UnityEditor.EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
        }
    }
}