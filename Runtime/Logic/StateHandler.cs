using System;
using System.Collections.Generic;

namespace ACTSkill
{
    public class StateHandler
    {
        public MachineBehaviour Owner;
        public StateConfig Config { private set; get; }
        
        /// <summary>
        /// Include loop
        /// </summary>
        public int TotalFrame { private set; get; }
        public int Frame { private set; get; }
        public int LoopTimes { private set; get; }
        private int realTotalFrame;
        public string AnimName { private set; get; }
        
        private List<ActionNode> actionNodes = new List<ActionNode>();
        
        /// <summary>
        /// animName, animTransitionDuration, animTimeOffset
        /// </summary>
        public event Action<string, float, float> OnStateChanged;
        public event Action OnStateExiting;
        public event Action OnStateUpdating;

        public StateHandler(MachineBehaviour owner)
        {
            Owner = owner;
        }
        
        public void Enter(StateConfig stateConfig, int animIndex, float animTransitionDuration, float animTimeOffset)
        {
            //Init
            Config = stateConfig;
            realTotalFrame = -1;
            SetTotalFrame(0);
            actionNodes.Clear();
            if (Config?.ActionConfig?.Actions != null)
            {
                foreach (var actionConfig in Config.ActionConfig.Actions)
                {
                    ActionNode actionNode = null;
                    var handler = actionConfig?.CreateHandler();
                    if (handler != null)
                    {
                        actionNode = MachineHelper.ActionNodePool.Get();
                        actionNode.Init(this, actionConfig, handler);
                    }
                    actionNodes.Add(actionNode);
                }
            }

            //OnEnter
            ExecuteActionHandlers(InvokeOnEnter);
            
            //Anim
            if (animIndex < 0)
                animIndex = stateConfig.DefaultAnimIndex;
            if (stateConfig.Animations != null && animIndex >= 0 && animIndex < stateConfig.Animations.Count)
                AnimName = stateConfig.Animations[animIndex];
            OnStateChanged?.Invoke(AnimName, animTransitionDuration, animTimeOffset);
        }
        
        /// <summary>
        /// Execute every frame. After OnEnter.
        /// </summary>
        public void Update()
        {
            if (Config == null) return;
            var lastFrame = Frame;
            var lastLoopTimes = LoopTimes;
            SetTotalFrame(++realTotalFrame);
            OnStateUpdating?.Invoke();
            
            for (int i = 0, count = actionNodes.Count; i < count; i++)
            {
                var actionNode = actionNodes[i];
                if (actionNode == null) continue;
                bool lastAlive = actionNode.IsAlive(lastFrame, lastLoopTimes);
                bool curAlive = actionNode.IsAlive(Frame, LoopTimes);
                if (!lastAlive && curAlive)
                    actionNode.InvokeEnter();
                if (curAlive)
                    actionNode.InvokeUpdate();
                else if (lastAlive)
                    actionNode.InvokeExit();
            }

            //Change to next state
            if (Frame == Config.Frames.Count - 1 && !Config.Loop)
                Owner.ChangeState(Config.NextStateName, Config.NextStatePriority, Config.NextStateTransition);
        }

        public void Exit()
        {
            OnStateExiting?.Invoke();
            ExecuteActionHandlers(InvokeOnExit);
            for (int i = 0, count = actionNodes.Count; i < count; i++)
            {
                var actionNode = actionNodes[i];
                if (actionNode == null) continue;
                actionNode.InvokeRelease();
                MachineHelper.ActionNodePool.Release(actionNode);
            }
            actionNodes.Clear();
        }

        private void ExecuteActionHandlers(Action<ActionNode> executor)
        {
            if (executor == null) return;
            for (int i = 0, count = actionNodes.Count; i < count; i++)
            {
                var actionNode = actionNodes[i];
                if (actionNode != null && actionNode.IsAlive(Frame, LoopTimes))
                    executor(actionNode);
            }
        }

        private void SetTotalFrame(int totalFrame)
        {
            TotalFrame = totalFrame;
            if (totalFrame == 0)
            {
                Frame = 0;
                LoopTimes = 0;
            }
            else
            {
                Frame = totalFrame % Config.Frames.Count;
                LoopTimes = totalFrame / Config.Frames.Count;
            }
        }
        
        private static void InvokeOnEnter(ActionNode actionNode)
        {
            actionNode.InvokeEnter();
        }
        
        private static void InvokeOnExit(ActionNode actionNode)
        {
            actionNode.InvokeExit();
        }
    }
}
