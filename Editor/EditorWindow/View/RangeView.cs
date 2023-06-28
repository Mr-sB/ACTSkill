using System.ComponentModel;
using ACTSkill;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class RangeView : CopyableViewBase
    {
        private string title;
        public override string Title
        {
            get => title;
            set => title = value;
        }
        
        private RangeViewHandlerBase handler;
        private Vector2 scrollPosition = Vector2.zero;
        private RangeConfigDrawer.RangesReorderableList reorderableList;

        public RangeView(ACTSkillEditorWindow owner, string title, RangeViewHandlerBase handler) : base(owner)
        {
            this.title = title;
            this.handler = handler;
        }
        
        public override void OnEnable()
        {
            if (string.IsNullOrEmpty(title))
                title = ObjectNames.NicifyVariableName(nameof(RangeView));
            Owner.PropertyChanged += OnOwnerPropertyChanged;
            reorderableList = new RangeConfigDrawer.RangesReorderableList(0);
            reorderableList.OnDrawActiveIndex += OnDrawActiveIndex;
        }
        
        protected override void OnGUI(Rect contentRect)
        {
            GUILayout.BeginArea(contentRect);
            GUILayout.BeginVertical();

            var property = handler.RangeProperty;
            if (property != null)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                // EditorGUIExtensions.DrawDefaultInspectorWithoutScript(serializedObject);
                var modifyRangesProperty = property.FindPropertyRelative(nameof(RangeConfig.ModifyRange));
                EditorGUILayout.PropertyField(modifyRangesProperty, true);
                if (modifyRangesProperty.boolValue)
                {
                    EditorGUI.indentLevel++;
                    reorderableList.GetReorderableList(property.FindPropertyRelative(nameof(RangeConfig.Ranges))).DoLayoutList();
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public override void OnDisable()
        {
            if (Owner)
                Owner.PropertyChanged -= OnOwnerPropertyChanged;
            reorderableList.OnDrawActiveIndex -= OnDrawActiveIndex;
        }

        public override object CopyData()
        {
            return handler?.Data?.Clone();
        }

        public override void PasteData(object data)
        {
            if (handler?.Data == null || data is not RangeConfig other) return;
            Owner.RecordObject("Paste range view");
            handler.Data.Copy(other);
        }
        
        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (handler != null && e.PropertyName == handler.GetSelectedIndexPropertyName())
            {
                int index = handler.GetSelectedIndex(Owner);
                if (reorderableList.ReorderableList != null && !reorderableList.ReorderableList.IsSelected(index))
                    reorderableList.ReorderableList.Select(index);
            }
        }
        
        private void OnDrawActiveIndex(int index)
        {
            handler?.SetSelectedIndex(Owner, index);
        }
    }
}
