namespace ACTSkill
{
    public class ActionNode
    {
        public StateHandler Owner { internal set; get; }
        public IAction Config { internal set; get; }
        public IActionHandler Handler { internal set; get; }

        public void Init(StateHandler owner, IAction config, IActionHandler handler)
        {
            Owner = owner;
            Config = config;
            Handler = handler;
            Handler.OnCreate(this);
        }
        
        public void InvokeEnter()
        {
            Handler.OnEnter(this);
        }

        public void InvokeUpdate()
        {
            Handler.OnUpdate(this);
        }
        
        public void InvokeExit()
        {
            Handler.OnExit(this);
        }

        public void InvokeRelease()
        {
            Handler.OnRelease(this);
            Config.OnReleaseHandler(Handler);
        }

        public bool IsAlive(int frame, int loopTimes)
        {
            if (Config == null || Handler == null) return false;
            return Config.IsAlive(frame, loopTimes);
        }
        
        public void Reset()
        {
            Owner = null;
            Config = null;
            Handler = null;
        }
    }
}
