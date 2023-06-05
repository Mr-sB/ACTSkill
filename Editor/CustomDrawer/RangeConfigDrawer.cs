using System;
using ACTSkill;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ACTSkillEditor
{
    [CustomPropertyDrawer(typeof(RangeConfig))]
    public class RangeConfigDrawer : PropertyDrawer
    {
        public class RangesReorderableList
        {
            private const float ELEMENT_OFFSET_Y = 1;
            private const float BUTTON_WIDTH = 20;
            private const float SPACE = 2;
            public ReorderableList ReorderableList { private set; get; }
            private float elementOffsetX = 10;
            private GUIContent guiContent;
            private GUIContent GUIContent => guiContent ??= new GUIContent();
            public event Action<int> OnDrawActiveIndex;
            
            public RangesReorderableList() { }

            public RangesReorderableList(float elementOffsetX)
            {
                this.elementOffsetX = elementOffsetX;
            }
            
            private ReorderableList InitReorderableList(SerializedProperty property)
            {
                ReorderableList list = new ReorderableList(property.serializedObject, property.FindPropertyRelative(nameof(RangeConfig.Ranges)),
                    true, true, true, true);
                list.multiSelect = false;
                list.drawHeaderCallback = position =>
                {
                    var rect = position;
                    rect.width -= 50;
                    EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(nameof(RangeConfig.Ranges)));
                    rect = position;
                    rect.xMin = rect.xMax - 50;
                    // var enabled = GUI.enabled;
                    // GUI.enabled = false;
                    var count = EditorGUI.DelayedIntField(rect, ReorderableList.count);
                    if (count >= 0 && count != ReorderableList.count)
                        ReorderableList.serializedProperty.arraySize = count;
                    // GUI.enabled = enabled;
                };

                list.drawElementCallback = (rect, index, active, focused) =>
                {
                    rect.xMin += elementOffsetX;
                    rect.yMin += ELEMENT_OFFSET_Y;

                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    float buttonX = rect.xMax - BUTTON_WIDTH;
                    if (GUI.Button(new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight), "P"))
                    {
                        if (ACTSkillEditorWindow.CopyBuffer is IRange range)
                            element.managedReferenceValue = range.Clone();
                    }
                    buttonX -= BUTTON_WIDTH + SPACE;
                    if (GUI.Button(new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight), "C"))
                    {
                        if (element.managedReferenceValue is IRange from)
                            ACTSkillEditorWindow.CopyBuffer = from.Clone();
                        else
                            ACTSkillEditorWindow.CopyBuffer = null;
                    }

                    // var oldLabelWidth = EditorGUIUtility.labelWidth;
                    // EditorGUIUtility.labelWidth = 80;
                    // Do not use EditorUtil.TempContent to PropertyField
                    GUIContent.text = element.displayName;
                    GUIContent.tooltip = element.tooltip;
                    EditorGUI.PropertyField(rect, element, GUIContent, true);
                    // EditorGUI.PropertyField(rect, element, new GUIContent(element.displayName, element.tooltip), true);
                    // EditorGUIUtility.labelWidth = oldLabelWidth;

                    if (active)
                        OnDrawActiveIndex?.Invoke(index);
                };

                list.elementHeightCallback = index => EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index), true);

                var menu = new GenericMenu();
                foreach (var type in TypeCache.GetTypesDerivedFrom<IRange>())
                {
                    if (ValidateRangeType(type))
                        menu.AddItem(new GUIContent(type.FullName), false, OnMenuItemClick, type);
                }

                list.onAddDropdownCallback = (rect, l) =>
                {
                    menu.ShowAsContext();
                };

                return list;
            }

            public ReorderableList GetReorderableList(SerializedProperty property)
            {
                if (ReorderableList == null)
                    ReorderableList = InitReorderableList(property);
                else
                    ReorderableList.serializedProperty = property;
                return ReorderableList;
            }

            private void OnMenuItemClick(object obj)
            {
                if (obj is not Type type) return;
                if (!ValidateRangeType(type)) return;
                var value = Activator.CreateInstance(type);
                ReorderableList.serializedProperty.arraySize = ReorderableList.count + 1;
                ReorderableList.index = ReorderableList.serializedProperty.arraySize - 1;
                ReorderableList.serializedProperty.GetArrayElementAtIndex(ReorderableList.index).managedReferenceValue = value;
                //Save changes
                ReorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
                ReorderableList.serializedProperty.serializedObject.Update();
            }

            private bool ValidateRangeType(Type type)
            {
                return !type.IsInterface && !type.IsAbstract && !type.IsGenericType && typeof(IRange).IsAssignableFrom(type);
            }
        }

        private RangesReorderableList reorderableList;
        
        public RangeConfigDrawer()
        {
            reorderableList = new RangesReorderableList();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rect = position;
            rect.height = EditorGUI.GetPropertyHeight(property, false);
            if (!EditorGUI.PropertyField(rect, property, label, false)) return;
            
            rect = EditorGUI.IndentedRect(rect);
            var modifyRangesProperty = property.FindPropertyRelative(nameof(RangeConfig.ModifyRange));
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            rect.height = EditorGUI.GetPropertyHeight(modifyRangesProperty, true);
            EditorGUI.PropertyField(rect, modifyRangesProperty, true);

            if (modifyRangesProperty.boolValue)
            {
                rect = EditorGUI.IndentedRect(rect);
                var list = reorderableList.GetReorderableList(property.FindPropertyRelative(nameof(RangeConfig.Ranges)));
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = list.GetHeight();
                list.DoList(rect);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, false);
            if (!property.isExpanded) return height;
            
            var modifyRangesProperty = property.FindPropertyRelative(nameof(RangeConfig.ModifyRange));
            height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(modifyRangesProperty, true);

            if (modifyRangesProperty.boolValue)
                height += EditorGUIUtility.standardVerticalSpacing + reorderableList.GetReorderableList(property.FindPropertyRelative(nameof(RangeConfig.Ranges))).GetHeight();
            return height;
        }
    }
}
