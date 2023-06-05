using System;
using UnityEngine;

namespace ACTSkill
{
    [Serializable]
    public class RectRange : Range2DBase, IRange
    {
        public Vector2 Size = Vector2.one;

        public RectRange() { }

        public RectRange(Vector2 offset, float rotation, Vector2 size)
        {
            Copy(offset, rotation, size);
        }

        public RectRange(RectRange other)
        {
            Copy(other);
        }

        public Vector3? GetSize()
        {
            return Size;
        }

        public void SetSize(Vector3 size)
        {
            Size = size;
        }

        public RectRange Clone()
        {
            return new RectRange(this);
        }

        public void Copy(object obj)
        {
            if (obj is RectRange other)
                Copy(other);
        }

        IRange IRange.Clone()
        {
            return Clone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public void Copy(Vector2 offset, float rotation, Vector2 size)
        {
            Copy(offset, rotation);
            Size = size;
        }
        
        public void Copy(RectRange other)
        {
            if (other == null) return;
            Copy(other.Offset, other.Rotation, other.Size);
        }
    }

    [Serializable]
    public class CircleRange : Range2DBase, IRange
    {
        public float Radius = 0.5f;

        public CircleRange() { }

        public CircleRange(Vector2 offset, float rotation, float radius)
        {
            Copy(offset, rotation, radius);
        }

        public CircleRange(CircleRange other)
        {
            Copy(other);
        }

        public Vector3? GetSize()
        {
            return Vector2.one.normalized * Radius;
        }

        public void SetSize(Vector3 size)
        {
            Radius = size.magnitude;
        }

        public CircleRange Clone()
        {
            return new CircleRange(this);
        }

        public void Copy(object obj)
        {
            if (obj is CircleRange other)
                Copy(other);
        }

        IRange IRange.Clone()
        {
            return Clone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public void Copy(Vector2 offset, float rotation, float radius)
        {
            Copy(offset, rotation);
            Radius = radius;
        }

        public void Copy(CircleRange other)
        {
            if (other == null) return;
            Copy(other.Offset, other.Rotation, other.Radius);
        }
    }

    [Serializable]
    public class BoxRange : Range3DBase, IRange
    {
        public Vector3 Size = Vector3.one;

        public BoxRange() { }

        public BoxRange(Vector3 offset, Vector3 rotation, Vector3 size)
        {
            Copy(offset, rotation, size);
        }

        public BoxRange(BoxRange other)
        {
            Copy(other);
        }
        
        Vector3? IRange.GetSize()
        {
            return Size;
        }

        public void SetSize(Vector3 size)
        {
            Size = size;
        }

        public BoxRange Clone()
        {
            return new BoxRange(this);
        }

        public void Copy(object obj)
        {
            if (obj is BoxRange other)
                Copy(other);
        }

        IRange IRange.Clone()
        {
            return Clone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public void Copy(Vector3 offset, Vector3 rotation, Vector3 size)
        {
            Copy(offset, rotation);
            Size = size;
        }

        public void Copy(BoxRange other)
        {
            if (other == null) return;
            Copy(other.Offset, other.Rotation, other.Size);
        }
    }

    [Serializable]
    public class SphereRange : Range3DBase, IRange
    {
        public float Radius = 0.5f;

        public SphereRange() { }
        
        public SphereRange(Vector3 offset, Vector3 rotation, float radius)
        {
            Copy(offset, rotation, radius);
        }

        public SphereRange(SphereRange other)
        {
            Copy(other);
        }

        public Vector3? GetSize()
        {
            return Vector3.one.normalized * Radius;
        }

        public void SetSize(Vector3 size)
        {
            Radius = size.magnitude;
        }

        public SphereRange Clone()
        {
            return new SphereRange(this);
        }

        public void Copy(object obj)
        {
            if (obj is SphereRange other)
                Copy(other);
        }

        IRange IRange.Clone()
        {
            return Clone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        
        public void Copy(Vector3 offset, Vector3 rotation, float radius)
        {
            Copy(offset, rotation);
            Radius = radius;
        }

        public void Copy(SphereRange other)
        {
            if (other == null) return;
            Copy(other.Offset, other.Rotation, other.Radius);
        }
    }
}
