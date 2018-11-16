using FrameWork.FSM;

public class FSM_Action : FsmState<EnumState, FSM_AI>
{
    public string animName;
    public float startFade;
    public float endFade;
    private EnumState _state;
    public override EnumState state { get { return _state; } }
    
    public string GetAction()
    {
        return this.animName;
    }

    public override void OnEnter()
    {
        base.OnEnter();
       // self.PlayAnim(GetAction(),startFade,endFade);
    }
}