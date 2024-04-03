using UnityEngine;

namespace ACTSkillEditor
{
    public class ContainerView : ViewBase
    {
        public ContainerView(string name, params WindowNodeOption[] options) : base(null, name, options)
        {
        }

        protected override void OnGUI(Rect rect)
        {
        }
    }
}
