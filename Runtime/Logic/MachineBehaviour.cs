using System;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public class MachineBehaviour
    {
        public IMachineController Controller { get; }
        public MachineConfig MachineConfig;
        public StateHandler StateHandler { get; }
        public string CurStateName => StateHandler.Config?.StateName;
        public string PrevStateName { private set; get; }
        public string NextStateName { private set; get; }
        public int NextStatePriority { private set; get; }
        
        public float LogicPerFrameTime { private set; get; }

        public event Action<bool> OnLogicUpdating;
        public event Action OnLogicUpdated;
        private Func<bool> canUpdateLogicHandler;
        
        private AnimationTransitionConfig nextStateTransition = new AnimationTransitionConfig();
        private float logicTime;

        public MachineBehaviour(IMachineController controller, MachineConfig config)
        {
            Controller = controller;
            MachineConfig = config;
            StateHandler = new StateHandler(this);
        }

        public void SetCanUpdateLogicHandler(Func<bool> canUpdateLogicHandler)
        {
            this.canUpdateLogicHandler = canUpdateLogicHandler;
        }
        
        public void OnEnable()
        {
            string stateName = MachineConfig?.GetDefaultStateConfig()?.StateName;
            if (stateName == null)
            {
                Debug.LogError("MachineBehaviour can not find default state config!");
                return;
            }
            logicTime = 0;
            LogicPerFrameTime = 1f / MachineConfig.FrameRate;
            PrevStateName = null;
            ChangeStateImmediate(stateName, MachineConfig.DefaultStateTransition);
        }

        public void Update(float deltaTime)
        {
            if (LogicPerFrameTime <= 0) return;
            logicTime += deltaTime;
            while (logicTime >= LogicPerFrameTime)
            {
                logicTime -= LogicPerFrameTime;
                bool canUpdate = canUpdateLogicHandler?.Invoke() ?? true;
                OnLogicUpdating?.Invoke(canUpdate);
                if (!canUpdate) continue;
                if (NextStateName != null)
                    ChangeStateImmediate(NextStateName, nextStateTransition);
                StateHandler.Update();
                OnLogicUpdated?.Invoke();
            }
        }

        public void OnDisable()
        {
            StateHandler.Exit();
        }

        /// <summary>
        /// Change state in next frame
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="priority"></param>
        /// <param name="animIndex"></param>
        /// <param name="animTransitionDuration"></param>
        /// <param name="animTimeOffset"></param>
        /// <returns></returns>
        public bool ChangeState(string stateName, int priority, int animIndex = -1, float animTransitionDuration = 0, float animTimeOffset = 0)
        {
            if (NextStateName != null && priority < NextStatePriority) return false;
            StateConfig nextStateConfig = MachineConfig.GetStateConfig(stateName);
            if (nextStateConfig == null)
            {
                Debug.LogError($"MachineBehaviour ChangeState failed: can not find state config {stateName}!");
                return false;
            }

            NextStateName = stateName;
            nextStateTransition.Copy(animIndex, animTransitionDuration, animTimeOffset);
            return true;
        }
        
        public bool ChangeState(string stateName, int priority, AnimationTransitionConfig transition)
        {
            return transition == null ? ChangeState(stateName, priority) : ChangeState(stateName, priority, transition.Index, transition.Duration, transition.Offset);
        }
        
        private bool ChangeStateImmediate(string stateName, int animIndex, float animTransitionDuration, float animTimeOffset)
        {
            StateConfig nextStateConfig = MachineConfig.GetStateConfig(stateName);
            if (nextStateConfig == null)
            {
                Debug.LogError($"MachineBehaviour ChangeStateImmediate failed: can not find state config {stateName}!");
                return false;
            }
            //Clear next state
            NextStateName = null;
            NextStatePriority = 0;

            PrevStateName = CurStateName;

            StateHandler.Exit();

            StateHandler.Enter(nextStateConfig, animIndex, animTransitionDuration, animTimeOffset);
            return true;
        }
        
        private bool ChangeStateImmediate(string stateName, AnimationTransitionConfig transition)
        {
            return ChangeStateImmediate(stateName, transition.Index, transition.Duration, transition.Offset);
        }
    }
}
