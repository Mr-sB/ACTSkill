using System;
using UnityEngine;

namespace ACTSkillEditor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class CustomRangeDrawerAttribute : Attribute
    {
        public readonly Type TargetType;
        public CustomRangeDrawerAttribute(Type targetType)
        {
            if (targetType == null)
                Debug.LogError((object) "Failed to load CustomRangeDrawer target type");
            TargetType = targetType;
        }
    }
}
