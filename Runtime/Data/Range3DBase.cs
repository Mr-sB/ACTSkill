using UnityEngine;

namespace ACTSkill
{
    public class Range3DBase
    {
        public Vector3 Offset = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;

        public Range3DBase() { }

        public Range3DBase(Vector3 offset, Vector3 rotation)
        {
            Copy(offset, rotation);
        }

        public Range3DBase(Range3DBase other)
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
            return Rotation;
        }

        public void SetRotation(Vector3 rotation)
        {
            Rotation = rotation;
        }

        public void Copy(Vector3 offset, Vector3 rotation)
        {
            Offset = offset;
            Rotation = rotation;
        }

        public void Copy(Range3DBase other)
        {
            if (other == null) return;
            Copy(other.Offset, other.Rotation);
        }
    }
}
