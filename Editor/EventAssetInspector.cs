using MobX.Mediator.Events;
using MobX.Utilities.Editor;
using MobX.Utilities.Editor.Inspector;
using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mobx.Mediator.Editor
{
    [UnityEditor.CustomEditor(typeof(EventMediator), true)]
    public class EventAssetInspector : OverrideInspector<EventMediator>
    {
        private const string EVENT_FIELD_NAME = "Event";
        private const string NEXT_INDEX_FIELD_NAME = "_nextIndex";
        private const string LISTENER_FIELD_NAME = "_listener";
        private const string CLEAR_METHOD_NAME = "Clear";
        private const string CLEAR_INVALID_METHOD_NAME = "ClearInvalid";
        private const string RAISE_METHOD_NAME = "Raise";
        private const string REMOVE_METHOD_NAME = "Remove";

        private bool _showListener;
        private ParameterInfo[] _parameterInfos;
        private object[] _arguments;

        private Func<int> _count;
        private Func<Delegate[]> _listener;
        private Func<object, object>[] _elementEditors;
        private Action _clear;
        private Action _clearInvalid;
        private Action _raise;
        private Action<Delegate> _remove;

        private Vector2 _scrollPosition;

        protected override void OnEnable()
        {
            base.OnEnable();
            FieldInfo eventField = GetFieldIncludeBaseTypes(target.GetType(), EVENT_FIELD_NAME);
            var eventValue = eventField.GetValue(target);

            Type broadcastType = eventValue.GetType();
            Type receiverType = eventValue.GetType().BaseType!;
            FieldInfo indexField = receiverType.GetField(NEXT_INDEX_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo listenerField =
                receiverType.GetField(LISTENER_FIELD_NAME, BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo clearMethod = receiverType.GetMethod(CLEAR_METHOD_NAME,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            MethodInfo clearInvalidMethod = receiverType.GetMethod(CLEAR_INVALID_METHOD_NAME,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            MethodInfo raiseMethod = broadcastType.GetMethod(RAISE_METHOD_NAME, BindingFlags.Public | BindingFlags.Instance)!;
            MethodInfo removeMethod = receiverType.GetMethod(REMOVE_METHOD_NAME, BindingFlags.Public | BindingFlags.Instance)!;

            _count = () => (int) indexField!.GetValue(eventValue);
            _listener = () => listenerField!.GetValue(eventValue) as Delegate[];
            _clear = () => clearMethod!.Invoke(eventValue, null);
            _clearInvalid = () => clearInvalidMethod!.Invoke(eventValue, null);
            _raise = () => raiseMethod!.Invoke(eventValue, _arguments);
            _remove = listener => removeMethod!.Invoke(eventValue, new object[]
            {
                listener
            });

            _parameterInfos = raiseMethod.GetParameters();
            _elementEditors = new Func<object, object>[_parameterInfos.Length];
            _arguments = new object[_parameterInfos.Length];
            for (var i = 0; i < _arguments.Length; i++)
            {
                ParameterInfo parameterInfo = _parameterInfos[i];
                Type parameterType = parameterInfo.ParameterType;
                Type underlyingParameterType = parameterType.GetElementType() ?? parameterType;
                _arguments[i] = underlyingParameterType.IsValueType
                    ? Activator.CreateInstance(underlyingParameterType, true)
                    : Convert.ChangeType(_arguments[i], underlyingParameterType);

                _elementEditors[i] =
                    GUIHelper.CreateEditor(new GUIContent(parameterInfo.Name), parameterInfo.ParameterType);
            }
        }

        public override void OnInspectorGUI()
        {
            try
            {
                base.OnInspectorGUI();
            }
            catch (NullReferenceException)
            {
            }

            if (Foldout["Settings"])
            {
                UnityEditor.EditorGUILayout.LabelField("Listener Count", _count().ToString());

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear"))
                {
                    _clear();
                }

                if (GUILayout.Button("Clear Invalid"))
                {
                    _clearInvalid();
                }

                GUILayout.EndHorizontal();
            }

            if (Foldout["Arguments"])
            {
                GUIHelper.Space();
                for (var i = 0; i < _parameterInfos.Length; i++)
                {
                    _arguments[i] = _elementEditors[i](_arguments[i]);
                }

                GUIHelper.Space();
                if (GUILayout.Button("Raise"))
                {
                    _raise();
                }

                GUIHelper.Space();
            }

            if (Foldout["Listener"])
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                DrawListenerGUI();
                GUILayout.EndScrollView();
            }
        }

        private void DrawListenerGUI()
        {
            Delegate[] listeners = _listener();
            var count = _count();

            for (var index = 0; index < count; index++)
            {
                Delegate listener = listeners[index];
                DrawListener(listener);
            }
        }

        private void DrawListener(Delegate listener)
        {
            if (listener == null)
            {
                UnityEditor.EditorGUILayout.LabelField("Listener: NULL");
                UnityEditor.EditorGUILayout.LabelField("Target:   NULL");
                return;
            }

            GUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField($"Listener: {listener.Method}");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Raise", GUILayout.Width(70)))
            {
                listener.DynamicInvoke(_arguments);
            }

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                _remove(listener);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField($"Target: {listener.Target}");
            GUILayout.FlexibleSpace();
            if (listener.Target is Object unityObject && unityObject)
            {
                if (GUILayout.Button("Select", GUILayout.Width(70)))
                {
                    UnityEditor.Selection.activeObject = unityObject;
                    UnityEditor.EditorGUIUtility.PingObject(unityObject);
                }

                if (GUILayout.Button("Ping", GUILayout.Width(70)))
                {
                    UnityEditor.EditorGUIUtility.PingObject(unityObject);
                }
            }

            GUILayout.EndHorizontal();
        }


        #region Reflection

        private static FieldInfo GetFieldIncludeBaseTypes(Type type, string fieldName, BindingFlags flags =
            BindingFlags.Static |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.FlattenHierarchy)
        {
            FieldInfo fieldInfo = null;
            Type targetType = type;

            while (fieldInfo == null)
            {
                fieldInfo = targetType.GetField(fieldName, flags);
                targetType = targetType.BaseType;

                if (targetType == null)
                {
                    return null;
                }
            }

            return fieldInfo;
        }

        #endregion
    }
}
