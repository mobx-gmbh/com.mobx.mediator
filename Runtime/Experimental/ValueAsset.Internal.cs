using MobX.Mediator.Callbacks;
using MobX.Mediator.Deprecated;
using MobX.Mediator.Events;
using MobX.Serialization;
using MobX.Utilities.Inspector;
using MobX.Utilities.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace MobX.Mediator.Experimental
{
    public abstract partial class ValueAsset<TValue>
    {
        #region Inspector

        [Space]
        [SerializeField] private Type type;
        [Label("Value")]
        [ConditionalHide(nameof(type), Type.Persistent)]
        [SerializeField] private TValue serializedValue;
        [ConditionalShow(nameof(type), Type.Persistent)]
        [SerializeField] private TValue defaultValue;
        [SerializeField] private bool raiseChangedEvents = true;

        [DrawLine]
        [ConditionalShow(nameof(type), Type.Persistent)]
        [SerializeField] private RuntimeGUID guid;
        [ConditionalShow(nameof(type), Type.Persistent)]
        [Tooltip("The level to store the data on. Either profile specific or shared between profiles")]
        [SerializeField] private StorageLevel storageLevel = StorageLevel.Profile;

        #endregion


        #region Fields

        [NonSerialized] private TValue _value;
        [NonSerialized] private TValue _cache;

        [ConditionalShow(nameof(type), Type.Property)]
        private Func<TValue> _getter;
        [ConditionalShow(nameof(type), Type.Property)]
        private Action<TValue> _setter;

        private readonly IBroadcast<TValue> _changedEvent = new Broadcast<TValue>();

        #endregion


        #region Get & Set

        public override TValue GetValue()
        {
            return type switch
            {
                Type.Serialized => serializedValue,
                Type.Runtime => _value,
                Type.Persistent => _value,
                Type.Property => _getter is not null ? _getter() : default(TValue),
                var _ => throw new InvalidOperationException()
            };
        }

        public void SetValue(TValue value)
        {
            var raiseChangedEvent = raiseChangedEvents && _changedEvent.Count > 0 && AreEqual(Value, value);

            switch (type)
            {
                case Type.Serialized:
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning("Value Asset", "Setting a serialized value during runtime!", this);
                    }
#endif
                    serializedValue = value;
                    if (raiseChangedEvent)
                    {
                        _changedEvent.Raise(value);
                    }
                    break;

                case Type.Runtime:
                    if (raiseChangedEvent)
                    {
                        _changedEvent.Raise(value);
                    }
                    _value = value;
                    break;

                case Type.Persistent:
                    break;

                case Type.Property:
                    _setter?.Invoke(value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Save()
        {
            switch (type)
            {
#if UNITY_EDITOR
                case Type.Serialized:
                    UnityEditor.EditorUtility.SetDirty(this);
                    break;

                case Type.Runtime:
                    _cache = _value;
                    break;
#endif
                case Type.Persistent:
                    SavePersistentData();
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AreEqual(in TValue first, in TValue second)
        {
            return EqualityComparer<TValue>.Default.Equals(first, second);
        }

        #endregion


        #region Property Binding

        private void BindGetterInternal(Func<TValue> getter)
        {
            _getter = getter;
        }

        private void ReleaseGetterInternal(Func<TValue> getter)
        {
            Assert.AreEqual(getter, _getter);
            _getter = null;
        }

        private void BindSetterInternal(Action<TValue> setter)
        {
            _setter = setter;
        }

        private void ReleaseSetterInternal(Action<TValue> setter)
        {
            Assert.AreEqual(setter, _setter);
            _setter = null;
        }

        #endregion


        #region Persistent Data

        private IProfile Profile => storageLevel switch
        {
            StorageLevel.Profile => FileSystem.Profile,
            StorageLevel.Shared => FileSystem.SharedProfile,
            _ => throw new ArgumentOutOfRangeException()
        };

        [CallbackOnInitialization]
        private void InitializePersistentData()
        {
            if (type == Type.Persistent)
            {
                Assert.IsTrue(FileSystem.IsInitialized);
                // TODO: LoadPersistentData();
            }
        }

        [Button]
        [DrawLine]
        [ConditionalShow(nameof(type), Type.Persistent)]
        private void SavePersistentData()
        {
            var profile = Profile;
            profile.Store(guid.ToString(), _value);
            profile.SaveFile(guid.ToString());
            FileSystem.Save();
        }

        [Button]
        [ConditionalShow(nameof(type), Type.Persistent)]
        private void LoadPersistentData()
        {
            var profile = Profile;

            if (profile.HasFile(guid.ToString()))
            {
                _value = profile.Get<TValue>(guid.ToString());
            }
        }

        [Button]
        [ConditionalShow(nameof(type), Type.Persistent)]
        private void ResetPersistentData()
        {
            var profile = Profile;
            _value = defaultValue;
            profile.Store(guid.ToString(), _value);
            profile.SaveFile(guid.ToString());
            FileSystem.Save();
        }

        [Button]
        [DrawLine]
        [ConditionalShow(nameof(type), Type.Persistent)]
        private void OpenInFileSystem()
        {
            var dataPath = Application.persistentDataPath;
            var systemPath = FileSystem.RootFolder;
            var profilePath = Profile.FolderName;
            var folderPath = Path.Combine(dataPath, systemPath, profilePath);
            Application.OpenURL(folderPath);
        }

        #endregion


        #region Editor

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            switch (type)
            {
                case Type.Serialized:
                    break;
                case Type.Runtime:
                    if (Application.isPlaying is false)
                    {
                        _value = serializedValue;
                        _cache = _value;
                    }
                    break;
                case Type.Persistent:
                    break;
                case Type.Property:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [CallbackOnEnterPlayMode]
        private void OnEnterPlayMode()
        {
            _cache = _value;
        }

        [CallbackOnEnterPlayMode]
        private void OnExitPlayMode()
        {
            _value = _cache;
        }
#endif

        #endregion


        #region Enable & Disable

        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            RuntimeGUID.Create(this, ref guid);
#endif

            // TODO: Load data
        }

        #endregion
    }
}