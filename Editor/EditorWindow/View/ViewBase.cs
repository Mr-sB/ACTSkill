using CustomizationInspector.Editor;
using UnityEngine;

namespace ACTSkillEditor
{
    public abstract class ViewBase : WindowNode
    {
        public readonly ACTSkillEditorWindow Owner;

        public ViewBase(ACTSkillEditorWindow owner, string name, params WindowNodeOption[] options) : base(name, options)
        {
            Owner = owner;
        }
        
        public virtual void OnEnable()
        {
        }
        
        public virtual void OnDisable()
        {
        }
    }
}
