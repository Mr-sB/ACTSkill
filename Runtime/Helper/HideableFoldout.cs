using System;

namespace ACTSkill
{
    [Serializable]
    public class HideableFoldout
    {
        /// <summary>
        /// Use for editor. False for normal drawer. Otherwise do not draw class foldout.
        /// </summary>
        [NonSerialized]
        public bool HideFoldout;
    }
}
