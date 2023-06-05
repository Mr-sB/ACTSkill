using ACTSkill;
using UnityEngine;

namespace ACTSkillEditor
{
    public interface IRangeDrawer
    {
        public void Draw(IRange range, Color? color = null);
    }
}
