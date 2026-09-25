using Player;
using Player.States;
using UnityEngine;

namespace Player.States
{
    [System.Serializable]
    public class AirState : RunState
    {
        public AirState(Controller core) : base(core) { }
        [SerializeField] protected float defaultGravityMultipler = 2f;
        protected float _defaultGravity;

        private float defaultFriction;

        protected override void Enter()
        {
            PlayAnimation(Animator.StringToHash(_data.animNames[0]));

            _defaultGravity = _core.Rb.gravityScale;
            _core.Rb.gravityScale = defaultGravityMultipler;
            defaultFriction = _core.Collider.sharedMaterial.friction;
            _core.Collider.sharedMaterial.friction = 0;
        }

        protected override void LogicUpdate()
        {
            base.LogicUpdate();
        }

        protected override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
        }

        protected override void Exit()
        {
            _core.Rb.gravityScale = _defaultGravity;
            _core.Collider.sharedMaterial.friction = defaultFriction;
        }

        protected void CheckNextStates()
        {
            if (_core.inputManager.dash.GetTrigger())
            {
                _core.ChangeMoveState<DashState>();
            }
            if (!_core.isGrounded() || _core.Rb.velocity.y > 0.1f)
                return;

            if (_core.inputManager.MoveInput != Vector2.zero)
                _core.ChangeMoveState<RunState>();
            else
                _core.ChangeMoveState<IdleState>();
        }
        protected override void LookForNextState()
        {
            if(_core.canCoyoteJump)
                if (_core.inputManager.Jump.GetTrigger())
                    _core.ChangeMoveState<JumpState>();

            CheckNextStates();
        }
    }
}