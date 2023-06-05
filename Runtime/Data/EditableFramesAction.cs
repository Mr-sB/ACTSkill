using System;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public abstract class EditableFramesAction : IAction
    {
        [SerializeField]
        private bool full;
        [SerializeField, HideInInspector]
        private int beginFrame;
        [SerializeField, HideInInspector]
        private int endFrame;
        [SerializeField]
        private bool loop = true;

        public virtual bool Full
        {
            get => full;
            set => full = value;
        }

        public virtual int BeginFrame
        {
            get => beginFrame;
            set => beginFrame = value;
        }
        
        public virtual int EndFrame{
            get => endFrame;
            set => endFrame = value;
        }
        
        public virtual bool Loop
        {
            get => loop;
            set => loop = value;
        }
        
        public void CopyBase(EditableFramesAction other)
        {
            if (other == null) return;
            Full = other.Full;
            BeginFrame = other.BeginFrame;
            EndFrame = other.EndFrame;
            Loop = other.Loop;
        }

        public abstract IActionHandler CreateHandler();
        public abstract void OnReleaseHandler(IActionHandler handler);
        public abstract EditableFramesAction Clone();
        public abstract void Copy(object obj);

        IAction IAction.Clone()
        {
            return Clone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
