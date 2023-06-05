using UnityEditor;

namespace ACTSkillEditor
{
    public abstract class SceneGUIBase
    {
        public readonly ACTSkillEditorWindow Owner;

        public SceneGUIBase(ACTSkillEditorWindow owner)
        {
            Owner = owner;
        }
        
        public virtual void OnEnable()
        {
        }
        
        public abstract void OnSceneGUI(SceneView sceneView);

        public virtual void OnDisable()
        {
        }
    }
}
