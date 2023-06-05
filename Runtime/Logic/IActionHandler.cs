namespace ACTSkill
{
    public interface IActionHandler
    {
        void OnCreate(ActionNode actionNode);
        void OnEnter(ActionNode actionNode);
        void OnUpdate(ActionNode actionNode);
        void OnExit(ActionNode actionNode);
        void OnRelease(ActionNode actionNode);
    }
}
