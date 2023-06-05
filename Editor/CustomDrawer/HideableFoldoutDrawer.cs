using ACTSkill;
using CustomizationInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    [CustomPropertyDrawer(typeof(HideableFoldout))]
    public class HideableFoldoutDrawer : PropertyDrawer
    {
        public virtual string HideFoldoutName { get; } = nameof(HideableFoldout.HideFoldout);
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsHide(property))
                EditorGUI.PropertyField(position, property, label, true);
            else
            {
                //Hide
                var rect = position;
                for (bool enterChildren = true; property.NextVisible(enterChildren); enterChildren = false)
                {
                    rect.height = EditorGUI.GetPropertyHeight(property, true);
                    EditorGUI.PropertyField(rect, property, true);
                    rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!IsHide(property))
                return EditorGUI.GetPropertyHeight(property, true);
            property.isExpanded = true;
            float fullHeight = EditorGUI.GetPropertyHeight(property, true);
            float labelHeight = EditorGUI.GetPropertyHeight(property, false);
            if (fullHeight > labelHeight)
                return fullHeight - labelHeight - EditorGUIUtility.standardVerticalSpacing;
            return fullHeight;
        }

        public bool IsHide(SerializedProperty property)
        {
            object obj = property.GetObject();
            return obj?.GetType().GetField(HideFoldoutName)?.GetValue(obj) is bool hide && hide;
        }
    }
}
