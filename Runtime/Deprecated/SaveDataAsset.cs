﻿using MobX.Inspector;
using MobX.Mediator.Events;
using MobX.Mediator.Values;
using MobX.Serialization;
using MobX.Utilities.Types;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MobX.Mediator.Deprecated
{
    /// <summary>
    ///     Asset represents a value that can be saved and loaded to the persistent data storage.
    /// </summary>
    /// <typeparam name="T">The type of the saved value. Must be serializable!</typeparam>
    public abstract class SaveDataAsset<T> : ValueAsset<T>, ISaveDataAsset
    {
        #region Inspector & Data

        [NonSerialized] private Storage<T> _storage;
        [Foldout("Save Data")]
        [SerializeField] private RuntimeGUID guid;
        [SerializeField] private T defaultValue;
        [Tooltip("The level to store the data on. Either profile specific or shared between profiles")]
        [SerializeField] private StorageLevel storageLevel = StorageLevel.Profile;
        [Tooltip("The key used to save the value. Only use asset name for debugging!")]
        [SerializeField] private KeyType key;
        private readonly Broadcast<T> _changedEvent = new();

        /// <summary>
        ///     Event, called every time the value changed.
        /// </summary>
        public sealed override event Action<T> Changed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => _changedEvent.Add(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => _changedEvent.Remove(value);
        }

        public RuntimeGUID GUID => guid;

        private string Key => key switch
        {
            KeyType.AssetGUID => guid.ToString(),
            KeyType.AssetName => name,
            var _ => throw new ArgumentOutOfRangeException()
        };

        private IProfile Profile => storageLevel switch
        {
            StorageLevel.Profile => FileSystem.Profile,
            StorageLevel.SharedProfile => FileSystem.SharedProfile,
            _ => throw new ArgumentOutOfRangeException()
        };

        private enum KeyType
        {
            AssetGUID = 0,
            AssetName = 1
        }

        #endregion


        #region Value

        [Line(DrawTiming = DrawTiming.After)]
        [ShowInInspector]
        [Foldout("Save Data")]
        public override T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public override void SetValue(T newValue)
        {
            var isEqual = EqualityComparer<T>.Default.Equals(newValue, _storage.value);
            if (isEqual)
            {
                return;
            }
            ref var valueRef = ref GetValueRef();
            valueRef = newValue;
            Save();
            _changedEvent.Raise(newValue);
        }

        public override T GetValue()
        {
            return _storage.value;
        }

        protected ref T GetValueRef()
        {
            return ref _storage.value;
        }

        #endregion


        #region Setup

        protected override void OnEnable()
        {
            base.OnEnable();
            RuntimeGUID.Create(this, ref guid);
            FileSystem.InitializationCompleted += OnFileSystemInitialized;
            FileSystem.ProfileChanged += OnProfileChanged;

            if (FileSystem.IsInitialized)
            {
                Load();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            FileSystem.InitializationCompleted -= OnFileSystemInitialized;
            FileSystem.ProfileChanged -= OnProfileChanged;
        }

        private void OnFileSystemInitialized()
        {
            Load();
        }

        private void OnProfileChanged(IProfile profile)
        {
            Load();
        }

        #endregion


        #region IO Operations

        [Button]
        [ButtonGroup("Save Data/Buttons")]
        public void Save()
        {
            var profile = Profile;
            profile.StoreData(Key, _storage);
            profile.SaveFile(Key);
        }

        [Button]
        [ButtonGroup("Save Data/Buttons")]
        public void Load()
        {
            var profile = Profile;

            if (profile.HasFile(Key))
            {
                profile.ResolveData(Key, out _storage);
                return;
            }

            _storage = new Storage<T>(defaultValue);
        }

        [Button("Reset")]
        [ButtonGroup("Save Data/Buttons")]
        public void ResetData()
        {
            var profile = Profile;
            profile.DeleteEntry(Key);
            _storage = new Storage<T>(defaultValue);
        }

        #endregion
    }
}