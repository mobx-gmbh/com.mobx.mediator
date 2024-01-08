using MobX.Inspector;
using MobX.Mediator.Callbacks;
using MobX.Mediator.Events;
using MobX.Serialization;
using MobX.Utilities.Types;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MobX.Mediator.Values
{
    public abstract class ValueAssetSave<TValue> : ValueAssetRW<TValue>, IValueAsset<TValue>,
        IPersistentDataAsset<TValue>, ISavable
    {
        [Line]
        [LabelText("Default Value")]
        [SerializeField] private TValue defaultPersistentValue;
        [SerializeField] private RuntimeGUID guid;
        [Tooltip("The level to store the data on. Either profile specific or shared between profiles")]
        [SerializeField] private StorageLevel storageLevel = StorageLevel.Profile;
        [Tooltip("When enabled, the value is always saved when updated")]
        [SerializeField] private bool autoSave = true;

        [NonSerialized] private TValue _persistentValue;
        [NonSerialized] private readonly Broadcast<TValue> _changedEvent = new();
        [NonSerialized] private StoreOptions _storeOptions;

        [ShowInInspector]
        public override TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public override void SetValue(TValue value)
        {
            var oldValue = _persistentValue;
            _persistentValue = value;
            if (autoSave)
            {
                SavePersistentData();
            }
            if (EqualityComparer<TValue>.Default.Equals(oldValue, value))
            {
                return;
            }
            _changedEvent.Raise(value);
        }

        public override TValue GetValue()
        {
            return _persistentValue;
        }

        /// <summary>
        ///     Called when the value was changed.
        /// </summary>
        public override event Action<TValue> Changed
        {
            add => _changedEvent.Add(value);
            remove => _changedEvent.Remove(value);
        }


        #region Initialization

        protected override void OnEnable()
        {
            _storeOptions = new StoreOptions(name);
            if (FileSystem.IsInitialized)
            {
                LoadPersistentData();
            }
            base.OnEnable();
        }

        [CallbackOnInitialization]
        private void Initialize()
        {
            LoadPersistentData();
        }

        [CallbackOnApplicationQuit]
        private void Shutdown()
        {
            SavePersistentData();
        }

        #endregion


        #region Persistent Data

        private ISaveProfile Profile => storageLevel switch
        {
            StorageLevel.Profile => FileSystem.Profile,
            StorageLevel.SharedProfile => FileSystem.SharedProfile,
            var _ => throw new ArgumentOutOfRangeException()
        };

        private string Key => guid.ToString();

        [Line]
        [Button]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 8)]
        private void OpenInFileSystem()
        {
            var dataPath = Application.persistentDataPath;
            var systemPath = FileSystem.RootFolder;
            var profilePath = Profile.Info.FolderName;
            var folderPath = Path.Combine(dataPath, systemPath, profilePath);
            Application.OpenURL(folderPath);
        }

        public void Save()
        {
            SavePersistentData();
        }

        [Button("Save")]
        [ButtonGroup("Persistent")]
        public void SavePersistentData()
        {
            Profile.SaveFile(Key, _persistentValue, _storeOptions);
        }

        [Button("Load")]
        [ButtonGroup("Persistent")]
        public void LoadPersistentData()
        {
            if (!Profile.TryLoadFile(Key, out _persistentValue, _storeOptions))
            {
                _persistentValue = defaultPersistentValue;
            }
        }

        [Button("Reset")]
        [ButtonGroup("Persistent")]
        public void ResetPersistentData()
        {
            _persistentValue = defaultPersistentValue;
            SavePersistentData();
        }

        #endregion
    }
}