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

        public const float VIEW_DRAGGABLE_SPACE = 4;
        public static readonly float MenuViewHeight = EditorGUIUtility.singleLineHeight + 3;

        private const float MIN_WIDTH_RATIO = 0.1f;
        private const float MAX_WIDTH_RATIO = 0.7f;
        private const float MIN_HEIGHT_RATIO = 0.1f;
        private const float MAX_HEIGHT_RATIO = 0.9f;

        public float FlexibleWidth => position.width;
        public float FlexibleHeight => position.height - MenuViewHeight;
        private float stateListWidthRatio = 0.25f;
        private float stateListMaxWidthRatio => Mathf.Min(1 - (attackRangeWidthRatio + bodyRangeWidthRatio + MIN_WIDTH_RATIO), MAX_WIDTH_RATIO);
        public float stateListWidth => stateListWidthRatio * FlexibleWidth;
        private float stateListHeightRatio = 0.5f;
        public float stateListHeight => stateListHeightRatio * FlexibleHeight;
        
        public float stateSettingHeight => FlexibleHeight - stateListHeight;

        public float timelineWidth => FlexibleWidth - stateListWidth;
        private float timelineHeightRatio = 0.5f;
        public float timelineHeight => timelineHeightRatio * FlexibleHeight;
        
        private float attackRangeWidthRatio = 0.25f;
        private float attackRangeMaxWidthRatio => Mathf.Min(1 - (stateListWidthRatio + bodyRangeWidthRatio + MIN_WIDTH_RATIO), MAX_WIDTH_RATIO);
        public float attackRangeWidth => attackRangeWidthRatio * FlexibleWidth;
        public float attackRangeHeight => FlexibleHeight - timelineHeight;
        
        private float bodyRangeWidthRatio = 0.25f;
        private float bodyRangeMaxWidthRatio => Mathf.Min(1 - (stateListWidthRatio + attackRangeWidthRatio + MIN_WIDTH_RATIO), MAX_WIDTH_RATIO);
        public float bodyRangeWidth => bodyRangeWidthRatio * FlexibleWidth;

        public float actionListWidth => FlexibleWidth - stateListWidth - attackRangeWidth - bodyRangeWidth;
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
        
        private MachineConfig curMachine = new MachineConfig();
        public MachineConfig CurMachine
        {
            get => curMachine;
            set
            {
                if (curMachine == value) return;
                OnPropertyChanging();
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
        private IAction curAction;
        public IAction CurAction
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

        private void InitCurAction()
        {
            if (CurActionConfig?.Actions != null && selectedActionIndex >= 0 && selectedActionIndex < CurActionConfig.Actions.Count)
                CurAction = CurActionConfig.Actions[selectedActionIndex];
            else
                CurAction = null;
        }
        
        #endregion


        #region View

        private List<ViewBase> views;
        private MenuView menuView;
        private StateListView stateListView;
        private StateSettingView stateSettingView;
        private TimelineView timelineView;
        private RangeView attackRangeView;
        private RangeView bodyRangeView;
        private ActionListView actionListView;

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
            var window = GetWindow<ACTSkillEditorWindow>(false, ObjectNames.NicifyVariableName("ACTSkillEditor"), true);
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
            if (curMachine == null)
                CurMachine = new MachineConfig();
            
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            OnPlayModeStateChanged(Application.isPlaying ? PlayModeStateChange.EnteredPlayMode : PlayModeStateChange.EnteredEditMode);
            
            CreateViews();
            CreateSceneGUIs();
            
            ShowSceneGUI = EditorPrefs.GetBool(ShowSceneGUISaveKey, true);

            foreach (var view in views)
                view.OnEnable();
            
            foreach (var sceneGUI in sceneGUIs)
                sceneGUI.OnEnable();
        }
        
        private void CreateViews()
        {
            views = new List<ViewBase>();
            
            menuView = new MenuView(this);
            views.Add(menuView);
            
            stateListView = new StateListView(this);
            views.Add(stateListView);
            
            stateSettingView = new StateSettingView(this);
            views.Add(stateSettingView);
            
            timelineView = new TimelineView(this);
            views.Add(timelineView);
            
            attackRangeView = new RangeView(this, "Attack Range View", new AttackRangeViewHandler());
            views.Add(attackRangeView);
            
            bodyRangeView = new RangeView(this, "Body Range View", new BodyRangeViewHandler());
            views.Add(bodyRangeView);
            
            actionListView = new ActionListView(this);
            views.Add(actionListView);
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

        private void OnGUI()
        {
            skillWindowHandler.OnGUI();
            skillWindowHandler.BeginOnGUI();
            Rect viewRect = new Rect(0, 0, FlexibleWidth, MenuViewHeight);
            DrawMenuView(viewRect);
            
            Rect flexibleRect = new Rect(0, MenuViewHeight, FlexibleWidth, FlexibleHeight);
            viewRect = flexibleRect;
            viewRect.width = stateListWidth;
            viewRect.height = stateListHeight;
            DrawStateListView(viewRect);
            
            viewRect = flexibleRect;
            viewRect.y += stateListHeight;
            viewRect.width = stateListWidth;
            viewRect.height = stateSettingHeight;
            DrawStateSettingView(viewRect);

            viewRect = flexibleRect;
            viewRect.x += stateListWidth;
            viewRect.width = timelineWidth;
            viewRect.height = timelineHeight;
            DrawTimelineView(viewRect);
            
            viewRect = flexibleRect;
            viewRect.x += stateListWidth;
            viewRect.y += timelineHeight;
            viewRect.width = attackRangeWidth;
            viewRect.height = attackRangeHeight;
            DrawAttackRangeView(viewRect);
            
            viewRect = flexibleRect;
            viewRect.x += stateListWidth + attackRangeWidth;
            viewRect.y += timelineHeight;
            viewRect.width = bodyRangeWidth;
            viewRect.height = attackRangeHeight;
            DrawBodyRangeView(viewRect);
            
            viewRect = flexibleRect;
            viewRect.x += stateListWidth + attackRangeWidth + bodyRangeWidth;
            viewRect.y += timelineHeight;
            viewRect.width = actionListWidth;
            viewRect.height = attackRangeHeight;
            DrawActionListView(viewRect);
            skillWindowHandler.EndOnGUI();
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

            foreach (var view in views)
                view.OnDisable();
            views.Clear();
            
            foreach (var sceneGUI in sceneGUIs)
                sceneGUI.OnDisable();
            sceneGUIs.Clear();
            
            skillWindowHandler?.OnDisable();
            skillWindowHandler = null;
        }

        private void DrawMenuView(Rect rect)
        {
            menuView.Draw(rect);
        }
        
        private void DrawStateListView(Rect rect)
        {
            Rect viewRect = rect;
            viewRect.width -= VIEW_DRAGGABLE_SPACE;
            var dragRect = rect;
            dragRect.xMin = dragRect.xMax - VIEW_DRAGGABLE_SPACE;
            var delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeHorizontal).x;
            if (delta != 0)
                stateListWidthRatio = Mathf.Clamp((stateListWidth + delta) / FlexibleWidth, MIN_WIDTH_RATIO, stateListMaxWidthRatio);
            viewRect.width = stateListWidth - VIEW_DRAGGABLE_SPACE;
            
            dragRect = rect;
            dragRect.yMin = dragRect.yMax - VIEW_DRAGGABLE_SPACE;
            delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeVertical).y;
            if (delta != 0)
                stateListHeightRatio = Mathf.Clamp((stateListHeight + delta) / FlexibleHeight, MIN_HEIGHT_RATIO, MAX_HEIGHT_RATIO);
            viewRect.height = stateListHeight - VIEW_DRAGGABLE_SPACE;
            stateListView.Draw(viewRect);
        }
        
        private void DrawStateSettingView(Rect rect)
        {
            Rect viewRect = rect;
            viewRect.width -= VIEW_DRAGGABLE_SPACE;
            var dragRect = rect;
            dragRect.xMin = dragRect.xMax - VIEW_DRAGGABLE_SPACE;
            var delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeHorizontal).x;
            if (delta != 0)
                stateListWidthRatio = Mathf.Clamp((stateListWidth + delta) / FlexibleWidth, MIN_WIDTH_RATIO, stateListMaxWidthRatio);
            viewRect.width = stateListWidth - VIEW_DRAGGABLE_SPACE;
            stateSettingView.Draw(viewRect);
        }
        
        private void DrawTimelineView(Rect rect)
        {
            Rect viewRect = rect;
            viewRect.height -= VIEW_DRAGGABLE_SPACE;
            var dragRect = rect;
            dragRect.yMin = viewRect.yMax - VIEW_DRAGGABLE_SPACE;
            var delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeVertical).y;
            if (delta != 0)
                timelineHeightRatio = Mathf.Clamp((timelineHeight + delta) / FlexibleHeight, MIN_HEIGHT_RATIO, MAX_HEIGHT_RATIO);
            viewRect.height = timelineHeight - VIEW_DRAGGABLE_SPACE;
            timelineView.Draw(viewRect);
        }

        private void DrawAttackRangeView(Rect rect)
        {
            Rect viewRect = rect;
            viewRect.width -= VIEW_DRAGGABLE_SPACE;
            var dragRect = rect;
            dragRect.xMin = dragRect.xMax - VIEW_DRAGGABLE_SPACE;
            var delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeHorizontal).x;
            if (delta != 0)
                attackRangeWidthRatio = Mathf.Clamp((attackRangeWidth + delta) / FlexibleWidth, MIN_WIDTH_RATIO, attackRangeMaxWidthRatio);
            viewRect.width = attackRangeWidth - VIEW_DRAGGABLE_SPACE;
            attackRangeView.Draw(viewRect);
        }
        
        private void DrawBodyRangeView(Rect rect)
        {
            Rect viewRect = rect;
            viewRect.width -= VIEW_DRAGGABLE_SPACE;
            var dragRect = rect;
            dragRect.xMin = dragRect.xMax - VIEW_DRAGGABLE_SPACE;
            var delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeHorizontal).x;
            if (delta != 0)
                bodyRangeWidthRatio = Mathf.Clamp((bodyRangeWidth + delta) / FlexibleWidth, MIN_WIDTH_RATIO, bodyRangeMaxWidthRatio);
            viewRect.width = bodyRangeWidth - VIEW_DRAGGABLE_SPACE;
            bodyRangeView.Draw(viewRect);
        }

        private void DrawActionListView(Rect rect)
        {
            actionListView.Draw(rect);
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
        
        public void RefreshAnimationProcessor()
        {
            if (skillWindowHandler is EditorSkillWindowHandler editorSkillWindowHandler)
                editorSkillWindowHandler.RefreshAnimationProcessor();
        }

        public void ScrollFrameToView()
        {
            Rect flexibleRect = new Rect(0, MenuViewHeight, FlexibleWidth, FlexibleHeight);

            var viewRect = flexibleRect;
            viewRect.x += stateListWidth;
            viewRect.width = timelineWidth;
            // viewRect.height = timelineHeight;
            
            viewRect.height = timelineHeight - VIEW_DRAGGABLE_SPACE;
            timelineView.ScrollFrameToView(viewRect);
        }

        public void Save()
        {
            if (curMachine == null)
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
            File.WriteAllText(path, curMachine.Serialize(true));
        }

        public void SaveAs()
        {
            if (curMachine == null)
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
