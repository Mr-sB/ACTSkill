using System;
using System.Collections.Generic;
using CustomizationInspector.Runtime;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public class MachineConfig : ICopyable
    {
        public int FrameRate = 30;
        public string DefaultStateName;
        public AnimationTransitionConfig DefaultStateTransition = new AnimationTransitionConfig();
        [LabelWidth(125)]
        public List<StateConfig> States = new List<StateConfig>();


        private bool stateDirty = true;
        private Dictionary<string, StateConfig> stateDict;

        public IReadOnlyDictionary<string, StateConfig> StateDict
        {
            get
            {
                if (!stateDirty) return stateDict;
                stateDict ??= new Dictionary<string, StateConfig>(States?.Count ?? 10);
                stateDirty = false;
                if (States == null) return stateDict;
                foreach (var stateConfig in States)
                {
                    if (stateConfig.StateName == null)
                    {
                        Debug.LogWarning("MachineConfig has same null name state, skip!");
                        continue;
                    }
                    if (!stateDict.ContainsKey(stateConfig.StateName))
                        stateDict.Add(stateConfig.StateName, stateConfig);
                    else
                        Debug.LogWarning($"MachineConfig has same state name : {stateConfig.StateName}, only first will be used.");
                }
                return stateDict;
            }
        }
        
        public MachineConfig() { }

        public MachineConfig(MachineConfig other)
        {
            Copy(other);
        }

        public StateConfig GetStateConfig(string stateName)
        {
            if (stateName == null) return null;
            StateDict.TryGetValue(stateName, out var stateConfig);
            return stateConfig;
        }

        public StateConfig GetDefaultStateConfig()
        {
            string stateName = DefaultStateName;
            var stateConfig = GetStateConfig(stateName);
            if (stateConfig != null) return stateConfig;
            //Find next state
            if (States != null)
            {
                foreach (var config in States)
                {
                    if (config?.StateName != null)
                    {
                        stateConfig = config;
                        break;
                    }
                }
            }
            return stateConfig;
        }

        public void SetStateDirty()
        {
            stateDirty = true;
        }
        
        public void Copy(MachineConfig other)
        {
            if (other == null) return;
            FrameRate = other.FrameRate;
            DefaultStateName = other.DefaultStateName;
            DefaultStateTransition.Copy(other.DefaultStateTransition);
            States.Clear();
            if (other.States != null)
                foreach (var state in other.States)
                    States.Add(state?.Clone());
        }
        
        public MachineConfig Clone()
        {
            return new MachineConfig(this);
        }

        public void Copy(object obj)
        {
            if (obj is MachineConfig other)
                Copy(other);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
