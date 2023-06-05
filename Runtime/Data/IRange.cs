using UnityEngine;

namespace ACTSkill
{
    public interface IRange : ICopyable
    {
        Vector3? GetOffset();
        void SetOffset(Vector3 offset);
        Vector3? GetRotation();
        void SetRotation(Vector3 rotation);
        Vector3? GetSize();
        void SetSize(Vector3 size);
        new IRange Clone();
    }
}
