using System.Collections.Generic;
using UnityEngine;

namespace ACTSkill
{
    public interface IMachineController
    {
        MachineBehaviour MachineBehaviour { get; }
        Matrix4x4 LocalToWorld { get; }
        Dictionary<string, object> Datas { get; }
        bool TryGetData<T>(string key, out T value);
        T GetData<T>(string key, T defaultValue = default);
        void SetData<T>(string key, T value);
        void RemoveData(string key);
    }
}
