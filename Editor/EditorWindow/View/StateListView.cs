using System.Collections.Generic;
using System.ComponentModel;
using ACTSkill;
using CustomizationInspector.Editor;
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
        public MachineConfig Data { private set; get; }

        private ReorderableList reorderableList;
        private Vector2 scrollPosition = Vector2.zero;
        private WrapperSO wrapperSO;
        private Dictionary<string, int> stateNameDict;

        public StateListView(ACTSkillEditorWindow owner) : base(owner)
        {
        }

        private WrapperSO GetOrCreateWrapperSO()
        {
            if (!wrapperSO)
            {
                wrapperSO = ScriptableObject.CreateInstance<WrapperSO>();
                wrapperSO.OnValidateEvent += OnWrapperSOValidate;
            }
            return wrapperSO;
        }
        
        private ReorderableList InitReorderableList<T>(List<T> elements)
        {
            ReorderableList list = new ReorderableList(elements, typeof(T),
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
                var element = list.list[index];
                bool hasSameName = false;
                string name = element == null ? TypeDropdown.NULL_TYPE_NAME : element.ToString();
                
                if (stateNameDict?.Count > 0)
                    hasSameName = stateNameDict.TryGetValue(name, out var num) && num > 1;
                else
                {
                    for (var i = 0; i < list.list.Count; i++)
                    {
                        var obj = list.list[i];
                        if ((obj == null ? TypeDropdown.NULL_TYPE_NAME : obj.ToString()) == name && i != index)
                        {
                            hasSameName = true;
                            break;
                        }
                    }
                }

                //Name
                var oldColor = GUI.color;
                if (hasSameName)
                    GUI.color = Color.yellow;
                EditorGUI.LabelField(new Rect(rect.position, new Vector2(rect.width - 20, rect.height)), name);
                if (hasSameName)
                    GUI.color = oldColor;
                
                if (element is StateConfig state)
                {
                    //Loop
                    Rect loopRect = rect;
                    loopRect.xMin = loopRect.xMax - 20;
                    if (GUI.Button(loopRect, state.Loop ? GUIStyleHelper.LoopOnTexture : GUIStyleHelper.LoopOffTexture, GUIStyle.none))
                    {
                        state.Loop = !state.Loop;
                        Event.current.Use();
                    }
                }
                
                if (active)
                {
                    if (Owner && Owner.SelectedStateIndex != index)
                    {
                        Owner.SelectedStateIndex = index;
                        GetOrCreateWrapperSO().Data = Owner.CurState;
                    }
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
                    if (l.list == null) return;
                    bool canAdd = true;
                    foreach (var obj in l.list)
                    {
                        if (obj is not StateConfig state) continue;
                        if (state.StateName == newStateName)
                            canAdd = false;
                    }

                    if (canAdd)
                    {
                        l.list.Add(new StateConfig {StateName = newStateName});
                        l.index = l.list.Count - 1;
                    }
                    else if (Owner)
                        Owner.ShowNotification(EditorUtil.TempContent("Can not add state with same name!"), 3);
                };
                newStateWindow.ShowAsDropDown(rect);
            };
            
            return list;
        }
        
        private ReorderableList GetReorderableList()
        {
            if (reorderableList == null)
                reorderableList = InitReorderableList(Data.States);
            else
                reorderableList.list = Data.States;
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
            Owner.PropertyChanged += OnOwnerPropertyChanged;
            //CreateScriptableObjectInstanceFromType is not allowed to be called from a ScriptableObject constructor (or instance field initializer)
            GetOrCreateWrapperSO();
            RefreshData();
        }
        
        private void RefreshData()
        {
            SetData(Owner.CurMachine);
        }

        private void SetData(MachineConfig data)
        {
            Data = data;
        }
        
        protected override void OnGUI(Rect contentRect)
        {
            if (Data == null) return;
            GUILayout.BeginArea(contentRect);
            GUILayout.BeginVertical();

            if (Data != null)
            {
                var oldLabelWidth = EditorGUIUtility.labelWidth;
                
                //DefaultStateName
                GUIContent content = EditorUtil.TempContent(ObjectNames.NicifyVariableName(nameof(Data.DefaultStateName)));
                EditorGUIUtility.labelWidth = EditorStyles.textField.CalcSize(content).x;
                Data.DefaultStateName = EditorGUILayout.TextField(content, Data.DefaultStateName);
                
                //DefaultStateTransition
                content = EditorUtil.TempContent(ObjectNames.NicifyVariableName(nameof(Data.DefaultStateTransition)));
                FieldInspector.DrawFieldLayout(content, typeof(AnimationTransitionConfig), Data.DefaultStateTransition, Owner);
                
                EditorGUIUtility.labelWidth = oldLabelWidth;

                //List
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GetReorderableList().DoLayoutList();
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public override void OnDisable()
        {
            if (Owner)
                Owner.PropertyChanged -= OnOwnerPropertyChanged;
            if (wrapperSO)
            {
                wrapperSO.OnValidateEvent -= OnWrapperSOValidate;
                Object.DestroyImmediate(wrapperSO);
            }
        }

        public override object CopyData()
        {
            return Data?.Clone();
        }

        public override void PasteData(object data)
        {
            if (Data == null || data is not MachineConfig other) return;
            Data.Copy(other);
        }
        
        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ACTSkillEditorWindow.CurMachine))
                RefreshData();
            else if (e.PropertyName == nameof(ACTSkillEditorWindow.CurState))
            {
                if (Selection.activeObject is WrapperSO so && so.Data == Owner.CurState)
                    Selection.activeObject = null;
            }
        }

        private void OnWrapperSOValidate()
        {
            if (Owner)
                Owner.Repaint();
        }
    }
}
