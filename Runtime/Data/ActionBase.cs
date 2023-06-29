using System;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public abstract class ActionBase : ICopyable
    {
        public bool Full;
        [HideInInspector]
        public int BeginFrame;
        [HideInInspector]
        public int EndFrame;
        public bool Loop = true;
        
        public void CopyBase(ActionBase other)
        {
            if (other == null) return;
            Full = other.Full;
            BeginFrame = other.BeginFrame;
            EndFrame = other.EndFrame;
            Loop = other.Loop;
        }

        public abstract IActionHandler CreateHandler();
        public abstract void OnReleaseHandler(IActionHandler handler);
        public abstract ActionBase Clone();
        public abstract void Copy(object obj);

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
