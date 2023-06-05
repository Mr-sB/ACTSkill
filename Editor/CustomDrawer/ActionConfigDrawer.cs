// using ACTSkill;
// using UnityEditor;
//
// namespace ACTSkillEditor
// {
//     [CustomPropertyDrawer(typeof(ActionConfig))]
//     public class ActionConfigDrawer : HideableFoldoutDrawer
//     {
//         // private const float ELEMENT_OFFSET_X = 0;
//         // private const float ELEMENT_OFFSET_Y = 2;
//         // private ReorderableList reorderableList;
//         //
//         // private ReorderableList InitReorderableList(SerializedProperty property)
//         // {
//         //     ReorderableList list = new ReorderableList(property.serializedObject, property.FindPropertyRelative(nameof(ActionConfig.Actions)),
//         //         true, true, true, true);
//         //     list.drawHeaderCallback = position =>
//         //     {
//         //         var rect = position;
//         //         rect.width -= 50;
//         //         EditorGUI.LabelField(rect, EditorUtil.TempContent(ObjectNames.NicifyVariableName(nameof(ActionConfig.Actions))));
//         //         rect = position;
//         //         rect.xMin = rect.xMax - 50;
//         //         var enabled = GUI.enabled;
//         //         GUI.enabled = false;
//         //         EditorGUI.IntField(rect, reorderableList.count);
//         //         GUI.enabled = enabled;
//         //     };
//         //
//         //     list.drawElementCallback = (rect, index, active, focused) =>
//         //     {
//         //         rect.xMin += ELEMENT_OFFSET_X;
//         //         rect.yMin += ELEMENT_OFFSET_Y;
//         //         var element = list.serializedProperty.GetArrayElementAtIndex(index);
//         //         var oldLabelWidth = EditorGUIUtility.labelWidth;
//         //         EditorGUIUtility.labelWidth = 80;
//         //         EditorGUI.PropertyField(rect, element,
//         //             EditorUtil.TempContent($"{element.displayName} : {element.managedReferenceValue?.GetType().FullName ?? "Null"}"), true);
//         //         EditorGUIUtility.labelWidth = oldLabelWidth;
//         //     };
//         //     
//         //     list.elementHeightCallback = index => EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index), true) + ELEMENT_OFFSET_Y;
//         //
//         //     var menu = new GenericMenu();
//         //     foreach (var type in TypeCache.GetTypesDerivedFrom<IActionConfig>())
//         //     {
//         //         if (ValidateActionConfigType(type))
//         //             menu.AddItem(new GUIContent(type.FullName), false, OnMenuItemClick, type);
//         //     }
//         //
//         //     list.onAddDropdownCallback = (rect, l) =>
//         //     {
//         //         menu.ShowAsContext();
//         //     };
//         //     
//         //     return list;
//         // }
//         //
//         // private ReorderableList GetReorderableList(SerializedProperty property)
//         // {
//         //     if (reorderableList == null)
//         //         reorderableList = InitReorderableList(property);
//         //     else
//         //         reorderableList.serializedProperty = property.FindPropertyRelative(nameof(ActionConfig.Actions));
//         //     return reorderableList;
//         // }
//         //
//         // private void OnMenuItemClick(object obj)
//         // {
//         //     if (obj is not Type type) return;
//         //     if (!ValidateActionConfigType(type)) return;
//         //     var value = Activator.CreateInstance(type);
//         //     reorderableList.serializedProperty.arraySize = reorderableList.count + 1;
//         //     reorderableList.index = reorderableList.serializedProperty.arraySize - 1;
//         //     reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.index).managedReferenceValue = value;
//         //     //Save changes
//         //     reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
//         //     reorderableList.serializedProperty.serializedObject.Update();
//         // }
//         //
//         // private bool ValidateActionConfigType(Type type)
//         // {
//         //     return !type.IsInterface && !type.IsAbstract && !type.IsGenericType && typeof(IActionConfig).IsAssignableFrom(type);
//         // }
//         //
//         // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         // {
//         //     float usedHeight = 0;
//         //     SerializedProperty hideFoldoutProperty = property.FindPropertyRelative(nameof(ActionConfig.HideFoldout));
//         //     bool hideFoldout = hideFoldoutProperty.boolValue;
//         //     bool expand = true;
//         //     if (!hideFoldout)
//         //     {
//         //         Rect rect = position;
//         //         rect.height = EditorGUIUtility.singleLineHeight;
//         //         expand = EditorGUI.PropertyField(rect, property, label, false);
//         //         EditorGUI.indentLevel++;
//         //         position = EditorGUI.IndentedRect(position);
//         //         usedHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//         //     }
//         //
//         //     if (expand)
//         //     {
//         //         var rect = position;
//         //         rect.yMin += usedHeight;
//         //         GetReorderableList(property).DoList(rect);
//         //     }
//         //
//         //     if (!hideFoldout)
//         //         EditorGUI.indentLevel--;
//         // }
//         //
//         // public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         // {
//         //     float height = 0;
//         //     SerializedProperty hideFoldoutProperty = property.FindPropertyRelative(nameof(ActionConfig.HideFoldout));
//         //     if (!hideFoldoutProperty.boolValue)
//         //     {
//         //         height += EditorGUIUtility.singleLineHeight;
//         //         if (!property.isExpanded) return height;
//         //     }
//         //
//         //     height += GetReorderableList(property).GetHeight() + EditorGUIUtility.standardVerticalSpacing;
//         //     return height;
//         // }
//     }
// }
