using FrameWork.FSM;

public enum EnumState
{
    idle = 1,
    run = 2,
}

public class Idle : FsmState<EnumState, FSM_AI>
{
    public string animName = "idle";
    public override EnumState state { get { return EnumState.idle; } }
    public override void OnEnter()
    {
        base.OnEnter();
        self.PlayAnim(animName);
    }
}

public class Run : FsmState<EnumState, FSM_AI>
{
    public string animName = "run";
    public override EnumState state { get { return EnumState.run; } }
    public override void OnEnter()
    {
        base.OnEnter();
        self.PlayAnim(animName);
    }
}
