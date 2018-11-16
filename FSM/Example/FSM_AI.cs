using UnityEngine;
using FrameWork.FSM;

public class FSM_Factory
{
    public static FsmState<EnumState,FSM_AI> CreateState(EnumState state)
    {
        switch (state)
        {
            case EnumState.idle:
                return new Idle();
            case EnumState.run:
                return new Run();
            default:
                return null;
        }
    }
}

public class FSM_StateMachine : StateMachine<EnumState,FSM_AI>
{
    public override FsmState<EnumState,FSM_AI> CreateState(EnumState state)
    {
        return FSM_Factory.CreateState(state);
    }
    public override bool Equal(EnumState a, EnumState b) { return a == b; }
    public FSM_StateMachine(FSM_AI self)
    {
        this.self = self;
    }

    public void Run()
    {
        ChangeState(EnumState.run);
    }

    public void Idle()
    {
        ChangeState(EnumState.idle);
    }
}

public class FSM_AI : MonoBehaviour
{
    public Animation anim;
    public FSM_StateMachine stateMachine;
    public Animator animator;

    void Start()
    {
        stateMachine = new FSM_StateMachine(this); 
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(10,10,200,200),"Change To Idle"))
        {
            stateMachine.Idle();
        }

        if (GUI.Button(new Rect(220, 10, 200, 200), "Change To Run"))
        {
            stateMachine.Run();
        }
    }

    public void PlayAnim(string name)
    {
        anim.CrossFade(name);
        //anim.Play(name);
    }

    public void ChangeState(EnumState state)
    {
        stateMachine.ChangeState(state);
    }
}

