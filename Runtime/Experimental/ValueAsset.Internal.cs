using MobX.Inspector;
using MobX.Mediator.Callbacks;
using MobX.Mediator.Deprecated;
using MobX.Mediator.Events;
using MobX.Serialization;
using MobX.Utilities.Types;
using Sirenix.OdinInspector;
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

        [Line]
        [SerializeField] private Type type;
        [SerializeField] private bool enableEvents = true;

        // Serialized Value
        [Line]
        [LabelText("@name", NicifyText = true)]
        [ShowIf(nameof(type), Type.Serialized)]
        [SerializeField] private TValue serializedValue;

        // Runtime Value
        [Line]
        [LabelText("Default Value")]
        [ShowIf(nameof(type), Type.Runtime)]
        [DisableIf("@UnityEngine.Application.isPlaying")]
        [Tooltip("The default value for the runtime value. The runtime value will be reset to this value")]
        [OnValueChanged(nameof(OnUpdateRuntimeValue))]
        [SerializeField] private TValue defaultRuntimeValue;

        // Persistent Value
        [Line]
        [ShowIf(nameof(type), Type.Persistent)]
        [LabelText("Default Value")]
        [SerializeField] private TValue defaultPersistentValue;
        [ShowIf(nameof(type), Type.Persistent)]
        [SerializeField] private RuntimeGUID guid;
        [ShowIf(nameof(type), Type.Persistent)]
        [Tooltip("The level to store the data on. Either profile specific or shared between profiles")]
        [SerializeField] private StorageLevel storageLevel = StorageLevel.Profile;
        [Tooltip("When enabled, the value is always saved when updated")]
        [ShowIf(nameof(type), Type.Persistent)]
        [SerializeField] private bool autoSave = true;

        // Property
        [Line]
        [ShowIf(nameof(type), Type.Property)]
        [SerializeField] private bool logPropertyWarnings;
        [ShowInInspector]
        [PropertyOrder(2)]
        [ShowIf(nameof(type), Type.Property)]
        private bool HasSetter => _setter != null;
        [ShowInInspector]
        [PropertyOrder(2)]
        [ShowIf(nameof(type), Type.Property)]
        private bool HasGetter => _getter != null;

        #endregion


        #region Fields

        [NonSerialized] private TValue _runtimeValue;
        [NonSerialized] private ManagedStorage<TValue> _persistentValue = new();

        private Func<TValue> _getter;
        private Action<TValue> _setter;

        private readonly IBroadcast<TValue> _changedEvent = new Broadcast<TValue>();

        #endregion


        #region Initialization

        [CallbackOnInitialization]
        private void Initialize()
        {
            switch (type)
            {
                case Type.Serialized:
                    break;
                case Type.Runtime:
                    _runtimeValue = defaultRuntimeValue;
                    break;
                case Type.Persistent:
                    Assert.IsTrue(FileSystem.IsInitialized);
                    LoadPersistentData();
                    break;
                case Type.Property:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [CallbackOnApplicationQuit]
        private void Shutdown()
        {
            switch (type)
            {
                case Type.Serialized:
                    break;
                case Type.Runtime:
                    _runtimeValue = defaultRuntimeValue;
                    break;
                case Type.Persistent:
                    SavePersistentData();
                    break;
                case Type.Property:
                    _setter = null;
                    _getter = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion


        #region Get & Set

        public override TValue GetValue()
        {
            return type switch
            {
                Type.Serialized => serializedValue,
                Type.Runtime => _runtimeValue,
                Type.Persistent => _persistentValue is not null ? _persistentValue.value : defaultPersistentValue,
                Type.Property => GetPropertyValue(),
                var _ => throw new InvalidOperationException()
            };

            TValue GetPropertyValue()
            {
                if (_getter is null)
                {
#if DEBUG
                    if (logPropertyWarnings)
                    {
                        Debug.LogWarning("Value Asset", "Property getter is not set!", this);
                    }
#endif
                    return default(TValue);
                }
                return _getter();
            }
        }

        public override void SetValue(TValue value)
        {
            var raiseChangedEvent = enableEvents && _changedEvent.Count > 0 && AreNotEqual(Value, value);

            switch (type)
            {
                case Type.Serialized:
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning("Value Asset", "Setting a serialized value during runtime!", this);
                        return;
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
                    _runtimeValue = value;
                    break;

                case Type.Persistent:
                    _persistentValue.value = value;
                    if (autoSave)
                    {
                        SavePersistentData();
                    }
                    break;

                case Type.Property:
                    if (_setter is null)
                    {
#if DEBUG
                        if (logPropertyWarnings)
                        {
                            Debug.LogWarning("Mediator", "Property setter is not set!", this);
                        }
#endif
                        break;
                    }
                    _setter.Invoke(value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SaveInternal()
        {
            switch (type)
            {
#if UNITY_EDITOR
                case Type.Serialized:
                    UnityEditor.EditorUtility.SetDirty(this);
                    break;
#endif
                case Type.Persistent:
                    SavePersistentData();
                    break;

                case Type.Property:
                    _changedEvent.Raise(Value);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AreEqual(in TValue first, in TValue second)
        {
            return EqualityComparer<TValue>.Default.Equals(first, second);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AreNotEqual(in TValue first, in TValue second)
        {
            return !EqualityComparer<TValue>.Default.Equals(first, second);
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
            StorageLevel.SharedProfile => FileSystem.SharedProfile,
            var _ => throw new ArgumentOutOfRangeException()
        };

        private string Key => guid.ToString();

        [Line]
        [Button]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 8)]
        [ShowIf(nameof(type), Type.Persistent)]
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
        [ShowIf(nameof(type), Type.Persistent)]
        public void SavePersistentData()
        {
            Profile.Store(Key, _persistentValue);
            Profile.SaveFile(Key);
        }

        [Button("Load")]
        [ButtonGroup("Persistent")]
        [ShowIf(nameof(type), Type.Persistent)]
        public void LoadPersistentData()
        {
            _persistentValue = Profile.HasFile(Key)
                ? Profile.Get<ManagedStorage<TValue>>(Key)
                : new ManagedStorage<TValue>(defaultPersistentValue);
        }

        [Button("Reset")]
        [ButtonGroup("Persistent")]
        [ShowIf(nameof(type), Type.Persistent)]
        public void ResetPersistentData()
        {
            _persistentValue = new ManagedStorage<TValue>(defaultPersistentValue);
            SavePersistentData();
        }

        #endregion


        #region Editor

#if UNITY_EDITOR
        private void OnUpdateRuntimeValue()
        {
            if (Application.isPlaying is false)
            {
                _runtimeValue = defaultRuntimeValue;
            }
        }

        [CallbackOnEnterPlayMode]
        private void OnEnterPlayMode()
        {
            Initialize();
        }

        [CallbackOnEnterPlayMode]
        private void OnExitPlayMode()
        {
            Shutdown();
        }
#endif

        #endregion


        #region Enable & Disable

        protected override void OnEnable()
        {
            _persistentValue = new ManagedStorage<TValue>(defaultPersistentValue);
#if UNITY_EDITOR
            RuntimeGUID.Create(this, ref guid);
#endif
            base.OnEnable();
        }

        #endregion
    }
}