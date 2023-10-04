using JetBrains.Annotations;
using MobX.Utilities.Inspector;
using UnityEngine;

namespace MobX.Mediator.Enums
{
    public abstract class EnumAsset<T> : ScriptableObject where T : EnumAsset<T>
    {
        [ReadonlyInspector] [SerializeField] private int hash;

        [TextArea(0, 6)]
        [DrawLineAfter]
        [UsedImplicitly]
        [SerializeField] private string description;

        /// <summary>
        ///     An optional description of the value.
        /// </summary>
        public string Description => description;

        public static T None => none ??= CreateInstance<T>();

        private static T none;
    }
}