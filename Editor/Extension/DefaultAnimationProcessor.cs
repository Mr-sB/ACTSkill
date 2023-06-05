using System;
using ACTSkill;
using UnityEditor.Animations;
using UnityEngine;

namespace ACTSkillEditor
{
    public class DefaultAnimationProcessor : AnimationProcessor
    {
        private Animator animator;
        private AnimationClip animationClip;
        
        public DefaultAnimationProcessor(ACTSkillEditorWindow owner) : base(owner)
        {
        }

        protected override void OnTargetChange(GameObject target)
        {
            animator = !target ? null : target.GetComponentInChildren<Animator>();
            if (!IsValidate()) return;
            OnStateChange(Owner.CurState);
        }

        protected override void OnStateChange(StateConfig stateConfig)
        {
            if (!IsValidate() || stateConfig == null)
                animationClip = null;
            else
            {
                string defaultAnimName = stateConfig.DefaultAnimName;
                animationClip = null;
                var animatorController = animator.runtimeAnimatorController as AnimatorController;
                foreach (var layer in animatorController.layers)
                {
                    if (!ForeachState(layer.stateMachine, animatorState =>
                    {
                        if (animatorState.name != defaultAnimName) return true;
                        return ForeachAnimationClip(animatorState.motion, clip =>
                        {
                            if (!clip) return true;
                            animationClip = clip;
                            return false;
                        });
                    }))
                    {
                        break;
                    }
                }
            }
            OnFrameChange(Owner.SelectedFrameIndex);
        }

        protected override void OnFrameChange(int frameIndex)
        {
            if (!IsValidate() || !animationClip) return;
            frameIndex = Mathf.Clamp(frameIndex, 0, Owner.CurState.Frames.Count);
            animationClip.SampleAnimation(animator.gameObject, (float)frameIndex / Owner.CurMachine.FrameRate);
        }

        public override void OnRefresh()
        {
            OnTargetChange(Owner.Target);
        }

        private bool IsValidate()
        {
            return animator && animator.runtimeAnimatorController;
        }
        
        /// <param name="stateMachine"></param>
        /// <param name="callback">return Continue?</param>
        /// <returns>Continue?</returns>
        private static bool ForeachState(AnimatorStateMachine stateMachine, Func<AnimatorState, bool> callback)
        {
            if (callback == null) return false;
            foreach (var childAnimatorState in stateMachine.states)
                if(!callback(childAnimatorState.state)) return false;

            foreach (var childAnimatorStateMachine in stateMachine.stateMachines)
                if(!ForeachState(childAnimatorStateMachine.stateMachine, callback)) return false;

            return true;
        }
        
        /// <param name="motion"></param>
        /// <param name="callback">return Continue?</param>
        /// <returns>Continue?</returns>
        private static bool ForeachAnimationClip(Motion motion, Func<AnimationClip, bool> callback)
        {
            if (callback == null) return false;
            switch (motion)
            {
                case AnimationClip animationClip:
                    return callback(animationClip);
                case BlendTree blendTree:
                    foreach (var blendTreeChild in blendTree.children)
                        return ForeachAnimationClip(blendTreeChild.motion, callback);
                    return true;
                default:
                    return true;
            }
        }
    }
}
