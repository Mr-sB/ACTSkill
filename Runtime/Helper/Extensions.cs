namespace ACTSkill
{
    public static class Extensions
    {
        public static bool IsAlive(this ActionBase action, int frame, int loopTimes)
        {
            if (action == null) return false;
            if (loopTimes > 0 && !action.Loop) return false;
            return action.Full || action.BeginFrame <= frame && action.EndFrame >= frame;
        }
    }
}
