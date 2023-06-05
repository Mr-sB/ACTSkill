using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using ACTSkill;
using CustomizationInspector.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ACTSkillEditor
{
    public class TimelineView : CopyableViewBase
    {
        private string title;
        public override string Title
        {
            get => title;
            set => title = value;
        }

        private Vector2 scrollPosition = Vector2.zero;

        #region Styles
        
        public const float ELEMENT_SPACE = 1f;
        public const float FRAME_HEAD_HEIGHT = 35f;
        public const float FRAME_WIDTH = 35f;
        public const float FRAME_SPACE = 1f;
        public const float ACTION_HEAD_WIDTH = 26f;
        public const float ACTION_HEIGHT = 26f;
        public const float ACTION_SPACE = 1f;
        public const float ACTION_DRAGGABLE_SPACE = 4;
        public static readonly float ToolBarHeight = EditorGUIUtility.singleLineHeight + 2;
        private static float? barSize;
        public static float BarSize
        {
            get
            {
                //You can only call GUI functions from inside OnGUI.
                return barSize ??= GUI.skin.horizontalScrollbar.fixedHeight;
            }
        }
        private static GUIStyle actionBarStyle;

        public static GUIStyle ActionBarStyle
        {
            get
            {
                //set_fixedHeight is not allowed to be called from a ScriptableObject constructor (or instance field initializer)
                return actionBarStyle ??= new GUIStyle(EditorStyles.miniButtonMid) {fixedHeight = ACTION_HEIGHT};
            }
        }

        #endregion

        private Vector2 dragOffset;
        private WrapperSO wrapperSO;
        private bool playing;
        private float playSpeed = 1;
        private float lastChangeFrameTime = 0;
        private EditorCoroutine updateCoroutine;
        private EditorWaitForSeconds waitForSeconds = new EditorWaitForSeconds(0.0167f);

        public TimelineView(ACTSkillEditorWindow owner) : base(owner)
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

        public override void OnEnable()
        {
            if (string.IsNullOrEmpty(title))
                title = ObjectNames.NicifyVariableName(nameof(TimelineView));
            Owner.PropertyChanging += OnOwnerPropertyChanging;
            Owner.PropertyChanged += OnOwnerPropertyChanged;
            GetOrCreateWrapperSO();
        }

        public override void OnDisable()
        {
            Pause();
            if (Owner)
            {
                Owner.PropertyChanging -= OnOwnerPropertyChanging;
                Owner.PropertyChanged -= OnOwnerPropertyChanged;
            }

            if (wrapperSO)
            {
                wrapperSO.OnValidateEvent -= OnWrapperSOValidate;
                Object.DestroyImmediate(wrapperSO);
            }
        }

        protected override void OnGUI(Rect contentRect)
        {
            Rect toolBarRect = contentRect;
            toolBarRect.height = ToolBarHeight;
            DrawToolBar(toolBarRect);
            Rect frameRect = contentRect;
            frameRect.y = toolBarRect.yMax + ELEMENT_SPACE;
            frameRect.height -= ToolBarHeight + ELEMENT_SPACE;
            DrawFrames(frameRect);
        }

        public override object CopyData()
        {
            if (Owner.CurFrames == null) return null;
            List<FrameConfig> copy = new List<FrameConfig>(Owner.CurFrames.Count);
            foreach (var frame in Owner.CurFrames)
                copy.Add(frame?.Clone());
            return copy;
        }

        public override void PasteData(object data)
        {
            if (Owner.CurFrames == null || data is not List<FrameConfig> other) return;
            Owner.CurFrames.Clear();
            foreach (var frame in other)
                Owner.CurFrames.Add(frame?.Clone());
        }
        
        private void DrawFrames(Rect rect)
        {
            if (Owner.CurState == null) return;
            
            int frameCount = Owner.CurFrames.Count;
            int actionCount = Owner.CurActionConfig?.Actions.Count ?? 0;

            float scrollViewHeight = rect.height - FRAME_HEAD_HEIGHT - BarSize;
            float scrollViewWidth = (FRAME_WIDTH + FRAME_SPACE) * frameCount - FRAME_SPACE;

            float minViewWidth = rect.width - ACTION_HEAD_WIDTH - BarSize;
            if (scrollViewWidth < minViewWidth)
                scrollViewWidth = minViewWidth;

            float actionsHeight = (ACTION_HEIGHT + ACTION_SPACE) * actionCount - ACTION_SPACE;
            if (actionsHeight > scrollViewHeight)
                scrollViewHeight = actionsHeight;

            #region Frame
            
            Rect framePosition = new Rect(rect.x + ACTION_HEAD_WIDTH, rect.y, rect.width - ACTION_HEAD_WIDTH - BarSize, rect.height - BarSize);
            Rect frameViewRect = new Rect(framePosition.x, framePosition.y, scrollViewWidth, framePosition.height);
            GUI.BeginScrollView(framePosition, new Vector2(scrollPosition.x, 0), frameViewRect, GUIStyle.none, GUIStyle.none);
            for (int i = 0; i < frameCount; i++)
            {
                Rect headRect = new Rect(frameViewRect.x + (FRAME_WIDTH + FRAME_SPACE) * i, frameViewRect.y, FRAME_WIDTH, FRAME_HEAD_HEIGHT);
                Rect itemRect = headRect;
                itemRect.y += FRAME_HEAD_HEIGHT;
                itemRect.height = frameViewRect.height - FRAME_HEAD_HEIGHT;
                
                bool selected = Owner.SelectedFrameIndex == i;
                
                FrameConfig config = Owner.CurFrames[i];

                string title = string.Format("{0}\n{1}|{2}", i,
                    config.AttackRange.ModifyRange ? (config.AttackRange.Ranges?.Count ?? 0).ToString() : "<-",
                    config.BodyRange.ModifyRange ? (config.BodyRange.Ranges?.Count ?? 0).ToString() : "<-");
                if (GUI.Button(headRect, title, selected ? GUIStyleHelper.ItemHeadSelect : GUIStyleHelper.ItemHeadNormal))
                    SelectFrame(i, true);
                GUI.Box(itemRect, GUIContent.none, selected ? GUIStyleHelper.ItemBodySelect : GUIStyleHelper.ItemBodyNormal);
            }
            GUI.EndScrollView();

            #endregion

            #region Action
            
            Rect actionPosition = new Rect(rect.x + ACTION_HEAD_WIDTH, rect.y + FRAME_HEAD_HEIGHT, rect.width - ACTION_HEAD_WIDTH, rect.height - FRAME_HEAD_HEIGHT);
            Rect actionViewRect = new Rect(actionPosition.x, actionPosition.y, scrollViewWidth, scrollViewHeight);
            
            //Control scroll
            scrollPosition = GUI.BeginScrollView(actionPosition, scrollPosition, actionViewRect, true, true);
            for (int i = 0; i < actionCount; i++)
            {
                IAction action = Owner.CurActionConfig.Actions[i];
                //Do not draw null action
                if (action == null) continue;
                
                int beginFrame;
                int endFrame;
                
                if (action.Full)
                {
                    beginFrame = 0;
                    endFrame = frameCount - 1;
                }
                else
                {
                    beginFrame = Mathf.Clamp(action.BeginFrame, 0, frameCount - 1);
                    endFrame = Mathf.Clamp(action.EndFrame, beginFrame, frameCount - 1);
                }
                
                Rect actionRect = new Rect( actionViewRect.x + beginFrame * (FRAME_WIDTH + FRAME_SPACE), actionViewRect.y + (ACTION_HEIGHT + ACTION_SPACE) * i,
                    (endFrame - beginFrame + 1) * (FRAME_WIDTH + FRAME_SPACE) - FRAME_SPACE, ACTION_HEIGHT);
                //Bar
                GUI.Label(actionRect, GUIContent.none, ActionBarStyle);
                GUIContent nameContent = EditorUtil.TempContent(action.GetType().FullName);
                var nameSize = GUIStyleHelper.LabelMiddleCenter.CalcSize(nameContent);
                Rect nameRect = actionRect;
                if (nameSize.x > nameRect.width)
                    nameRect.width = nameSize.x;
                GUI.Label(nameRect, nameContent, GUIStyleHelper.LabelMiddleCenter);
                //Loop
                Rect loopRect = new Rect(actionRect.x + ACTION_DRAGGABLE_SPACE, actionRect.y + ACTION_HEIGHT / 2, FRAME_WIDTH / 2, ACTION_HEIGHT / 2);
                if (GUI.Button(loopRect, action.Loop ? GUIStyleHelper.LoopOnTexture : GUIStyleHelper.LoopOffTexture, GUIStyle.none))
                {
                    action.Loop = !action.Loop;
                    Event.current.Use();
                }
                //Draggable frame
                if (!action.Full)
                {
                    //Left
                    Rect leftDragRect = actionRect;
                    leftDragRect.xMax = leftDragRect.x + ACTION_DRAGGABLE_SPACE;
                    var delta = EditorGUIExtensions.SlideRect(Vector2.zero, leftDragRect, MouseCursor.ResizeHorizontal).x;
                    if (delta != 0)
                    {
                        int crossFrame = Mathf.RoundToInt(delta / (FRAME_WIDTH + FRAME_SPACE));
                        //Cross at least one frame
                        if (crossFrame != 0)
                            action.BeginFrame = Mathf.Clamp(beginFrame + crossFrame, 0, endFrame);
                    }
                    //Right
                    Rect rightDragRect = actionRect;
                    rightDragRect.xMin = rightDragRect.xMax - ACTION_DRAGGABLE_SPACE;
                    delta = EditorGUIExtensions.SlideRect(Vector2.zero, rightDragRect, MouseCursor.ResizeHorizontal).x;
                    if (delta != 0)
                    {
                        int crossFrame = Mathf.RoundToInt(delta / (FRAME_WIDTH + FRAME_SPACE));
                        //Cross at least one frame
                        if (crossFrame != 0)
                            action.EndFrame = Mathf.Clamp(endFrame + crossFrame, beginFrame, frameCount - 1);
                    }
                    //Middle
                    Rect middleDragRect = actionRect;
                    middleDragRect.xMin += ACTION_DRAGGABLE_SPACE;
                    middleDragRect.xMax -= ACTION_DRAGGABLE_SPACE;
                    //SlideRect will call Event.current.Use() to change event type to Used, so record event type first.
                    var eventType = Event.current.type;
                    var offset = EditorGUIExtensions.SlideRect(Vector2.zero, middleDragRect, null);
                    delta = offset.x;
                    if (delta != 0)
                    {
                        if (eventType == EventType.MouseDown)
                        {
                            dragOffset = offset;
                            SelectAction(i, true);
                        }
                        else
                        {
                            delta -= dragOffset.x;
                            int crossFrame = Mathf.RoundToInt(delta / (FRAME_WIDTH + FRAME_SPACE));
                            //Cross at least one frame
                            if (crossFrame != 0)
                            {
                                //Clamp crossFrame
                                if (crossFrame > 0)
                                {
                                    var newEndFrame = Mathf.Min(endFrame + crossFrame, frameCount - 1);
                                    crossFrame = newEndFrame - endFrame;
                                }
                                else
                                {
                                    var newBeginFrame = Mathf.Max(beginFrame + crossFrame, 0);
                                    crossFrame = newBeginFrame - beginFrame;
                                }
                                //Cross at least one frame
                                if (crossFrame != 0)
                                {
                                    action.BeginFrame = Mathf.Clamp(beginFrame + crossFrame, 0, frameCount - 1);
                                    //Read from action
                                    var newBeginFrame = Mathf.Clamp(action.BeginFrame, 0, frameCount - 1);
                                    action.EndFrame = Mathf.Clamp(endFrame + crossFrame, newBeginFrame, frameCount - 1);
                                }
                            }
                        }
                    }
                }
                else if (GUI.Button(actionRect, GUIContent.none, GUIStyle.none))
                    SelectAction(i, true);
            }
            GUI.EndScrollView();

            #endregion
            
            #region ActionHead
            
            Rect actionHeadPosition = new Rect(rect.x, rect.y + FRAME_HEAD_HEIGHT, ACTION_HEAD_WIDTH, rect.height - FRAME_HEAD_HEIGHT - BarSize);
            Rect actionHeadViewRect = new Rect(actionHeadPosition.x, actionHeadPosition.y, actionHeadPosition.width, scrollViewHeight);
            GUI.BeginScrollView(actionHeadPosition, new Vector2(0, scrollPosition.y), actionHeadViewRect, GUIStyle.none, GUIStyle.none);
            for (int i = 0; i < actionCount; i++)
            {
                IAction action = Owner.CurActionConfig.Actions[i];
                //Do not draw null action
                if (action == null) continue;
                Rect headRect = new Rect(actionViewRect.x - ACTION_HEAD_WIDTH, actionViewRect.y + (ACTION_HEIGHT + ACTION_SPACE) * i, ACTION_HEAD_WIDTH,
                    ACTION_HEIGHT);

                bool selected = Owner.SelectedActionIndex == i;

                if (GUI.Button(headRect, i.ToString(), selected ? GUIStyleHelper.ItemHeadSelect : GUIStyleHelper.ItemHeadNormal))
                    SelectAction(i, true);
            }
            GUI.EndScrollView();

            #endregion
        }

        private void DrawToolBar(Rect rect)
        {
            GUILayout.BeginArea(rect, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            if (Owner.CurState == null)
            {
                EndDraw();
                return;
            }
            
            if (GUILayout.Button(EditorUtil.TempImageOrTextContent(
                "First Frame", GUIStyleHelper.FirstKeyButtonContent.image, "Go to the beginning of the timeline"),
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                SelectFrame(0, true);

            if (GUILayout.Button(EditorUtil.TempImageOrTextContent(
                    "Prev Frame", GUIStyleHelper.PrevKeyButtonContent.image, "Go to the previous frame"),
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                var newFrame = Owner.SelectedFrameIndex - 1;
                if (newFrame < 0)
                    newFrame = Owner.CurFrames.Count - 1;
                else
                    Math.Clamp(newFrame, 0, Owner.CurFrames.Count - 1);
                SelectFrame(newFrame, true);
            }

            if (playing != GUILayout.Toggle(playing, EditorUtil.TempImageOrTextContent(
                    "Play", GUIStyleHelper.PlayButtonContent.image, "Play the timeline"),
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                if (playing)
                    Pause();
                else
                    Play();
            }

            if (GUILayout.Button(EditorUtil.TempImageOrTextContent(
                    "Next Frame", GUIStyleHelper.NextKeyButtonContent.image, "Go to the next frame"),
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                var newFrame = Owner.SelectedFrameIndex + 1;
                if (newFrame >= Owner.CurFrames.Count)
                    newFrame = 0;
                else
                    Math.Clamp(newFrame, 0, Owner.CurFrames.Count - 1);
                SelectFrame(newFrame, true);
            }

            if (GUILayout.Button(EditorUtil.TempImageOrTextContent(
                "Last Frame", GUIStyleHelper.LastKeyButtonContent.image, "Go to the end of the timeline"),
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                SelectFrame(Owner.CurFrames.Count - 1, true);
            
            if (GUILayout.Button(EditorUtil.TempImageOrTextContent(
                    "Refresh", GUIStyleHelper.RefreshButtonContent.image, "Refresh animation"),
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                Owner.RefreshAnimationProcessor();

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            
            GUIContent content = EditorUtil.TempContent("Frame");
            EditorGUIUtility.labelWidth = EditorStyles.numberField.CalcSize(content).x;
            var selectIndex = EditorGUILayout.IntSlider(content, Owner.SelectedFrameIndex, -1, Owner.CurFrames.Count - 1);
            if (selectIndex != Owner.SelectedFrameIndex)
                SelectFrame(selectIndex, false);
            
            content = EditorUtil.TempContent("Speed");
            EditorGUIUtility.labelWidth = EditorStyles.numberField.CalcSize(content).x;
            playSpeed = EditorGUILayout.Slider(content, playSpeed, 0, 1);
            
            content = EditorUtil.TempContent("Frame Count");
            EditorGUIUtility.labelWidth = EditorStyles.numberField.CalcSize(content).x;
            int frameCount = EditorGUILayout.DelayedIntField(content, Owner.CurFrames.Count, GUILayout.MaxWidth(EditorGUIUtility.labelWidth + 50));
            if (frameCount != Owner.CurFrames.Count)
            {
                if (frameCount <= 0)
                    Owner.CurFrames.Clear();
                else if (frameCount > Owner.CurFrames.Count)
                {
                    for (int i = Owner.CurFrames.Count; i < frameCount; i++)
                        Owner.CurFrames.Add(new FrameConfig());
                }
                else
                    Owner.CurFrames.RemoveRange(frameCount, Owner.CurFrames.Count - frameCount);
            }
            
            EditorGUIUtility.labelWidth = oldLabelWidth;

            EndDraw();
        }

        private void SelectFrame(int frameIndex, bool clearKeyboardControl)
        {
            Owner.SelectedFrameIndex = frameIndex;
            GetOrCreateWrapperSO().Data = Owner.CurFrameConfig;
            if (frameIndex >= 0)
                Selection.activeObject = GetOrCreateWrapperSO();
            if (clearKeyboardControl)
            {
                Event.current.Use();
                //Cancel keyboardControl
                GUIUtility.keyboardControl = 0;
            }
        }
        
        private void SelectAction(int actionIndex, bool clearKeyboardControl)
        {
            Owner.SelectedActionIndex = actionIndex;
            GetOrCreateWrapperSO().Data = Owner.CurAction;
            if (actionIndex >= 0)
                Selection.activeObject = GetOrCreateWrapperSO();
            if (clearKeyboardControl)
            {
                Event.current.Use();
                //Cancel keyboardControl
                GUIUtility.keyboardControl = 0;
            }
        }

        public void ScrollFrameToView(Rect rect)
        {
            if (Owner.CurState == null) return;
            var frameCount = Owner.CurState.Frames.Count;
            float scrollViewHeight = rect.height - FRAME_HEAD_HEIGHT - BarSize;
            float scrollViewWidth = (FRAME_WIDTH + FRAME_SPACE) * frameCount - FRAME_SPACE;

            float minViewWidth = rect.width - ACTION_HEAD_WIDTH - BarSize;
            if (scrollViewWidth < minViewWidth)
                scrollViewWidth = minViewWidth;
            Rect actionPosition = new Rect(rect.x + ACTION_HEAD_WIDTH, rect.y + FRAME_HEAD_HEIGHT, rect.width - ACTION_HEAD_WIDTH, rect.height - FRAME_HEAD_HEIGHT);
            Rect actionViewRect = new Rect(actionPosition.x, actionPosition.y, scrollViewWidth, scrollViewHeight);
            if (actionPosition.width >= actionViewRect.width) return;

            float scrollOffsetX = scrollPosition.x;
            float scrollViewXMin = actionPosition.x + scrollOffsetX;
            float scrollViewXMax = actionPosition.xMax + scrollOffsetX - BarSize;

            float frameXMin = actionPosition.x + (FRAME_WIDTH + FRAME_SPACE) * Owner.SelectedFrameIndex;
            float frameXMax = frameXMin + FRAME_WIDTH;
            
            float? adjustOffsetX = null;
            if (scrollViewXMin > frameXMin)
                adjustOffsetX = frameXMin - scrollViewXMax;
            else if (scrollViewXMax < frameXMax)
                adjustOffsetX = frameXMax - scrollViewXMax;
            
            if (!adjustOffsetX.HasValue) return;

            scrollPosition.x = scrollOffsetX + adjustOffsetX.Value;
        }
        
        private void Play()
        {
            playing = true;
            lastChangeFrameTime = Time.realtimeSinceStartup;
            if (updateCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(updateCoroutine);
            updateCoroutine = EditorCoroutineUtility.StartCoroutine(EditorUpdate(), this);
            RefreshSelectedFrame();
        }

        private void Pause()
        {
            playing = false;
            lastChangeFrameTime = 0;
            if (updateCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        
        private IEnumerator EditorUpdate()
        {
            while (playing)
            {
                yield return waitForSeconds;
                if (!playing) yield break;
                if (Owner.CurState == null) continue;
                var curTime = Time.realtimeSinceStartup;
                var deltaTime = (curTime - lastChangeFrameTime) * playSpeed;
                int frame = Mathf.FloorToInt(deltaTime * Owner.CurMachine.FrameRate);
                if (frame > 0)
                {
                    var remainTime = deltaTime - (float) frame / Owner.CurMachine.FrameRate;
                    lastChangeFrameTime = curTime - remainTime;
                    int newIndex = (Owner.SelectedFrameIndex + frame) % Owner.CurFrames.Count;
                    if (newIndex != Owner.SelectedFrameIndex)
                    {
                        Owner.SelectedFrameIndex = newIndex;
                        RefreshSelectedFrame();
                    }
                }
            }
        }

        private void RefreshSelectedFrame()
        {
            Owner.ScrollFrameToView();
            Owner.Repaint();
        }

        private static void EndDraw()
        {
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        
        private void OnOwnerPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(ACTSkillEditorWindow.CurFrameConfig))
            {
                if (Selection.activeObject == wrapperSO && wrapperSO.Data == Owner.CurFrameConfig)
                    Selection.activeObject = null;
            }
            else if (e.PropertyName == nameof(ACTSkillEditorWindow.CurAction))
            {
                if (Selection.activeObject is WrapperSO so && so.Data == Owner.CurAction)
                    Selection.activeObject = null;
            }
        }
        
        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ACTSkillEditorWindow.CurFrames))
                Pause();
        }

        private void OnWrapperSOValidate()
        {
            if (Owner)
            {
                Owner.Repaint();
                ACTSkillEditorWindow.RepaintSceneViews();
            }
        }
    }
}
