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

        private UnityEditor.Editor _settingsEditor;

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            var serializedObject = new UnityEditor.SerializedObject(MediatorEditorSettings.instance);

            _settingsEditor ??= UnityEditor.Editor.CreateEditor(serializedObject.targetObject);

            serializedObject.Update();
            _settingsEditor.OnInspectorGUI();
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
            return new MediatorSettingsProvider("Project/MobX/Mediator", UnityEditor.SettingsScope.Project);
        }

        [UnityEditor.MenuItem("MobX/Settings/Mediator", priority = 5000)]
        public static void MenuItem()
        {
            UnityEditor.SettingsService.OpenProjectSettings("Project/MobX/Mediator");
        }
    }
}