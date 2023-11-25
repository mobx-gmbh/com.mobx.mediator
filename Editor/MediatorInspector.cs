using MobX.Mediator;
using MobX.Utilities.Editor.Inspector;

namespace Mobx.Mediator.Editor
{
    [UnityEditor.CustomEditor(typeof(MediatorAsset), true)]
    public class MediatorInspector : OverrideInspector<MediatorAsset>{}
}