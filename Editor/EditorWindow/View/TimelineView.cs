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

        public List<FrameConfig> Data => Owner ? Owner.CurFrames : null;
        private Vector2 dragOffset;

        private static readonly string frameWrapperName = typeof(FrameConfig).FullName;
        private string actionWrapperName;
        // private GUIContent frameGUIContent;
        // private GUIContent actionGUIContent;
        private WrapperSO frameWrapperSO;
        private WrapperSO actionWrapperSO;
        private bool playing;
        private float playSpeed = 1;
        private float lastChangeFrameTime = 0;
        private EditorCoroutine updateCoroutine;
        private EditorWaitForSeconds waitForSeconds = new EditorWaitForSeconds(0.0167f);

        public TimelineView(ACTSkillEditorWindow owner) : base(owner)
        {
        }
        
        private WrapperSO GetOrCreateFrameWrapperSO()
        {
            if (!frameWrapperSO)
                frameWrapperSO = ScriptableObject.CreateInstance<WrapperSO>();
            frameWrapperSO.NameGetter = () => frameWrapperName;
            frameWrapperSO.DrawInspectorGUI ??= () =>
            {
                var property = Owner.CurFrameConfigProperty;
                if (property == null) return;
                Owner.SerializedObject?.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                // frameGUIContent.tooltip = property.tooltip;
                // EditorGUILayout.PropertyField(property, frameGUIContent, true);
                EditorGUILayout.PropertyField(property, true);
                Owner.ApplyModifiedProperties();
                if (EditorGUI.EndChangeCheck())
                {
                    Owner.ApplyModifiedProperties();
                    Owner.Repaint();
                }
            };
            frameWrapperSO.DoCopy ??= () => ACTSkillEditorWindow.CopyBuffer = Owner.CurFrameConfig?.Clone();
            frameWrapperSO.DoPaste ??= () =>
            {
                Owner.RecordObject("Paste frame config");
                Owner.CurFrameConfig?.Copy(ACTSkillEditorWindow.CopyBuffer);
                Owner.Repaint();
            };
            return frameWrapperSO;
        }
        
        private WrapperSO GetOrCreateActionWrapperSO()
        {
            if (!actionWrapperSO)
                actionWrapperSO = ScriptableObject.CreateInstance<WrapperSO>();
            actionWrapperSO.NameGetter = () => Owner.CurActionProperty?.GetObject()?.GetType().FullName ?? "Action";
            actionWrapperSO.DrawInspectorGUI ??= () =>
            {
                var property = Owner.CurActionProperty;
                if (property == null) return;
                Owner.SerializedObject?.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                // actionGUIContent.text = property.GetObject()?.GetType().FullName ?? TypeDropdown.NULL_TYPE_NAME;
                // actionGUIContent.tooltip = property.tooltip;
                // EditorGUILayout.PropertyField(property, actionGUIContent, true);
                EditorGUILayout.PropertyField(property, true);
                if (EditorGUI.EndChangeCheck())
                {
                    Owner.ApplyModifiedProperties();
                    Owner.Repaint();
                }
            };
            actionWrapperSO.DoCopy ??= () => ACTSkillEditorWindow.CopyBuffer = Owner.CurAction?.Clone();
            actionWrapperSO.DoPaste ??= () =>
            {
                Owner.RecordObject("Paste action");
                Owner.CurAction?.Copy(ACTSkillEditorWindow.CopyBuffer);
                Owner.Repaint();
            };
            return actionWrapperSO;
        }

        public override void OnEnable()
        {
            if (string.IsNullOrEmpty(title))
                title = ObjectNames.NicifyVariableName(nameof(TimelineView));
            // frameGUIContent = new GUIContent(typeof(FrameConfig).FullName);
            // actionGUIContent = new GUIContent();
            Owner.PropertyChanging += OnOwnerPropertyChanging;
            Owner.PropertyChanged += OnOwnerPropertyChanged;
        }

        public override void OnDisable()
        {
            Pause();
            if (Owner)
            {
                Owner.PropertyChanging -= OnOwnerPropertyChanging;
                Owner.PropertyChanged -= OnOwnerPropertyChanged;
            }

            if (frameWrapperSO)
                Object.DestroyImmediate(frameWrapperSO);
            if (actionWrapperSO)
                Object.DestroyImmediate(actionWrapperSO);
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
            // Repaint inspector
            if (frameWrapperSO && Selection.activeObject == frameWrapperSO)
                EditorUtility.SetDirty(frameWrapperSO);
            if (actionWrapperSO && Selection.activeObject == actionWrapperSO)
                EditorUtility.SetDirty(actionWrapperSO);
        }

        public override object CopyData()
        {
            if (Data == null) return null;
            List<FrameConfig> copy = new List<FrameConfig>(Data.Count);
            foreach (var frame in Data)
                copy.Add(frame?.Clone());
            return copy;
        }

        public override void PasteData(object data)
        {
            if (Data == null || data is not List<FrameConfig> other) return;
            Owner.RecordObject("Paste frame list");
            Data.Clear();
            foreach (var frame in other)
                Data.Add(frame?.Clone());
            Owner.SelectedFrameIndex = -1;
        }
        
        private void DrawFrames(Rect rect)
        {
            var frameListProperty = Owner.CurFrameListProperty;
            if (frameListProperty == null) return;
            
            int frameCount = frameListProperty.arraySize;
            var actionListProperty = Owner.CurActionListProperty;
            int actionCount = actionListProperty?.arraySize ?? 0;

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
                
                var frameProperty = frameListProperty.GetArrayElementAtIndex(i);
                var attackRangeProperty = frameProperty.FindPropertyRelative(nameof(FrameConfig.AttackRange));
                var bodyRangeProperty = frameProperty.FindPropertyRelative(nameof(FrameConfig.BodyRange));

                string title = string.Format("{0}\n{1}|{2}", i,
                    attackRangeProperty?.FindPropertyRelative(nameof(RangeConfig.ModifyRange))?.boolValue ?? false
                        ? (attackRangeProperty.FindPropertyRelative(nameof(RangeConfig.Ranges))?.arraySize ?? 0).ToString()
                        : "<-",
                    bodyRangeProperty?.FindPropertyRelative(nameof(RangeConfig.ModifyRange))?.boolValue ?? false
                        ? (bodyRangeProperty.FindPropertyRelative(nameof(RangeConfig.Ranges))?.arraySize ?? 0).ToString()
                        : "<-");
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
                //Do not draw null action
                var property = actionListProperty?.GetArrayElementAtIndex(i);
                if (property?.managedReferenceValue is not ActionBase action) continue;
                
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
                    Owner.RecordObject("Change action loop");
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
                        {
                            Owner.RecordObject("Change action begin frame");
                            action.BeginFrame = Mathf.Clamp(beginFrame + crossFrame, 0, endFrame);
                        }
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
                        {
                            Owner.RecordObject("Change action end frame");
                            action.EndFrame = Mathf.Clamp(endFrame + crossFrame, beginFrame, frameCount - 1);
                        }
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
                                    Owner.RecordObject("Change action frame");
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
                var property = actionListProperty?.GetArrayElementAtIndex(i);
                //Do not draw null action
                if (property?.managedReferenceValue is not ActionBase) continue;
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
                    newFrame = Data.Count - 1;
                else
                    Math.Clamp(newFrame, 0, Data.Count - 1);
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
                if (newFrame >= Data.Count)
                    newFrame = 0;
                else
                    Math.Clamp(newFrame, 0, Data.Count - 1);
                SelectFrame(newFrame, true);
            }

            if (GUILayout.Button(EditorUtil.TempImageOrTextContent(
                "Last Frame", GUIStyleHelper.LastKeyButtonContent.image, "Go to the end of the timeline"),
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                SelectFrame(Data.Count - 1, true);
            
            if (GUILayout.Button(EditorUtil.TempImageOrTextContent(
                    "Refresh", GUIStyleHelper.RefreshButtonContent.image, "Refresh animation"),
                EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                Owner.RefreshAnimationProcessor();

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            
            GUIContent content = EditorUtil.TempContent("Frame");
            EditorGUIUtility.labelWidth = EditorStyles.numberField.CalcSize(content).x;
            var selectIndex = EditorGUILayout.IntSlider(content, Owner.SelectedFrameIndex, -1, Data.Count - 1);
            if (selectIndex != Owner.SelectedFrameIndex)
                SelectFrame(selectIndex, false);
            
            content = EditorUtil.TempContent("Speed");
            EditorGUIUtility.labelWidth = EditorStyles.numberField.CalcSize(content).x;
            playSpeed = EditorGUILayout.Slider(content, playSpeed, 0, 1);
            
            content = EditorUtil.TempContent("Frame Count");
            EditorGUIUtility.labelWidth = EditorStyles.numberField.CalcSize(content).x;
            int frameCount = EditorGUILayout.DelayedIntField(content, Data.Count, GUILayout.MaxWidth(EditorGUIUtility.labelWidth + 50));
            if (frameCount != Data.Count)
            {
                if (frameCount <= 0)
                    Data.Clear();
                else if (frameCount > Data.Count)
                {
                    for (int i = Data.Count; i < frameCount; i++)
                        Data.Add(new FrameConfig());
                }
                else
                    Data.RemoveRange(frameCount, Data.Count - frameCount);
            }
            
            EditorGUIUtility.labelWidth = oldLabelWidth;

            EndDraw();
        }

        private void SelectFrame(int frameIndex, bool clearKeyboardControl)
        {
            Owner.SelectedFrameIndex = frameIndex;
            if (frameIndex >= 0)
                Selection.activeObject = GetOrCreateFrameWrapperSO();
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
            if (actionIndex >= 0)
                Selection.activeObject = GetOrCreateActionWrapperSO();
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
                    int newIndex = (Owner.SelectedFrameIndex + frame) % Data.Count;
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
                if (Selection.activeObject == frameWrapperSO)
                    Selection.activeObject = null;
            }
            else if (e.PropertyName == nameof(ACTSkillEditorWindow.CurAction))
            {
                if (Selection.activeObject == actionWrapperSO)
                    Selection.activeObject = null;
            }
        }
        
        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ACTSkillEditorWindow.CurFrames))
                Pause();
        }
    }
}
