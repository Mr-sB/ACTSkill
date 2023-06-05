using System;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public class AnimationTransitionConfig : ICopyable
    {
        [Tooltip("Negative means use default animation index")]
        public int Index = -1;
        [Tooltip("Transition duration")]
        public float Duration = 0;
        [Tooltip("Time offset")]
        public float Offset = 0;

        public AnimationTransitionConfig() { }

        public AnimationTransitionConfig(AnimationTransitionConfig other)
        {
            Copy(other);
        }

        public void Copy(AnimationTransitionConfig other)
        {
            if (other == null) return;
            Copy(other.Index, other.Duration, other.Offset);
        }
        
        public void Copy(int index, float duration, float offset)
        {
            Index = index;
            Duration = duration;
            Offset = offset;
        }

        public AnimationTransitionConfig Clone()
        {
            return new AnimationTransitionConfig(this);
        }

        public void Copy(object obj)
        {
            if (obj is AnimationTransitionConfig other)
                Copy(other);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
