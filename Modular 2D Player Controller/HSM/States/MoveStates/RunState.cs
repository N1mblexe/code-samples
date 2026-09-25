using UnityEngine;

namespace Player.States
{
    [System.Serializable]
    public class RunState : Player.HSM.State
    {
        public RunState(Controller core) : base(core) { }

        [SerializeField] private float walkSpeed = 2f;

        protected override void Enter()
        {
            PlayAnimation(Animator.StringToHash(_data.animNames[0]));
            _core.canAirDash = true;
        }
        protected override void LogicUpdate()
        {
            FacePlayer();
        }

        protected override void PhysicsUpdate()
        {
            float velX = _core.inputManager.MoveInput.x * walkSpeed;
            _core.Rb.velocity = new Vector2(velX, _core.Rb.velocity.y);
        }

        protected override void LookForNextState()
        {
            Debug.Log("Runstate kısmına giriyor");

            var input = _core.inputManager;
            if (input.Jump.GetTrigger())
            {
                _core.ChangeMoveState<JumpState>();
            }
            else if (!_core.isGrounded())
            {
                _core.StartCoyoteTime();
                _core.ChangeMoveState<AirState>();
            }
            else if (Vector2.Distance(input.MoveInput , Vector2.zero) < 0.1f)
            {
                _core.Rb.velocity = Vector2.zero;
                _core.ChangeMoveState<IdleState>();
            }
            else if (input.dash.GetTrigger())
                _core.ChangeMoveState<DashState>();
        }

        private void FacePlayer()
        {
            if (_core.inputManager.MoveInput.x == 0)
                return;

            var direction = (_core.inputManager.MoveInput.x > 0) ? 1 : -1;
            var scale = _core.transform.localScale;
            _core.transform.localScale = new Vector3(direction, scale.y, scale.z);

        }
    }
}