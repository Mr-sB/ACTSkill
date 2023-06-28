using System;
using UnityEngine;

namespace ACTSkillEditor
{
    public class WrapperSO : ScriptableObject
    {
        public Func<string> NameGetter;
        public Action DrawInspectorGUI;
        public Action DoCopy;
        public Action DoPaste;
    }
}
