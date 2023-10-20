using MobX.Mediator.Values;
using MobX.Utilities.Editor.Inspector.PropertyDrawer;

namespace Mobx.Mediator.Editor
{
    [UnityEditor.CustomPropertyDrawer(typeof(SerializedValueAsset<>), true)]
    public class SerializedPropertyDrawer : InlineInspectorDrawer
    {
    }
}
