using MobX.Mediator.Callbacks;
using MobX.Utilities.Inspector;
using UnityEngine;

namespace MobX.Mediator.Enums
{
    public abstract class EnumAsset<T> : ScriptableAsset where T : EnumAsset<T>
    {
        [ReadonlyInspector] [SerializeField] private int hash;

        public static T None => none ??= CreateInstance<T>();

        private static T none;
    }
}