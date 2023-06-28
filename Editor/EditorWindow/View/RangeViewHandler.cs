using ACTSkill;
using UnityEditor;

namespace ACTSkillEditor
{
    public abstract class RangeViewHandlerBase
    {
        public ACTSkillEditorWindow Owner { get; }

        public RangeViewHandlerBase(ACTSkillEditorWindow owner)
        {
            Owner = owner;
        }
        
        public RangeConfig Data => GetRangeConfig(Owner.CurFrameConfig);
        public SerializedProperty RangeProperty => GetRangeProperty(Owner.CurFrameConfigProperty);
        public abstract RangeConfig GetRangeConfig(FrameConfig frameConfig);
        public abstract int GetSelectedIndex(ACTSkillEditorWindow owner);
        public abstract void SetSelectedIndex(ACTSkillEditorWindow owner, int index);
        public abstract string GetSelectedIndexPropertyName();
        public abstract SerializedProperty GetRangeProperty(SerializedProperty serializedProperty);
    }

    public class AttackRangeViewHandler : RangeViewHandlerBase
    {
        public AttackRangeViewHandler(ACTSkillEditorWindow owner) : base(owner)
        {
        }
        
        public override RangeConfig GetRangeConfig(FrameConfig frameConfig)
        {
            return FrameConfig.GetAttackRange(frameConfig);
        }

        public override int GetSelectedIndex(ACTSkillEditorWindow owner)
        {
            if (!owner) return -1;
            return owner.SelectedAttackRangeIndex;
        }

        public override void SetSelectedIndex(ACTSkillEditorWindow owner, int index)
        {
            if (!owner) return;
            owner.SelectedAttackRangeIndex = index;
        }

        public override string GetSelectedIndexPropertyName()
        {
            return nameof(ACTSkillEditorWindow.SelectedAttackRangeIndex);
        }
        
        public override SerializedProperty GetRangeProperty(SerializedProperty serializedProperty)
        {
            return serializedProperty?.FindPropertyRelative(nameof(FrameConfig.AttackRange));
        }
    }
    
    public class BodyRangeViewHandler : RangeViewHandlerBase
    {
        public BodyRangeViewHandler(ACTSkillEditorWindow owner) : base(owner)
        {
        }
        
        public override RangeConfig GetRangeConfig(FrameConfig frameConfig)
        {
            return FrameConfig.GetBodyRange(frameConfig);
        }

        public override int GetSelectedIndex(ACTSkillEditorWindow owner)
        {
            if (!owner) return -1;
            return owner.SelectedBodyRangeIndex;
        }

        public override void SetSelectedIndex(ACTSkillEditorWindow owner, int index)
        {
            if (!owner) return;
            owner.SelectedBodyRangeIndex = index;
        }

        public override string GetSelectedIndexPropertyName()
        {
            return nameof(ACTSkillEditorWindow.SelectedBodyRangeIndex);
        }
        
        public override SerializedProperty GetRangeProperty(SerializedProperty serializedProperty)
        {
            return serializedProperty?.FindPropertyRelative(nameof(FrameConfig.BodyRange));
        }
    }
}
