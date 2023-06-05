using System.ComponentModel;
using ACTSkill;
using UnityEngine;

namespace ACTSkillEditor
{
    public abstract class AnimationProcessor
    {
        public readonly ACTSkillEditorWindow Owner;
        public AnimationProcessor(ACTSkillEditorWindow owner)
        {
            Owner = owner;
        }

        public virtual void OnEnable()
        {
            Owner.PropertyChanged += OnOwnerPropertyChanged;
            Owner.OnReload += OnRefresh;
            OnRefresh();
        }

        public virtual void OnDisable()
        {
            if (Owner)
            {
                Owner.PropertyChanged -= OnOwnerPropertyChanged;
                Owner.OnReload -= OnRefresh;
            }
        }
        
        protected abstract void OnTargetChange(GameObject target);
        protected abstract void OnStateChange(StateConfig stateConfig);
        protected abstract void OnFrameChange(int frameIndex);
        public abstract void OnRefresh();
        
        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ACTSkillEditorWindow.Target))
            {
                OnTargetChange(Owner.Target);
            }
            else if (e.PropertyName == nameof(ACTSkillEditorWindow.CurState))
            {
                OnStateChange(Owner.CurState);
            }
            else if (e.PropertyName == nameof(ACTSkillEditorWindow.SelectedFrameIndex))
            {
                OnFrameChange(Owner.SelectedFrameIndex);
            }
        }
    }
}