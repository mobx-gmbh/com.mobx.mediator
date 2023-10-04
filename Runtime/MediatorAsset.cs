using JetBrains.Annotations;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using MobX.Utilities.Reflection;
using UnityEngine;

namespace MobX.Mediator
{
    [SearchField(Enabled = false)]
    [AddressablesGroup("Mediator")]
    public class MediatorAsset : ScriptableAsset
    {
#pragma warning disable
        [TextArea(0, 6)]
        [DrawLineAfter]
        [UsedImplicitly]
        [SerializeField] private string description;
#pragma warning restore
    }
}