using System;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public class Range2DBase
    {
        public Vector2 Offset = Vector2.zero;
        public float Rotation = 0;

        public Range2DBase() { }

        public Range2DBase(Vector2 offset, float rotation)
        {
            Copy(offset, rotation);
        }

        public Range2DBase(Range2DBase other)
        {
            Copy(other);
        }

        public Vector3? GetOffset()
        {
            return Offset;
        }

        public void SetOffset(Vector3 offset)
        {
            Offset = offset;
        }

        public Vector3? GetRotation()
        {
            return new Vector3(0, 0, Rotation);
        }

        public void SetRotation(Vector3 rotation)
        {
            Rotation = rotation.z;
        }

        public void Copy(Vector2 offset, float rotation)
        {
            Offset = offset;
            Rotation = rotation;
        }
        
        public void Copy(Range2DBase other)
        {
            if (other == null) return;
            Copy(other.Offset, other.Rotation);
        }
    }
}
