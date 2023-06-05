using ACTSkill;

namespace ACTSkillEditor
{
    public interface IRangeViewHandler
    {
        public RangeConfig GetRangeConfig(FrameConfig frameConfig);
        public int GetSelectedIndex(ACTSkillEditorWindow owner);
        public void SetSelectedIndex(ACTSkillEditorWindow owner, int index);
        public string GetSelectedIndexPropertyName();
    }

    public class AttackRangeViewHandler : IRangeViewHandler
    {
        public RangeConfig GetRangeConfig(FrameConfig frameConfig)
        {
            return FrameConfig.GetAttackRange(frameConfig);
        }

        public int GetSelectedIndex(ACTSkillEditorWindow owner)
        {
            if (!owner) return -1;
            return owner.SelectedAttackRangeIndex;
        }

        public void SetSelectedIndex(ACTSkillEditorWindow owner, int index)
        {
            if (!owner) return;
            owner.SelectedAttackRangeIndex = index;
        }

        public string GetSelectedIndexPropertyName()
        {
            return nameof(ACTSkillEditorWindow.SelectedAttackRangeIndex);
        }
    }
    
    public class BodyRangeViewHandler : IRangeViewHandler
    {
        public RangeConfig GetRangeConfig(FrameConfig frameConfig)
        {
            return FrameConfig.GetBodyRange(frameConfig);
        }

        public int GetSelectedIndex(ACTSkillEditorWindow owner)
        {
            if (!owner) return -1;
            return owner.SelectedBodyRangeIndex;
        }

        public void SetSelectedIndex(ACTSkillEditorWindow owner, int index)
        {
            if (!owner) return;
            owner.SelectedBodyRangeIndex = index;
        }

        public string GetSelectedIndexPropertyName()
        {
            return nameof(ACTSkillEditorWindow.SelectedBodyRangeIndex);
        }
    }
}
