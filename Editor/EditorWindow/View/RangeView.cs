using System.ComponentModel;
using ACTSkill;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ACTSkillEditor
{
    public class RangeView : CopyableViewBase
    {
        private string title;
        public override string Title
        {
            get => title;
            set => title = value;
        }
        
        private IRangeViewHandler handler;
        private Vector2 scrollPosition = Vector2.zero;
        private WrapperSO wrapperSO;
        private SerializedObject serializedObject;
        public RangeConfig Data { private set; get; }
        private RangeConfigDrawer.RangesReorderableList reorderableList;

        public RangeView(ACTSkillEditorWindow owner, string title, IRangeViewHandler handler) : base(owner)
        {
            this.title = title;
            this.handler = handler;
        }

        private WrapperSO GetOrCreateWrapperSO()
        {
            if (!wrapperSO)
                wrapperSO = ScriptableObject.CreateInstance<WrapperSO>();
            return wrapperSO;
        }
        
        public override void OnEnable()
        {
            if (string.IsNullOrEmpty(title))
                title = ObjectNames.NicifyVariableName(nameof(RangeView));
            Owner.PropertyChanged += OnOwnerPropertyChanged;
            //CreateScriptableObjectInstanceFromType is not allowed to be called from a ScriptableObject constructor (or instance field initializer)
            wrapperSO = ScriptableObject.CreateInstance<WrapperSO>();
            reorderableList = new RangeConfigDrawer.RangesReorderableList(0);
            reorderableList.OnDrawActiveIndex += OnDrawActiveIndex;
            RefreshData();
        }
        
        private void RefreshData()
        {
            if (Owner.CurFrameConfig == null || handler == null)
                SetData(null);
            else
                SetData(handler.GetRangeConfig(Owner.CurFrameConfig));
        }

        private void SetData(RangeConfig data)
        {
            Data = data;
            GetOrCreateWrapperSO().Data = data;
            serializedObject?.Dispose();
            serializedObject = null;
            if (data != null)
            {
                serializedObject = new SerializedObject(GetOrCreateWrapperSO());
            }
        }
        
        protected override void OnGUI(Rect contentRect)
        {
            GUILayout.BeginArea(contentRect);
            GUILayout.BeginVertical();

            if (serializedObject != null && serializedObject.targetObject)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                // EditorGUIExtensions.DrawDefaultInspectorWithoutScript(serializedObject);
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty dataProperty = serializedObject.FindProperty(nameof(WrapperSO.Data));
                var modifyRangesProperty = dataProperty.FindPropertyRelative(nameof(RangeConfig.ModifyRange));
                EditorGUILayout.PropertyField(modifyRangesProperty);
                if (modifyRangesProperty.boolValue)
                {
                    EditorGUI.indentLevel++;
                    reorderableList.GetReorderableList(dataProperty.FindPropertyRelative(nameof(RangeConfig.Ranges))).DoLayoutList();
                    EditorGUI.indentLevel--;
                }
                serializedObject.ApplyModifiedProperties();
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public override void OnDisable()
        {
            if (Owner)
                Owner.PropertyChanged -= OnOwnerPropertyChanged;
            reorderableList.OnDrawActiveIndex -= OnDrawActiveIndex;
            serializedObject?.Dispose();
            Object.DestroyImmediate(wrapperSO);
        }

        public override object CopyData()
        {
            return Data?.Clone();
        }

        public override void PasteData(object data)
        {
            if (Data == null || data is not RangeConfig other) return;
            Data.Copy(other);
        }
        
        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ACTSkillEditorWindow.CurFrameConfig))
                RefreshData();
            else if (handler != null && e.PropertyName == handler.GetSelectedIndexPropertyName())
            {
                int index = handler.GetSelectedIndex(Owner);
                if (reorderableList.ReorderableList != null && !reorderableList.ReorderableList.IsSelected(index))
                    reorderableList.ReorderableList.Select(index);
            }
        }
        
        private void OnDrawActiveIndex(int index)
        {
            handler?.SetSelectedIndex(Owner, index);
        }
    }
}
