using CustomizationInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public abstract class CopyableViewBase : HeaderViewBase
    {
        protected CopyableViewBase(ACTSkillEditorWindow owner) : base(owner)
        {
        }
        
        public abstract object CopyData();

        public abstract void PasteData(object data);

        protected override void OnHeaderDraw(float headerHeight)
        {
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
