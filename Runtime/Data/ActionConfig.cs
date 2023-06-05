using System;
using System.Collections.Generic;
using CustomizationInspector.Runtime;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public class ActionConfig : HideableFoldout, ICopyable
    {
        [MinLabelWidth(80)]
        [MaxLabelWidth(110)]
        [SerializeReference]
        [SerializeReferenceSelector]
        public List<IAction> Actions = new List<IAction>();

        public ActionConfig() { }

        public ActionConfig(ActionConfig other)
        {
            Copy(other);
        }
        
        public void Copy(ActionConfig other)
        {
            if (other == null) return;
            HideFoldout = other.HideFoldout;
            Actions.Clear();
            if (other.Actions != null)
                foreach (var range in other.Actions)
                    Actions.Add(range?.Clone());
        }

        public ActionConfig Clone()
        {
            return new ActionConfig(this);
        }

        public void Copy(object obj)
        {
            if (obj is ActionConfig other)
                Copy(other);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
