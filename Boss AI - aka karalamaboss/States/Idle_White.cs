using Core.StateMachine;
using UnityEngine;

public class Idle_White : BaseState
{
    public Idle_White(GameObject owner, KaralamaStateMachine.SharedData sharedData) : base(owner, sharedData)
    {
    }

    public override void OnExit()
    {
        sharedData.idleWhiteActive = false;
    }

    public override void OnStart()
    {
        sharedData.idleWhiteActive = true;
        sharedData.exploitActive = true;
    }

    public override void OnUpdate()
    {
    }
}
