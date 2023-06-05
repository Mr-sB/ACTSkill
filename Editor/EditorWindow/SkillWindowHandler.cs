using System;
using System.Collections;
using ACTSkill;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ACTSkillEditor
{
    public abstract class SkillWindowHandlerBase
    {
        public readonly ACTSkillEditorWindow Owner;

        public SkillWindowHandlerBase(ACTSkillEditorWindow owner)
        {
            Owner = owner;
        }
        
        public virtual void OnEnable() { }
        
        public virtual void OnDisable() { }

        public virtual void BeginOnGUI() { }
        
        public virtual void EndOnGUI() { }
    }

    public class EditorSkillWindowHandler : SkillWindowHandlerBase
    {
        private AnimationProcessor animationProcessor;
        private EditorGUI.DisabledScope disabledScope;

        public EditorSkillWindowHandler(ACTSkillEditorWindow owner) : base(owner)
        {
        }

        public override void OnEnable()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<AnimationProcessor>())
            {
                if (!type.IsAbstract && !type.IsGenericType && type != typeof(DefaultAnimationProcessor))
                {
                    try
                    {
                        animationProcessor = (AnimationProcessor) Activator.CreateInstance(type, new object[] {this});
                        break;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            animationProcessor ??= new DefaultAnimationProcessor(Owner);
            // Debug.Log($"Use {animationProcessor.GetType().FullName} to process animation.");
            
            animationProcessor.OnEnable();
        }

        public override void OnDisable()
        {
            animationProcessor.OnDisable();
            disabledScope.Dispose();
        }

        public override void BeginOnGUI()
        {
            disabledScope = new EditorGUI.DisabledScope(false);
        }

        public override void EndOnGUI()
        {
            disabledScope.Dispose();
        }
        
        public void RefreshAnimationProcessor()
        {
            animationProcessor?.OnRefresh();
        }
    }
    
    public class RuntimeSkillWindowHandler : SkillWindowHandlerBase
    {
        private EditorCoroutine updateCoroutine;
        private EditorWaitForSeconds waitForSeconds = new EditorWaitForSeconds(0.0167f);
        private bool playing;
        
        private EditorGUI.DisabledScope disabledScope;
        private GameObject oriTarget;
        private IMachineController controller;

        public RuntimeSkillWindowHandler(ACTSkillEditorWindow owner) : base(owner)
        {
        }

        public override void OnEnable()
        {
            oriTarget = Owner.Target;
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        public override void OnDisable()
        {
            RecoverEditorValue();
            Selection.selectionChanged -= OnSelectionChanged;
            disabledScope.Dispose();
            Pause();
        }

        public override void BeginOnGUI()
        {
            disabledScope = new EditorGUI.DisabledScope(true);
        }

        public override void EndOnGUI()
        {
            disabledScope.Dispose();
        }

        private void SetSelectedTarget(GameObject target)
        {
            controller = null;
            if (target && Application.IsPlaying(target))
                controller = target.GetComponent<IMachineController>();
            if ((controller is Object obj && !obj) || controller?.MachineBehaviour?.MachineConfig == null)
            {
                if (target != oriTarget)
                {
                    //Try oriTarget
                    SetSelectedTarget(oriTarget);
                }
                else
                {
                    //Recover
                    RecoverEditorValue();
                    Pause();
                }
            }
            else
            {
                //Apply runtime controller
                Owner.Target = target;
                Owner.Repaint();
                Play();
            }
        }
        
        private void OnSelectionChanged()
        {
            SetSelectedTarget(Selection.activeGameObject);
        }

        private void RecoverEditorValue()
        {
            Owner.Target = oriTarget;
            Owner.Repaint();
        }
        
        private void Play()
        {
            playing = true;
            if (updateCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(updateCoroutine);
            updateCoroutine = EditorCoroutineUtility.StartCoroutine(EditorUpdate(), this);
            RefreshSelectedFrame();
        }

        private void Pause()
        {
            playing = false;
            if (updateCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        
        private IEnumerator EditorUpdate()
        {
            while (playing)
            {
                yield return waitForSeconds;
                if ((controller is Object obj && !obj) || controller?.MachineBehaviour?.CurStateName == null) continue;
                string stateName = controller.MachineBehaviour.CurStateName;
                var stateIndex = controller.MachineBehaviour.MachineConfig.States.FindIndex(state => state.StateName == stateName);
                if (Owner.SelectedStateIndex != stateIndex || Owner.SelectedFrameIndex != controller.MachineBehaviour.StateHandler.Frame)
                {
                    Owner.SelectedStateIndex = stateIndex;
                    Owner.SelectedFrameIndex = controller.MachineBehaviour.StateHandler.Frame;
                    RefreshSelectedFrame();
                }
            }
        }
        
        private void RefreshSelectedFrame()
        {
            Owner.ScrollFrameToView();
            Owner.Repaint();
        }
    }
}
