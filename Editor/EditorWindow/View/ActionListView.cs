using ACTSkill;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class ActionListView : CopyableViewBase
    {
        private string title;
        public override string Title
        {
            get => title;
            set => title = value;
        }
        
        private Vector2 scrollPosition = Vector2.zero;
        public ActionConfig Data => Owner ? Owner.CurActionConfig : null;
        
        public ActionListView(ACTSkillEditorWindow owner) : base(owner)
        {
        }

        public override void OnEnable()
        {
            if (string.IsNullOrEmpty(title))
                title = ObjectNames.NicifyVariableName(nameof(ActionListView));
        }
        
        protected override void OnGUI(Rect contentRect)
        {
            GUILayout.BeginArea(contentRect);
            GUILayout.BeginVertical();

            var property = Owner.CurActionConfigProperty;
            if (property != null)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                // EditorGUILayout.PropertyField(property, true);
                // Skip
                string propertyPath = property.propertyPath;
                bool enterChildren = true;
                while (property.NextVisible(enterChildren) && property.propertyPath.StartsWith(propertyPath))
                {
                    enterChildren = false;
                    EditorGUILayout.PropertyField(property, true);
                }
                // managedReferenceValue maybe changed, try reinit cur action
                var oldValue = Owner.CurAction;
                Owner.InitCurAction();
                if (oldValue != Owner.CurAction)
                    Owner.Repaint();
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public override object CopyData()
        {
            return Data?.Clone();
        }

        public override void PasteData(object data)
        {
            if (Data == null || data is not ActionConfig other) return;
            Owner.RecordObject("Paste action list");
            Data.Copy(other);
            Owner.SelectedActionIndex = -1;
        }
    }
}
