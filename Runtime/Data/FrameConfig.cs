using System;
using System.Collections.Generic;

namespace ACTSkill
{
    [Serializable]
    public class FrameConfig : ICopyable
    {
        public RangeConfig AttackRange = new RangeConfig();
        public RangeConfig BodyRange = new RangeConfig();

        public FrameConfig() { }

        public FrameConfig(FrameConfig other)
        {
            Copy(other);
        }
        
        public FrameConfig(RangeConfig attackRange, RangeConfig bodyRange)
        {
            Copy(attackRange, bodyRange);
        }

        public void Copy(RangeConfig attackRange, RangeConfig bodyRange)
        {
            AttackRange.Copy(attackRange);
            BodyRange.Copy(bodyRange);
        }

        public void Copy(FrameConfig other)
        {
            if (other == null) return;
            Copy(other.AttackRange, other.BodyRange);
        }
        
        public FrameConfig Clone()
        {
            return new FrameConfig(this);
        }

        public void Copy(object obj)
        {
            if (obj is FrameConfig other)
                Copy(other);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        
        public static RangeConfig GetModifyRange(IReadOnlyList<FrameConfig> frames, int frameIndex, Func<FrameConfig, RangeConfig> rangeConfigGetter)
        {
            if (frames == null || frameIndex < 0 || rangeConfigGetter == null) return null;
            frameIndex %= frames.Count;
            
            RangeConfig rangeConfig = null;
            for (int i = frameIndex; i >= 0; i--)
            {
                var frameConfig = frames[i];
                if (frameConfig == null) continue;
                var config = rangeConfigGetter(frameConfig);
                if (config == null || !config.ModifyRange) continue;
                //Find the modify range
                rangeConfig = config;
                break;
            }
            return rangeConfig;
        }

        public static RangeConfig GetAttackRange(FrameConfig frameConfig)
        {
            return frameConfig?.AttackRange;
        }
        
        public static RangeConfig GetBodyRange(FrameConfig frameConfig)
        {
            return frameConfig?.BodyRange;
        }
    }
}
