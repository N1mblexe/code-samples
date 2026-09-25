using UnityEngine;

namespace Player.States
{
    [System.Serializable]
    public class DashState : Player.HSM.State
    {
        [SerializeField] private float dashDistance = 4f;
        [Range(0.1f, 2f)][SerializeField] private float dashTime = 0.4f;
        [SerializeField] private AnimationCurve dashCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private Vector2 _startPos;
        private Vector2 _targetPos;
        private float _currentTimer;
        private float _defaultGravity;
        private bool _isDashCompleted;

        public DashState(Controller core) : base(core) { }

        protected override void Enter()
        {
            _currentTimer = 0f;
            _isDashCompleted = false;

            _startPos = _core.Rb.position;
            float direction = _core.transform.localScale.x;
            _targetPos = new Vector2(_startPos.x + (dashDistance * direction), _startPos.y);

            _defaultGravity = _core.Rb.gravityScale;
            _core.Rb.gravityScale = 0f;
            _core.Rb.velocity = Vector2.zero;

            PlayAnimation(Animator.StringToHash(_data.animNames[0]));
        }

        protected override void PhysicsUpdate()
        {
            _currentTimer += Time.fixedDeltaTime;

            float progress = _currentTimer / dashTime;

            if (progress >= 1f)
            {
                _core.Rb.MovePosition(_targetPos);
                _isDashCompleted = true;
                return;
            }

            float curveValue = dashCurve.Evaluate(progress);
            Vector2 nextPosition = Vector2.Lerp(_startPos, _targetPos, curveValue);

            _core.Rb.MovePosition(nextPosition);
        }

        protected override void LogicUpdate() { }

        protected override void Exit()
        {
            _core.Rb.gravityScale = _defaultGravity;
            _core.Rb.velocity = Vector2.zero;
        }

        protected override void LookForNextState()
        {
            if (!_isDashCompleted) return;

            if (!_core.isGrounded())
            {
                _core.ChangeMoveState<AirState>();
            }
            else if (_core.inputManager.MoveInput != Vector2.zero)
                _core.ChangeMoveState<RunState>();
            else
                _core.ChangeMoveState<IdleState>();
        }
    }
}