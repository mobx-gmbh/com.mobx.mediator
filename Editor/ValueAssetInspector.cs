using MobX.Mediator.Values;
using MobX.Utilities.Editor.Inspector;

namespace Mobx.Mediator
{
#if !DISABLE_CUSTOM_INSPECTOR
    [UnityEditor.CustomEditor(typeof(ValueAsset), true)]
#endif
    public class ValueAssetInspector : OverrideInspector<ValueAsset>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            UnityEditor.EditorApplication.update += Repaint;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnityEditor.EditorApplication.update -= Repaint;
        }
    }
}
