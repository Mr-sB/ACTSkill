using System.ComponentModel;
using ACTSkill;
using CustomizationInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class StateSettingView : CopyableViewBase
    {
        private string title;
        public override string Title
        {
            get => title;
            set => title = value;
        }

        private Vector2 scrollPosition = Vector2.zero;
        private WrapperSO wrapperSO;
        private SerializedObject serializedObject;
        public StateConfig Data { private set; get; }
        
        public StateSettingView(ACTSkillEditorWindow owner) : base(owner)
        {
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
                title = ObjectNames.NicifyVariableName(nameof(StateSettingView));
            Owner.PropertyChanged += OnOwnerPropertyChanged;
            //CreateScriptableObjectInstanceFromType is not allowed to be called from a ScriptableObject constructor (or instance field initializer)
            wrapperSO = ScriptableObject.CreateInstance<WrapperSO>();
            RefreshData();
        }

        public override void OnDisable()
        {
            if (Owner)
                Owner.PropertyChanged -= OnOwnerPropertyChanged;
            serializedObject?.Dispose();
            Object.DestroyImmediate(wrapperSO);
        }

        private void RefreshData()
        {
            SetData(Owner.CurState);
        }
        
        private void SetData(StateConfig data)
        {
            Data = data;
            GetOrCreateWrapperSO().Data = data;
            serializedObject?.Dispose();
            serializedObject = null;
            if (data != null)
            {
                data.HideFoldout = true;
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
                EditorGUIExtensions.DrawDefaultInspectorWithoutScript(serializedObject);
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        public override object CopyData()
        {
            return Data?.Clone();
        }

        public override void PasteData(object data)
        {
            if (Data == null || data is not StateConfig other) return;
            Data.Copy(other);
        }
        
        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ACTSkillEditorWindow.CurState))
                RefreshData();
        }
    }
}