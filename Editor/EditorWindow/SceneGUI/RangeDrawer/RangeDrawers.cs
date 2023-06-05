using ACTSkill;
using UnityEngine;

namespace ACTSkillEditor
{
    [CustomRangeDrawer(typeof(RectRange))]
    public class RectRangeDrawer : IRangeDrawer
    {
        public void Draw(IRange range, Color? color = null)
        {
            if (range is not RectRange v) return;
            HandlesExtension.DrawRect(v.Size, v.Offset, Quaternion.Euler(range.GetRotation() ?? Vector3.zero), Vector3.one, color);
        }
    }
    
    [CustomRangeDrawer(typeof(CircleRange))]
    public class CircleRangeDrawer : IRangeDrawer
    {
        public void Draw(IRange range, Color? color = null)
        {
            if (range is not CircleRange v) return;
            HandlesExtension.DrawCircle(v.Radius, v.Offset, Quaternion.Euler(range.GetRotation() ?? Vector3.zero), Vector3.one, color);
        }
    }
    
    [CustomRangeDrawer(typeof(BoxRange))]
    public class BoxRangeDrawer : IRangeDrawer
    {
        public void Draw(IRange range, Color? color = null)
        {
            if (range is not BoxRange v) return;
            HandlesExtension.DrawBox(v.Size, v.Offset, Quaternion.Euler(range.GetRotation() ?? Vector3.zero), Vector3.one, color);
        }
    }
    
    [CustomRangeDrawer(typeof(SphereRange))]
    public class SphereRangeDrawer : IRangeDrawer
    {
        public void Draw(IRange range, Color? color = null)
        {
            if (range is not SphereRange v) return;
            HandlesExtension.DrawSphere(v.Radius, v.Offset, Quaternion.Euler(range.GetRotation() ?? Vector3.zero), Vector3.one, color);
        }
    }
}
