using UnityEngine;

namespace ACTSkillEditor
{
    public abstract class BGViewBase : ViewBase
    {
        public BGViewBase(ACTSkillEditorWindow owner, string name, params WindowNodeOption[] options) : base(owner, name, options)
        {
        }

        public override void Draw(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, GUIStyleHelper.ViewBg);
            base.Draw(rect);
        }
    }
}