using UnityEngine;

namespace ACTSkill
{
    public interface IRange : ICopyable
    {
        /// <returns>Return null means can not change offset, and con not use move tool</returns>
        Vector3? GetOffset();
        
        void SetOffset(Vector3 offset);
        
        /// <returns>Return null means can not change rotation, and can not use rotate tool</returns>
        Vector3? GetRotation();
        
        void SetRotation(Vector3 rotation);
        
        /// <returns>Return null means can not change size, and can not use scale tool</returns>
        Vector3? GetSize();
        
        void SetSize(Vector3 size);
        
        new IRange Clone();
    }
}
