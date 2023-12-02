﻿using UnityEngine;
using GUIUtility = MobX.Utilities.Editor.Helper.GUIUtility;

namespace Mobx.Mediator.Editor
{
    public class RenamingWindow : UnityEditor.EditorWindow
    {
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPosition;

        [UnityEditor.MenuItem("MobX/Asset Renaming")]
        public static void ShowWindow()
        {
            GetWindow<RenamingWindow>("Asset and Object Renaming");
        }

        private void OnEnable()
        {
            _editor = UnityEditor.Editor.CreateEditor(AssetRenamingTool.Local);
            UnityEditor.Selection.selectionChanged += Repaint;
        }

        private void OnDisable()
        {
            UnityEditor.Selection.selectionChanged -= Repaint;
        }

        protected void OnGUI()
        {
            _scrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(_scrollPosition);
            GUIUtility.BeginLabelWidthOverride(250);
            if (_editor != null)
            {
                _editor.OnInspectorGUI();
            }
            GUIUtility.EndLabelWidthOverride();
            UnityEditor.EditorGUILayout.EndScrollView();
        }
    }
}