using Core.StateMachine;
using UnityEngine;

public class Hammer : BaseState
{
    public Hammer(GameObject owner, KaralamaStateMachine.SharedData sharedData) : base(owner, sharedData)
    {
    }

    public override void OnExit()
    {
        sharedData.hammerActive = false;
    }

    public override void OnStart()
    {
        sharedData.hammerActive = true;
        PlayAnimationAndWait("Hammer", () =>
        {
            sharedData.idleWhiteActive = true;
        });
    }

    public override void OnUpdate()
    {
    }
}
