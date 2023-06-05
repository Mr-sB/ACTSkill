using System;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class CustomizeFrameRateWindow : EditorWindow
    {
        private bool focused;
        private string feameRateGuiName;
        private int frameRate;
        public event Action<int> WindowClosed;
        
        public static CustomizeFrameRateWindow Create(int frameRate = 30)
        {
            var window = CreateInstance<CustomizeFrameRateWindow>();
            window.frameRate = frameRate;
            return window;
        }

        public void ShowAsDropDown(Rect buttonRect)
        {
            buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
            ShowAsDropDown(buttonRect, new Vector2(150, (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2 + EditorGUIUtility.standardVerticalSpacing));
        }

        private void OnEnable()
        {
            feameRateGuiName = GetHashCode() + "Frame Rate";
        }

        private void OnGUI()
        {
            Rect rect = new Rect(Vector2.zero + new Vector2(EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.standardVerticalSpacing),
                position.size - 2 * new Vector2(EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.standardVerticalSpacing));

            Rect viewRect = rect;
            viewRect.height = EditorGUIUtility.singleLineHeight;
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 70;
            GUI.SetNextControlName(feameRateGuiName);
            frameRate = EditorGUI.IntField(viewRect, "Frame Rate", frameRate);
            //Auto focus text field
            if (!focused)
            {
                focused = true;
                EditorGUI.FocusTextInControl(feameRateGuiName);
            }
            EditorGUIUtility.labelWidth = oldLabelWidth;

            viewRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            viewRect.height = EditorGUIUtility.singleLineHeight;
            if (GUI.Button(viewRect, "Confirm"))
                CloseWindow();
        }
        
        private void CloseWindow()
        {
            WindowClosed?.Invoke(frameRate);
            WindowClosed = null;
            Close();
        }
    }
}
