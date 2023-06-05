using UnityEngine;
using UnityEngine.Pool;

namespace ACTSkill
{
    public static class MachineHelper
    {
        public static ObjectPool<ActionNode> ActionNodePool { get; } = new ObjectPool<ActionNode>(
            ()=> new ActionNode(),
            null,
            node => node?.Reset());

        public static string Serialize(this MachineConfig config, bool prettyPrint)
        {
            if (config == null) return string.Empty;
            return JsonUtility.ToJson(config, prettyPrint);
        }
        
        public static MachineConfig DeserializeMachineConfig(string configAsset)
        {
            if (string.IsNullOrEmpty(configAsset)) return null;
            return JsonUtility.FromJson<MachineConfig>(configAsset);
        }
    }
}