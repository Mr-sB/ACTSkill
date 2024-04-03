using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using ACTSkill;
using CustomizationInspector.Editor;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class ACTSkillEditorWindow : EditorWindow, INotifyPropertyChanging, INotifyPropertyChanged
    {
        public const string ShowSceneGUISaveKey = "ACTSkillEditorWindow.ShowSceneGUI";
        public static object CopyBuffer;

        #region Style

        public static readonly float MenuViewHeight = EditorGUIUtility.singleLineHeight + 3;

        #endregion

        #region Data

        private GameObject target;
        public GameObject Target
        {
            get => target;
            set
            {
                if (target == value) return;
                OnPropertyChanging();
                target = value;
                OnPropertyChanged();
            }
        }

        private TextAsset configAsset;
        public TextAsset ConfigAsset
        {
            get => configAsset;
            set
            {
                if (configAsset == value) return;
                OnPropertyChanging();
                configAsset = value;
                OnPropertyChanged();
            }
        }
        
        private bool? showSceneGUI;
        public bool ShowSceneGUI
        {
            get => showSceneGUI ?? true;
            set
            {
                if (showSceneGUI == value) return;
                showSceneGUI = value;
                RepaintSceneViews();
            }
        }

        // Make SerializeField to get serializedProperty.
        [SerializeField]
        private MachineConfig curMachine;
        
        public MachineConfig CurMachine
        {
            get => curMachine;
            private set
            {
                if (curMachine == value) return;
                OnPropertyChanging();
                RecordObject("Change machine config");
                curMachine = value;
                OnPropertyChanged();
                //Clear selection
                SelectedStateIndex = -1;
            }
        }

        // Don not recover data after compilation.
        [NonSerialized]
        private StateConfig curState;
        public StateConfig CurState
        {
            get => curState;
            private set
            {
                if (curState == value) return;
                OnPropertyChanging();
                curState = value;
                OnPropertyChanged();
                CurFrames = CurState?.Frames;
                CurActionConfig = CurState?.ActionConfig;
                SelectedFrameIndex = -1;
                SelectedActionIndex = -1;
            }
        }

        [NonSerialized]
        private List<FrameConfig> curFrames;

        public List<FrameConfig> CurFrames
        {
            get => curFrames;
            private set
            {
                if (curFrames == value) return;
                OnPropertyChanging();
                curFrames = value;
                OnPropertyChanged();
            }
        }
        
        [NonSerialized]
        private ActionConfig curActionConfig;
        public ActionConfig CurActionConfig
        {
            get => curActionConfig;
            private set
            {
                if (curActionConfig == value) return;
                OnPropertyChanging();
                curActionConfig = value;
                OnPropertyChanged();
            }
        }

        [NonSerialized]
        private FrameConfig curFrameConfig;

        public FrameConfig CurFrameConfig
        {
            get => curFrameConfig;
            private set
            {
                if (curFrameConfig == value) return;
                OnPropertyChanging();
                curFrameConfig = value;
                OnPropertyChanged();
                //Clear selection
                SelectedAttackRangeIndex = -1;
                SelectedBodyRangeIndex = -1;
                RepaintSceneViews();
            }
        }

        [NonSerialized]
        private ActionBase curAction;
        public ActionBase CurAction
        {
            get => curAction;
            private set
            {
                if (curAction == value) return;
                OnPropertyChanging();
                curAction = value;
                OnPropertyChanged();
            }
        }
        
        [NonSerialized]
        private int selectedStateIndex = -1;
        public int SelectedStateIndex
        {
            get => selectedStateIndex;
            set
            {
                if (selectedStateIndex == value) return;
                OnPropertyChanging();
                selectedStateIndex = value;
                OnPropertyChanged();
                InitCurState();
            }
        }
        
        [NonSerialized]
        private int selectedFrameIndex = -1;
        public int SelectedFrameIndex
        {
            get => selectedFrameIndex;
            set
            {
                if (selectedFrameIndex == value) return;
                OnPropertyChanging();
                selectedFrameIndex = value;
                OnPropertyChanged();
                InitCurFrameConfig();
            }
        }

        [NonSerialized]
        private int selectedActionIndex = -1;
        public int SelectedActionIndex
        {
            get => selectedActionIndex;
            set
            {
                if (selectedActionIndex == value) return;
                OnPropertyChanging();
                selectedActionIndex = value;
                OnPropertyChanged();
                InitCurAction();
            }
        }
        
        [NonSerialized]
        private int selectedAttackRangeIndex = -1;
        public int SelectedAttackRangeIndex
        {
            get => selectedAttackRangeIndex;
            set
            {
                if (selectedAttackRangeIndex == value) return;
                OnPropertyChanging();
                selectedAttackRangeIndex = value;
                OnPropertyChanged();
                RepaintSceneViews();
            }
        }
        
        [NonSerialized]
        private int selectedBodyRangeIndex = -1;
        public int SelectedBodyRangeIndex
        {
            get => selectedBodyRangeIndex;
            set
            {
                if (selectedBodyRangeIndex == value) return;
                OnPropertyChanging();
                selectedBodyRangeIndex = value;
                OnPropertyChanged();
                RepaintSceneViews();
            }
        }

        private void InitCurState()
        {
            if (CurMachine?.States != null && selectedStateIndex >= 0 && selectedStateIndex < CurMachine.States.Count)
                CurState = CurMachine.States[selectedStateIndex];
            else
                CurState = null;
        }

        private void InitCurFrameConfig()
        {
            if (CurFrames != null && selectedFrameIndex >= 0 && selectedFrameIndex < CurFrames.Count)
                CurFrameConfig = CurFrames[selectedFrameIndex];
            else
                CurFrameConfig = null;
        }

        internal void InitCurAction()
        {
            if (CurActionConfig?.Actions != null && selectedActionIndex >= 0 && selectedActionIndex < CurActionConfig.Actions.Count)
                CurAction = CurActionConfig.Actions[selectedActionIndex];
            else
                CurAction = null;
        }
        
        #endregion

        #region SerializedObject
        
        private SerializedObject serializedObject;

        public SerializedObject SerializedObject
        {
            get
            {
                if (curMachine == null) return null;
                if (serializedObject?.targetObject != this)
                {
                    serializedObject?.Dispose();
                    serializedObject = new SerializedObject(this);
                }
                return serializedObject;
            }
        }

        public SerializedProperty CurMachineProperty => SerializedObject?.FindProperty(nameof(curMachine));

        public SerializedProperty StateListProperty => CurMachineProperty?.FindPropertyRelative(nameof(MachineConfig.States));

        public SerializedProperty CurStateProperty
        {
            get
            {
                if (SelectedStateIndex < 0) return null;
                var property = StateListProperty;
                if (property == null || property.arraySize <= selectedStateIndex) return null;
                return property.GetArrayElementAtIndex(SelectedStateIndex);
            }
        }

        public SerializedProperty CurFrameListProperty => CurStateProperty?.FindPropertyRelative(nameof(StateConfig.Frames));
        
        public SerializedProperty CurFrameConfigProperty
        {
            get
            {
                if (selectedFrameIndex < 0) return null;
                var property = CurFrameListProperty;
                if (property == null || property.arraySize <= selectedFrameIndex) return null;
                return property.GetArrayElementAtIndex(selectedFrameIndex);
            }
        }
        
        public SerializedProperty CurActionConfigProperty => CurStateProperty?.FindPropertyRelative(nameof(StateConfig.ActionConfig));
        public SerializedProperty CurActionListProperty => CurActionConfigProperty?.FindPropertyRelative(nameof(ActionConfig.Actions));
        public SerializedProperty CurActionProperty
        {
            get
            {
                if (selectedActionIndex < 0) return null;
                var property = CurActionListProperty;
                if (property == null || property.arraySize <= selectedActionIndex) return null;
                return property.GetArrayElementAtIndex(selectedActionIndex);
            }
        }

        #endregion
        

        #region View

        public WindowTree<ViewBase> viewTree;
        private TimelineView timelineView;
        private List<float> viewLengthRatioList = new List<float>();

        #endregion

        #region SceneGUI

        private List<SceneGUIBase> sceneGUIs;
        private RangeSceneGUI rangeSceneGUI;
        
        #endregion

        #region Notify

        public event Action OnReload;
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }
        
        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private SkillWindowHandlerBase skillWindowHandler;

        [MenuItem("ACTSkill/Skill Editor")]
        public static ACTSkillEditorWindow ShowEditor()
        {
            var window = GetWindow<ACTSkillEditorWindow>(false, "ACT Skill Editor", true);
            window.minSize = new Vector2(400f, 200f);
            return window;
        }
        
        public static ACTSkillEditorWindow ShowEditor(GameObject target, TextAsset config)
        {
            var window = ShowEditor();
            window.Target = target;
            window.ConfigAsset = config;
            window.Reload();
            return window;
        }

        public static void RepaintSceneViews()
        {
            SceneView.RepaintAll();
        }

        private void OnEnable()
        {
            // Init data
            curMachine ??= new MachineConfig();

            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            OnPlayModeStateChanged(Application.isPlaying ? PlayModeStateChange.EnteredPlayMode : PlayModeStateChange.EnteredEditMode);
            
            CreateViews();
            CreateSceneGUIs();
            
            ShowSceneGUI = EditorPrefs.GetBool(ShowSceneGUISaveKey, true);

            foreach (var view in viewTree)
                view?.OnEnable();
            
            foreach (var sceneGUI in sceneGUIs)
                sceneGUI.OnEnable();
        }

        private void CreateViews()
        {
            viewTree = new WindowTree<ViewBase>(new ContainerView("Container0", WindowNode.WithDirection(WindowNode.LayoutDirection.Vertical))
            {
                new MenuView(this, "MenuView", WindowNode.WithFixedLength(MenuViewHeight)),
                new ContainerView("Container1", WindowNode.WithDirection(WindowNode.LayoutDirection.Horizontal))
                {
                    new ContainerView("Container2", WindowNode.WithDirection(WindowNode.LayoutDirection.Vertical))
                    {
                        new StateListView(this, "StateListView"),
                        new StateSettingView(this, "StateSettingView"),
                    },
                    new ContainerView("Container3", WindowNode.WithDirection(WindowNode.LayoutDirection.Vertical))
                    {
                        new TimelineView(this, "TimelineView"),
                        new ContainerView("Container4", WindowNode.WithDirection(WindowNode.LayoutDirection.Horizontal))
                        {
                            new RangeView(this, "Attack Range View", new AttackRangeViewHandler(this)),
                            new RangeView(this, "Body Range View", new BodyRangeViewHandler(this)),
                            new ActionListView(this, "ActionListView"),
                        }
                    }
                }
            });
            timelineView = viewTree.Find("TimelineView") as TimelineView;
            
            // Recovery ratio
            int index = 0;
            foreach (var view in viewTree)
            {
                if (view.IsFixed) continue;
                if (index >= viewLengthRatioList.Count) break;
                WindowNode.WithCurLengthRatio(viewLengthRatioList[index]).Invoke(view);
                index++;
            }
        }

        private void CreateSceneGUIs()
        {
            sceneGUIs = new List<SceneGUIBase>();

            rangeSceneGUI = new RangeSceneGUI(this);
            sceneGUIs.Add(rangeSceneGUI);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode && state != PlayModeStateChange.EnteredPlayMode) return;
            skillWindowHandler?.OnDisable();
            if (state == PlayModeStateChange.EnteredEditMode)
                skillWindowHandler = new EditorSkillWindowHandler(this);
            else
                skillWindowHandler = new RuntimeSkillWindowHandler(this);
            skillWindowHandler.Awake();
            Repaint();
        }
        
        private void OnUndoRedoPerformed()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (SerializedObject == null) return;
            SerializedObject.UpdateIfRequiredOrScript();
            skillWindowHandler.OnGUI();
            skillWindowHandler.BeginOnGUI();
            
            viewTree.Draw(new Rect(0, 0, position.width, position.height));
            // Record ratio
            viewLengthRatioList.Clear();
            foreach (var view in viewTree)
            {
                if (!view.IsFixed)
                    viewLengthRatioList.Add(view.CurLengthRatio);
            }
            
            skillWindowHandler.EndOnGUI();
            
            SerializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!ShowSceneGUI) return;
            foreach (var sceneGUI in sceneGUIs)
                sceneGUI.OnSceneGUI(sceneView);
        }

        private void ShowButton(Rect rect)
        {
            if (GUI.Button(rect, GUIStyleHelper.SettingTexture, EditorStyles.iconButton))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Show Scene GUI"), ShowSceneGUI, SetShowSceneGUI, !ShowSceneGUI);
                
                menu.ShowAsContext();
            }
        }
        
        private void SetShowSceneGUI(object userData)
        {
            if (userData is not bool showSceneGUI) return;
            ShowSceneGUI = showSceneGUI;
        }
        
        private void OnDisable()
        {
            EditorPrefs.SetBool(ShowSceneGUISaveKey, ShowSceneGUI);
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            foreach (var view in viewTree)
                view?.OnDisable();
            
            foreach (var sceneGUI in sceneGUIs)
                sceneGUI.OnDisable();
            sceneGUIs.Clear();
            
            skillWindowHandler?.OnDisable();
            skillWindowHandler = null;
            serializedObject?.Dispose();
            serializedObject = null;
        }

        public void ShowNotification(GUIContent content, double duration = 4.0, LogType? logType = LogType.Log)
        {
            base.ShowNotification(content, duration);
            if (logType.HasValue)
                Debug.unityLogger.Log(logType.Value, content.text);
        }
        
        public void ShowNotification(string content, double duration = 4.0, LogType? logType = LogType.Log)
        {
            ShowNotification(EditorUtil.TempContent(content), duration, logType);
        }

        public void ApplyModifiedProperties()
        {
            SerializedObject?.ApplyModifiedProperties();
        }
        
        public void RecordObject(string name = "Change machine config")
        {
            Undo.RecordObject(this, name);
        }
        
        public void RefreshAnimationProcessor()
        {
            if (skillWindowHandler is EditorSkillWindowHandler editorSkillWindowHandler)
                editorSkillWindowHandler.RefreshAnimationProcessor();
        }

        public void ScrollFrameToView()
        {
            timelineView.ScrollFrameToView();
        }

        public void Save()
        {
            if (CurMachine == null)
            {
                ShowNotification("Save failed, does not exist Machine Data!", 3, LogType.Error);
                return;
            }
            if (!configAsset)
            {
                ShowNotification("Save failed, does not exist Config Asset!", 3, LogType.Error);
                return;
            }

            string path = AssetDatabase.GetAssetPath(configAsset);
            Save(path);
            Debug.Log("Save success. Path: " + path);
            ShowNotification("Save success", 3, null);
            EditorUtility.SetDirty(configAsset);
            AssetDatabase.SaveAssetIfDirty(configAsset);
            AssetDatabase.Refresh();
        }

        private void Save(string path)
        {
            File.WriteAllText(path, CurMachine.Serialize(true));
        }

        public void SaveAs()
        {
            if (CurMachine == null)
            {
                ShowNotification("SaveAs failed, does not exist Machine Config!", 3, LogType.Error);
                return;
            }

            string path = EditorUtility.SaveFilePanel("Save Machine Config", Application.dataPath, "NewMachineData.json", null);
            //Cancel
            if (string.IsNullOrEmpty(path)) return;
            Save(path);
            Debug.Log("SaveAs success. Path: " + path);
            ShowNotification("SaveAs success", 3, null);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!configAsset)
            {
                string projectRelativePath = null;
                //Is root path
                if (Path.IsPathRooted(path))
                {
                    //Is in project Assets
                    if (path.StartsWith(Application.dataPath))
                        projectRelativePath = Path.GetRelativePath(Path.GetDirectoryName(Application.dataPath), path);
                }
                else
                    projectRelativePath = path;

                if (!string.IsNullOrEmpty(projectRelativePath))
                    ConfigAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(projectRelativePath);
            }
        }

        public void Reload()
        {
            if (!TryReload(out var error))
            {
                if (!string.IsNullOrEmpty(error))
                    ShowNotification("Reload failed. " + error, 3, LogType.Error);
            }
            else
            {
                ShowNotification("Reload success", 3, null);
                Debug.Log("Reload success. Path: " + AssetDatabase.GetAssetPath(configAsset));
            }
        }

        public void ClearConfig()
        {
            CurMachine = new MachineConfig();
        }

        private bool TryReload()
        {
            return TryReload(out _);
        }
        
        private bool TryReload(out string error)
        {
            error = null;
            if (!configAsset)
            {
                error = "Does not exist Config Asset!";
                return false;
            }
            MachineConfig newMachine = null;
            try
            {
                newMachine = MachineHelper.DeserializeMachineConfig(configAsset.text);
            }
            catch (Exception e)
            {
                error = e.ToString();
                return false;
            }
            if (newMachine == null)
            {
                error = "New Machine Config is null!";
                return false;
            }
            CurMachine = newMachine;
            OnReload?.Invoke();
            return true;
        }
    }
}
