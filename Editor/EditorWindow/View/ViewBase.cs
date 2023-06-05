using UnityEngine;

namespace ACTSkillEditor
{
    public abstract class ViewBase
    {
        public readonly ACTSkillEditorWindow Owner;

        public ViewBase(ACTSkillEditorWindow owner)
        {
            Owner = owner;
        }
        
        public virtual void OnEnable()
        {
        }
        
        protected abstract void OnGUI(Rect contentRect);

        public virtual void OnDisable()
        {
        }
        
        public virtual void Draw(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, GUIStyleHelper.ViewBg);
            OnGUI(rect);
        }
    }
}
