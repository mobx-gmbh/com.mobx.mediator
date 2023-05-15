using MobX.Utilities;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using System.Collections.Generic;
using UnityEngine;

namespace MobX.Mediator.Collections
{
    public abstract class RuntimeCollectionAsset<T> : CollectionAsset, IOnEnterEditMode
    {
        [Foldout("Options")]
        [SerializeField] private bool logLeaks = true;
        [SerializeField] private bool clearLeaks = true;

        protected RuntimeCollectionAsset()
        {
            EngineCallbacks.AddEnterEditModeListener(this);
        }

        protected private abstract int CountInternal { get; }
        protected private abstract IEnumerable<T> CollectionInternal { get; }

        public void OnEnterEditMode()
        {
            if (logLeaks && CountInternal > 0)
            {
                Debug.LogWarning("Collection",
                    $"Leak detected in runtime collection: {name}\n{CollectionInternal.ToCollectionString()}", this);
            }

            if (clearLeaks && CountInternal > 0)
            {
                ClearInternal();
            }
        }

        [Button("Clear")]
        [Foldout("Options")]
        protected private abstract void ClearInternal();
    }
}
