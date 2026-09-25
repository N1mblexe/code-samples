using Core.StateMachine;
using UnityEngine;

public class Idle_Black : BaseState
{
    public Idle_Black(GameObject owner, KaralamaStateMachine.SharedData sharedData) : base(owner, sharedData)
    {
    }

    public override void OnExit()
    {
        sharedData.idleBlackActive = false;
    }

    public override void OnStart()
    {
        sharedData.idleBlackActive = true;
    }

    public override void OnUpdate()
    {
    }
}
