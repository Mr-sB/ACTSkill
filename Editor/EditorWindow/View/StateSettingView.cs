using ACTSkill;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class StateSettingView : CopyableViewBase
    {
        private string title;
        public override string Title
        {
            get => title;
            set => title = value;
        }

        private Vector2 scrollPosition = Vector2.zero;

        public StateConfig Data => Owner ? Owner.CurState : null;

        public StateSettingView(ACTSkillEditorWindow owner) : base(owner)
        {
        }
        
        public override void OnEnable()
        {
            if (string.IsNullOrEmpty(title))
                title = ObjectNames.NicifyVariableName(nameof(StateSettingView));
        }

        protected override void OnGUI(Rect contentRect)
        {
            GUILayout.BeginArea(contentRect);
            GUILayout.BeginVertical();

            var property = Owner.CurStateProperty;
            if (property != null)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                DrawProperty(property);
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        public static void DrawProperty(SerializedProperty property)
        {
            if (property == null) return;
            // EditorGUILayout.PropertyField(property, true);
            // Skip
            string propertyPath = property.propertyPath;
            bool enterChildren = true;
            while (property.NextVisible(enterChildren) && property.propertyPath.StartsWith(propertyPath))
            {
                enterChildren = false;
                string relativePath = property.propertyPath.Substring(propertyPath.Length + 1);
                if (relativePath != nameof(StateConfig.Frames) && relativePath != nameof(StateConfig.ActionConfig))
                    EditorGUILayout.PropertyField(property, true);
            }
        }
        
        public override object CopyData()
        {
            return CopyStateSetting(Data);
        }

        public override void PasteData(object data)
        {
            if (Data == null || data is not StateConfig other) return;
            Owner.RecordObject("Paste state setting");
            PasteStateSetting(other, Data);
        }

        public static StateConfig CopyStateSetting(StateConfig data)
        {
            return data?.CloneStateSetting();
        }
        
        public static void PasteStateSetting(object from, StateConfig to)
        {
            if (to == null || from is not StateConfig other) return;
            to.CopyStateSetting(other);
        }
    }
}