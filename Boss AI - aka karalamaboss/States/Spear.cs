using Core.StateMachine;
using UnityEngine;

public class Spear : BaseState
{
    public Spear(GameObject owner, KaralamaStateMachine.SharedData sharedData) : base(owner, sharedData)
    {
    }

    public override void OnExit()
    {
        sharedData.spearActive = false;
    }

    public override void OnStart()
    {
        sharedData.spearActive = true;
        PlayAnimationAndWait("Spear", () =>
        {
            sharedData.idleWhiteActive = true;
        });
    }

    public override void OnUpdate()
    {
    }
}
