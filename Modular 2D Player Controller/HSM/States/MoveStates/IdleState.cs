using UnityEngine;

namespace Player.States
{
    [System.Serializable]
    public class IdleState : Player.HSM.State
    {
        public IdleState(Controller core) : base(core) { }

        protected override void Enter()
        {
            PlayAnimation(Animator.StringToHash(_data.animNames[0]));
            _core.canAirDash = true;
        }
        protected override void Exit() { }
        protected override void LogicUpdate() { }
        protected override void PhysicsUpdate() { }
        protected override void LookForNextState()
        {
            Debug.Log("Idle kısmına giriyor");
            var inputManager = _core.inputManager;
            if (inputManager.Jump.GetTrigger())
            {
                _core.ChangeMoveState<JumpState>();
            }
            if (Vector2.Distance(inputManager.MoveInput ,Vector2.zero) > 0.1f)
            {
                _core.ChangeMoveState<RunState>();
            }
        }
    }
}