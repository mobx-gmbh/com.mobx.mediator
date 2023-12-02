using MobX.Inspector;
using MobX.Mediator.Callbacks;
using MobX.Mediator.Deprecated;
using MobX.Serialization;
using MobX.Utilities.Types;
using Sirenix.OdinInspector;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace MobX.Mediator.Experimental
{
    public abstract class ValueAssetPersistent<TValue> : ValueAssetRW<TValue>, IValueAsset<TValue>,
        IPersistentDataAsset<TValue>
    {
        [Line]
        [LabelText("Default Value")]
        [SerializeField] private TValue defaultPersistentValue;
        [SerializeField] private RuntimeGUID guid;
        [Tooltip("The level to store the data on. Either profile specific or shared between profiles")]
        [SerializeField] private StorageLevel storageLevel = StorageLevel.Profile;
        [Tooltip("When enabled, the value is always saved when updated")]
        [SerializeField] private bool autoSave = true;

        [NonSerialized] private ManagedStorage<TValue> _persistentValue = new();

        [ShowInInspector]
        public override TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public override void SetValue(TValue value)
        {
            _persistentValue.value = value;
            if (autoSave)
            {
                SavePersistentData();
            }
        }

        public override TValue GetValue()
        {
            return _persistentValue is not null
                ? _persistentValue.value
                : defaultPersistentValue;
        }


        #region Initialization

        [CallbackOnInitialization]
        private void Initialize()
        {
            Assert.IsTrue(FileSystem.IsInitialized);
            LoadPersistentData();
        }

        [CallbackOnApplicationQuit]
        private void Shutdown()
        {
            SavePersistentData();
        }

        #endregion


        #region Persistent Data

        private IProfile Profile => storageLevel switch
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
            var profilePath = Profile.FolderName;
            var folderPath = Path.Combine(dataPath, systemPath, profilePath);
            Application.OpenURL(folderPath);
        }

        [Button("Save")]
        [ButtonGroup("Persistent")]
        public void SavePersistentData()
        {
            Profile.Store(Key, _persistentValue);
            Profile.SaveFile(Key);
        }

        [Button("Load")]
        [ButtonGroup("Persistent")]
        public void LoadPersistentData()
        {
            _persistentValue = Profile.HasFile(Key)
                ? Profile.Get<ManagedStorage<TValue>>(Key)
                : new ManagedStorage<TValue>(defaultPersistentValue);
        }

        [Button("Reset")]
        [ButtonGroup("Persistent")]
        public void ResetPersistentData()
        {
            _persistentValue = new ManagedStorage<TValue>(defaultPersistentValue);
            SavePersistentData();
        }

        #endregion
    }
}