using System;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class NewStateWindow : EditorWindow
    {
        private bool focused;
        private string stateGuiName;
        private string stateName = string.Empty;
        public event Action<string> WindowClosed;
        
        public static NewStateWindow Create()
        {
            return CreateInstance<NewStateWindow>();
        }

        public void ShowAsDropDown(Rect buttonRect)
        {
            buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
            ShowAsDropDown(buttonRect, new Vector2(150, (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2 + EditorGUIUtility.standardVerticalSpacing));
        }

        private void OnEnable()
        {
            stateGuiName = GetHashCode() + "State Name";
        }

        private void OnGUI()
        {
            Rect rect = new Rect(Vector2.zero + new Vector2(EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.standardVerticalSpacing),
                position.size - 2 * new Vector2(EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.standardVerticalSpacing));

            Rect viewRect = rect;
            viewRect.height = EditorGUIUtility.singleLineHeight;
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 70;
            GUI.SetNextControlName(stateGuiName);
            stateName = EditorGUI.TextField(viewRect, "State Name", stateName);
            //Auto focus text field
            if (!focused)
            {
                focused = true;
                EditorGUI.FocusTextInControl(stateGuiName);
            }
            EditorGUIUtility.labelWidth = oldLabelWidth;

            viewRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            viewRect.height = EditorGUIUtility.singleLineHeight;
            if (GUI.Button(viewRect, "Create"))
                CloseWindow();
        }
        
        private void CloseWindow()
        {
            WindowClosed?.Invoke(stateName);
            WindowClosed = null;
            Close();
        }
    }
}
