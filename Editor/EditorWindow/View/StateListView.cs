using System.Collections.Generic;
using System.ComponentModel;
using ACTSkill;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ACTSkillEditor
{
    public class StateListView : CopyableViewBase
    {
        private string title;
        public override string Title
        {
            get => title;
            set => title = value;
        }
        
        //More than the number, use dictionary
        // public const int STATE_COUNT_THRESHOLD = 30;
        public const int STATE_COUNT_THRESHOLD = 0;
        private static readonly string WrapperName = typeof(StateConfig).FullName;

        public MachineConfig Data => Owner ? Owner.CurMachine : null;

        // private GUIContent guiContent;
        private ReorderableList reorderableList;
        private Vector2 scrollPosition = Vector2.zero;
        private WrapperSO wrapperSO;
        private Dictionary<string, int> stateNameDict;

        public StateListView(ACTSkillEditorWindow owner) : base(owner)
        {
        }

        // ScriptableObject will be null after recompile. Call GetOrCreateActionWrapperSO method instead of wrapperSO.
        private WrapperSO GetOrCreateWrapperSO()
        {
            if (!wrapperSO)
                wrapperSO = ScriptableObject.CreateInstance<WrapperSO>();
            wrapperSO.NameGetter = () => WrapperName;
            wrapperSO.DrawInspectorGUI ??= () =>
            {
                var property = Owner.CurStateProperty;
                if (property == null) return;
                Owner.SerializedObject?.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                // Even if includeChildren is set to false, the child will still be drawn.
                // guiContent.tooltip = property.tooltip;
                // if (EditorGUILayout.PropertyField(property, guiContent, false))
                //     StateSettingView.DrawProperty(property);
                StateSettingView.DrawProperty(property);
                if (EditorGUI.EndChangeCheck())
                {
                    Owner.ApplyModifiedProperties();
                    Owner.Repaint();
                }
            };
            wrapperSO.DoCopy ??= () => ACTSkillEditorWindow.CopyBuffer = StateSettingView.CopyStateSetting(Owner.CurState);
            wrapperSO.DoPaste ??= () =>
            {
                Owner.RecordObject("Paste state setting");
                StateSettingView.PasteStateSetting(ACTSkillEditorWindow.CopyBuffer, Owner.CurState);
                Owner.Repaint();
            };
            return wrapperSO;
        }
        
        private ReorderableList InitReorderableList(SerializedProperty property)
        {
            ReorderableList list = new ReorderableList(property.serializedObject, property,
                true, true, true, true);
            list.multiSelect = false;
            list.drawHeaderCallback = position =>
            {
                var rect = position;
                rect.width -= 50;
                EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(nameof(MachineConfig.States)));
                rect = position;
                rect.xMin = rect.xMax - 50;
                using (new EditorGUI.DisabledScope(true))
                    EditorGUI.IntField(rect, list.count);
            };
        
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var elementProperty = list.serializedProperty.GetArrayElementAtIndex(index);
                string name = elementProperty.FindPropertyRelative(nameof(StateConfig.StateName)).stringValue;
                bool hasSameName = HasSameName(name, list.serializedProperty, index);

                //Name
                var oldColor = GUI.color;
                if (hasSameName)
                    GUI.color = Color.yellow;
                EditorGUI.LabelField(new Rect(rect.position, new Vector2(rect.width - 20, rect.height)), name);
                if (hasSameName)
                    GUI.color = oldColor;
                
                //Loop
                Rect loopRect = rect;
                loopRect.xMin = loopRect.xMax - 20;
                var loopProperty = elementProperty.FindPropertyRelative(nameof(StateConfig.Loop));
                if (GUI.Button(loopRect, loopProperty.boolValue ? GUIStyleHelper.LoopOnTexture : GUIStyleHelper.LoopOffTexture, GUIStyle.none))
                {
                    loopProperty.boolValue = !loopProperty.boolValue;
                    Event.current.Use();
                }
                
                if (active)
                {
                    if (Owner && Owner.SelectedStateIndex != index)
                        Owner.SelectedStateIndex = index;
                }
                if (focused)
                {
                    //active and keyboardControl
                    if (Selection.activeObject != GetOrCreateWrapperSO())
                        Selection.activeObject = GetOrCreateWrapperSO();
                }
            };

            list.elementHeight = EditorGUIUtility.singleLineHeight;
            
            list.onAddDropdownCallback = (rect, l) =>
            {
                var newStateWindow = NewStateWindow.Create();
                newStateWindow.WindowClosed += newStateName =>
                {
                    if (l.serializedProperty == null) return;
                    bool hasSameName = HasSameName(newStateName, l.serializedProperty, null);

                    if (Owner)
                    {
                        if (!hasSameName)
                        {
                            int size = l.serializedProperty.arraySize;
                            l.serializedProperty.InsertArrayElementAtIndex(size);
                            l.serializedProperty.GetArrayElementAtIndex(size).FindPropertyRelative(nameof(StateConfig.StateName)).stringValue = newStateName;
                            l.index = size;
                            Owner.ApplyModifiedProperties();
                            Owner.Repaint();
                        }
                        else
                            Owner.ShowNotification("Can not add state with same name!", 3, LogType.Error);
                    }
                };
                newStateWindow.ShowAsDropDown(rect);
            };
            
            return list;
        }

        private bool HasSameName(string name, SerializedProperty serializedProperty, int? exceptIndex)
        {
            if (serializedProperty == null) return false;
            bool hasSameName = false;
            if (stateNameDict?.Count > 0)
                hasSameName = stateNameDict.TryGetValue(name, out var num) && num > (exceptIndex.HasValue ? 1 : 0);
            else
            {
                for (int i = 0, size = serializedProperty.arraySize; i < size; i++)
                {
                    if (exceptIndex != i && serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(StateConfig.StateName)).stringValue == name)
                    {
                        hasSameName = true;
                        break;
                    }
                }
            }
            return hasSameName;
        }
        
        private ReorderableList GetReorderableList(SerializedProperty property)
        {
            if (reorderableList == null)
                reorderableList = InitReorderableList(property);
            else if (reorderableList.serializedProperty != property)
                reorderableList.serializedProperty = property;
            if (Data.States.Count > STATE_COUNT_THRESHOLD)
            {
                if (stateNameDict == null)
                    stateNameDict = new Dictionary<string, int>();
                else
                    stateNameDict.Clear();
                foreach (var state in Data.States)
                {
                    var stateName = state.ToString();
                    if (stateNameDict.TryGetValue(stateName, out var num))
                    {
                        if (num < 2)
                            stateNameDict[stateName] = num + 1;
                    }
                    else
                        stateNameDict.Add(stateName, 1);
                }
            }
            return reorderableList;
        }

        public override void OnEnable()
        {
            if (string.IsNullOrEmpty(title))
                title = ObjectNames.NicifyVariableName(nameof(StateListView));
            // guiContent = new GUIContent(typeof(StateConfig).FullName);
            Owner.PropertyChanging += OnOwnerPropertyChanging;
        }

        protected override void OnGUI(Rect contentRect)
        {
            GUILayout.BeginArea(contentRect);
            GUILayout.BeginVertical();

            var property = Owner.CurMachineProperty;
            if (property != null)
            {
                //DefaultStateName
                EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(MachineConfig.DefaultStateName)), true);
                
                //DefaultStateTransition
                EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(MachineConfig.DefaultStateTransition)), true);
                
                //List
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GetReorderableList(property.FindPropertyRelative(nameof(MachineConfig.States))).DoLayoutList();
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

            // Repaint inspector
            if (wrapperSO && Selection.activeObject == wrapperSO)
                EditorUtility.SetDirty(wrapperSO);
        }

        public override void OnDisable()
        {
            if (Owner)
                Owner.PropertyChanging -= OnOwnerPropertyChanging;
            if (wrapperSO)
                Object.DestroyImmediate(wrapperSO);
        }

        public override object CopyData()
        {
            return Data?.Clone();
        }

        public override void PasteData(object data)
        {
            if (Data == null || data is not MachineConfig other) return;
            Owner.RecordObject("Paste state list");
            Data.Copy(other);
            Owner.SelectedStateIndex = -1;
        }
        
        private void OnOwnerPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(ACTSkillEditorWindow.CurState))
            {
                if (Selection.activeObject is WrapperSO so && so == wrapperSO)
                    Selection.activeObject = null;
            }
        }
    }
}
