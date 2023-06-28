using System;
using System.Collections.Generic;
using CustomizationInspector.Runtime;

namespace ACTSkill
{
    [Serializable]
    public class StateConfig : ICopyable
    {
        private const int LABEL_WIDTH = 109;
        
        [LabelWidth(LABEL_WIDTH)]
        public string StateName = "New State";
        
        [LabelWidth(LABEL_WIDTH)]
        public int DefaultAnimIndex;
        
        [LabelWidth(80)]
        public List<string> Animations = new List<string>();
        
        [LabelWidth(LABEL_WIDTH)]
        public bool Loop;
        
        [LabelWidth(LABEL_WIDTH)]
        [HideIf(nameof(Loop))]
        public string NextStateName;
        
        [LabelWidth(LABEL_WIDTH)]
        [HideIf(nameof(Loop))]
        public int NextStatePriority = -1;

        [LabelWidth(LABEL_WIDTH)]
        [HideIf(nameof(Loop))]
        public AnimationTransitionConfig NextStateTransition = new AnimationTransitionConfig();

        // Can not use HideInInspector, otherwise can not get serialized property.
        public List<FrameConfig> Frames = new List<FrameConfig>();
        public ActionConfig ActionConfig = new ActionConfig();
        
        public string DefaultAnimName => GetAnimName(DefaultAnimIndex);

        public StateConfig() { }

        public StateConfig(StateConfig other)
        {
            Copy(other);
        }
        
        public string GetAnimName(int index)
        {
            return Animations?.Count > index ? Animations[index] : string.Empty;
        }
        
        public RangeConfig GetAttackRange(int frameIndex)
        {
            return GetRange(frameIndex, FrameConfig.GetAttackRange);
        }
        
        public RangeConfig GetBodyRange(int frameIndex)
        {
            return GetRange(frameIndex, FrameConfig.GetBodyRange);
        }

        private RangeConfig GetRange(int frameIndex, Func<FrameConfig, RangeConfig> rangeConfigGetter)
        {
            return FrameConfig.GetModifyRange(Frames, frameIndex, rangeConfigGetter);
        }

        public void Copy(StateConfig other)
        {
            if (other == null) return;
            CopyStateSetting(other);
            if (other.Animations != null)
                Animations.AddRange(other.Animations);
            Frames.Clear();
            if (other.Frames != null)
                foreach (var frame in other.Frames)
                    Frames.Add(frame?.Clone());
            ActionConfig.Copy(other.ActionConfig);
        }
        
        public void CopyStateSetting(StateConfig other)
        {
            if (other == null) return;
            StateName = other.StateName;
            DefaultAnimIndex = other.DefaultAnimIndex;
            Animations.Clear();
            Loop = other.Loop;
            NextStateName = other.NextStateName;
            NextStateTransition.Copy(other.NextStateTransition);
        }
        
        public StateConfig Clone()
        {
            return new StateConfig(this);
        }

        public StateConfig CloneStateSetting()
        {
            var clone = new StateConfig();
            clone.CopyStateSetting(this);
            return clone;
        }

        public void Copy(object obj)
        {
            if (obj is StateConfig other)
                Copy(other);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override string ToString()
        {
            return StateName;
        }
    }
}
