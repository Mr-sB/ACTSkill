using System;
using UnityEngine;

namespace ACTSkillEditor
{
    public class WrapperSO : ScriptableObject
    {
        [SerializeReference]
        public object Data;

        public event Action OnValidateEvent;
        
        public void OnValidate()
        {
            OnValidateEvent?.Invoke();
        }

        private void OnDestroy()
        {
            OnValidateEvent = null;
        }
    }
}
