using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    [CustomEditor(typeof(WrapperSO), true)]
    public class WrapperSOEditor : Editor
    {
        protected override void OnHeaderGUI()
        {
            if (!target || target is not WrapperSO wrapperSo) return;

            GUILayout.BeginVertical(GUIStyleHelper.InspectorBig);
            string name = wrapperSo.NameGetter?.Invoke();
            if (!string.IsNullOrEmpty(name))
                GUILayout.Label(name, GUIStyleHelper.InspectorTitlebarText, GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            if (wrapperSo.DoCopy != null)
                if (GUILayout.Button("Copy"))
                    wrapperSo.DoCopy();
            if (wrapperSo.DoPaste != null)
                if (GUILayout.Button("Paste"))
                    wrapperSo.DoPaste();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            if (!target || target is not WrapperSO wrapperSo) return;
            wrapperSo.DrawInspectorGUI?.Invoke();
        }
    }
}
