using Mobx.Mediator.Editor.Generation;
using MobX.Utilities.Editor.Inspector;
using System.Collections.Generic;
using UnityEngine;
using GUIUtility = MobX.Utilities.Editor.Helper.GUIUtility;

namespace Mobx.Mediator.Editor
{
    public class MediatorSettingsProvider : UnityEditor.SettingsProvider
    {
        private FoldoutHandler Foldout { get; } = new(nameof(MediatorSettingsProvider));

        public MediatorSettingsProvider(string path, UnityEditor.SettingsScope scopes,
            IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            var serializedObject = new UnityEditor.SerializedObject(MediatorEditorSettings.instance);
            var mediatorTypeSuffixProperty = serializedObject.FindProperty("mediatorTypeSuffix");
            var mediatorTypeSuffixFallbackProperty = serializedObject.FindProperty("fallbackSuffix");
            var mediatorTypeIconProperty = serializedObject.FindProperty("mediatorTypeIcons");
            var mediatorTypeIconFallbackProperty = serializedObject.FindProperty("fallbackIcon");

            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(mediatorTypeSuffixFallbackProperty);
            UnityEditor.EditorGUILayout.PropertyField(mediatorTypeSuffixProperty);
            GUIUtility.Space();
            GUIUtility.DrawLine();
            GUIUtility.Space();
            UnityEditor.EditorGUILayout.PropertyField(mediatorTypeIconFallbackProperty);
            UnityEditor.EditorGUILayout.PropertyField(mediatorTypeIconProperty);
            serializedObject.ApplyModifiedProperties();

            MediatorEditorSettings.instance.SaveSettings();

            if (GUI.changed)
            {
                Foldout.SaveState();
            }

            GUIUtility.Space();
            GUIUtility.DrawLine();
            GUIUtility.Space();

            if (GUILayout.Button("Generate Mediator"))
            {
                MediatorTypeGeneration.GenerateMediatorClasses();
            }

            GUIUtility.Space();
        }

        [UnityEditor.SettingsProviderAttribute]
        public static UnityEditor.SettingsProvider CreateSettingsProvider()
        {
            return new MediatorSettingsProvider("Project/Mediator", UnityEditor.SettingsScope.Project);
        }
    }
}