using CustomizationInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public abstract class CopyableViewBase : HeaderViewBase
    {
        protected CopyableViewBase(ACTSkillEditorWindow owner, string name, params WindowNodeOption[] options) : base(owner, name, options)
        {
        }
        
        public abstract object CopyData();

        public abstract void PasteData(object data);

        protected override void OnHeaderDraw(float headerHeight)
        {
            base.OnHeaderDraw(headerHeight);
            if (GUILayout.Button(EditorUtil.TempContent("C", "Copy data"), EditorStyles.toolbarButton, GUILayout.Width(26)))
                ACTSkillEditorWindow.CopyBuffer = CopyData();

            if (GUILayout.Button(EditorUtil.TempContent("P", "Past data"), EditorStyles.toolbarButton, GUILayout.Width(26)))
            {
                var data = ACTSkillEditorWindow.CopyBuffer;
                if (data != null)
                    PasteData(data);
            }
        }
    }
}
