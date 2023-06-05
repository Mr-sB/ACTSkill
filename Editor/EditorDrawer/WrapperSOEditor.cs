using System;
using ACTSkill;
using CustomizationInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    [CustomEditor(typeof(WrapperSO), true)]
    public class WrapperSOEditor : Editor
    {
        private const float BUTTON_WIDTH = 25;
        private const float SPACE = 2;
        private GUIContent guiContent;
        
        private void OnEnable()
        {
            guiContent = new GUIContent();
        }

        public override void OnInspectorGUI()
        {
            HideableFoldout hideableFoldout = null;
            bool hideFoldout = false;
            var rect = EditorGUILayout.BeginVertical();
            if (target && target is WrapperSO wrapperSo)
            {
                //Set HideFoldout to false
                if (wrapperSo.Data is HideableFoldout hideable)
                {
                    hideableFoldout = hideable;
                    hideFoldout = hideableFoldout.HideFoldout;
                    hideableFoldout.HideFoldout = false;
                }
                float x = rect.xMax - BUTTON_WIDTH;
                if (wrapperSo.Data is ICopyable copyable)
                {
                    if (GUI.Button(new Rect(x, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight), "P"))
                    {
                        copyable.Copy(ACTSkillEditorWindow.CopyBuffer);
                        (target as WrapperSO)?.OnValidate();
                    }
                    x -= BUTTON_WIDTH + SPACE;
                }
                if (wrapperSo.Data is ICloneable cloneable)
                    if (GUI.Button(new Rect(x, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight), "C"))
                        ACTSkillEditorWindow.CopyBuffer = cloneable.Clone();
            }

            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                switch (iterator.propertyPath)
                {
                    case "m_Script":
                        continue;
                    case nameof(WrapperSO.Data):
                        guiContent.text = iterator.GetObject()?.GetType().FullName ?? nameof(WrapperSO.Data);
                        // Do not use EditorUtil.TempContent to PropertyField
                        EditorGUILayout.PropertyField(iterator, guiContent, true);
                        break;
                    default:
                        EditorGUILayout.PropertyField(iterator, true);
                        break;
                }
            }
            
            if (hideableFoldout != null)
                hideableFoldout.HideFoldout = hideFoldout;
            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
            
            GUILayout.EndVertical();
        }
    }
}
