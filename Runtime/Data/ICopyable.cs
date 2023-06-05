using System;

namespace ACTSkill
{
    public interface ICopyable : ICloneable
    {
        void Copy(object obj);
    }
}
