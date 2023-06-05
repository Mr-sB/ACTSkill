using System;
using System.Collections.Generic;
using CustomizationInspector.Runtime;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public class RangeConfig : ICopyable
    {
        [Tooltip("True for new config. Otherwise use prev frame config.")]
        public bool ModifyRange;
        
        [LabelWidth(80)]
        [SerializeReference]
        [SerializeReferenceSelector(XMaxPadding = 44)]
        [ShowIf(nameof(ModifyRange))]
        public List<IRange> Ranges = new List<IRange>();

        public RangeConfig() { }

        public RangeConfig(RangeConfig other)
        {
            Copy(other);
        }
        
        public void Copy(RangeConfig other)
        {
            if (other == null) return;
            ModifyRange = other.ModifyRange;
            Ranges.Clear();
            if (other.Ranges != null)
                foreach (var range in other.Ranges)
                    Ranges.Add(range?.Clone());
        }
        
        public RangeConfig Clone()
        {
            return new RangeConfig(this);
        }

        public void Copy(object obj)
        {
            if (obj is RangeConfig other)
                Copy(other);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
