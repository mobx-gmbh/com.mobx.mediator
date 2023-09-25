using JetBrains.Annotations;
using MobX.Utilities.Callbacks;
using MobX.Utilities.Inspector;
using UnityEngine;

namespace MobX.Mediator
{
    [SearchField(Enabled = false)]
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