using MobX.Mediator.Values;
using MobX.Utilities.Editor.Helper;
using UnityEngine;

namespace Mobx.Mediator.Editor
{
    [UnityEditor.CustomPropertyDrawer(typeof(SerializedValueAsset<>), true)]
    public class ValueAssetPropertyDrawer : UnityEditor.PropertyDrawer
    {
        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            return -2f;
        }

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null)
            {
                DrawDefaultInspector(property, label);
            }
            else
            {
                UnityEditor.EditorGUILayout.PropertyField(property, label);
            }
        }

        private static void DrawDefaultInspector(UnityEditor.SerializedProperty property, GUIContent label)
        {
            if (Application.isPlaying)
            {
                DrawRuntimeInspector(property, label);
            }
            else
            {
                DrawEditorInspector(property, label);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private static void DrawEditorInspector(UnityEditor.SerializedProperty property, GUIContent label)
        {
            var serializedObject = new UnityEditor.SerializedObject(property.objectReferenceValue);
            UnityEditor.SerializedProperty valueProperty = serializedObject.FindProperty("value");

            serializedObject.Update();

            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.PropertyField(valueProperty, label);
            GUIHelper.BeginIndentOverride(0);
            UnityEditor.EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.MaxWidth(100));
            GUIHelper.EndIndentOverride();
            UnityEditor.EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawRuntimeInspector(UnityEditor.SerializedProperty property, GUIContent label)
        {
            var serializedObject = new UnityEditor.SerializedObject(property.objectReferenceValue);

            UnityEditor.SerializedProperty valueProperty = serializedObject.FindProperty("value");
            UnityEditor.SerializedProperty cachedProperty = serializedObject.FindProperty("cached");

            serializedObject.Update();

            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.PropertyField(valueProperty, label);
            GUI.enabled = false;
            GUIHelper.BeginIndentOverride(0);
            UnityEditor.EditorGUILayout.PropertyField(cachedProperty, GUIContent.none, GUILayout.MaxWidth(100));
            GUIHelper.EndIndentOverride();
            GUI.enabled = true;
            UnityEditor.EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
