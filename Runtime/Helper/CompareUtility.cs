using System;
using UnityEngine;

namespace ACTSkill
{
    public class CompareUtility
    {
        public static bool Compare<T>(T a, T b, CompareType compare) where T : IComparable<T>
        {
            int result = a.CompareTo(b);
            return CheckResult(result, compare);
        }

        public static bool CheckResult(int result, CompareType compare)
        {
            switch (compare)
            {
                case CompareType.Greater:
                    return result > 0;
                case CompareType.Less:
                    return result < 0;
                case CompareType.Equal:
                    return result == 0;
                case CompareType.GreaterEqual:
                    return result >= 0;
                case CompareType.LessEqual:
                    return result <= 0;
                case CompareType.NotEqual:
                    return result != 0;
                default:
                    Debug.LogError($"Not exist compare type: {compare}");
                    return false;
            }
        }
    }
}