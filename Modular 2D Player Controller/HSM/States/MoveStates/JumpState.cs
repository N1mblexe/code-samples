using System.Collections;
using UnityEngine;

namespace Player.States
{
    [System.Serializable]
    public class JumpState : AirState
    {
        public JumpState(Controller core) : base(core) { }

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float peakThreashold = .5f;
        [SerializeField] private float lowJumpMultiplier = 7f;

        private bool isJumpStarted = false;

        protected override void Enter()
        {
            base.Enter();

            isJumpStarted = false;
            _core.Rb.gravityScale = defaultGravityMultipler;

            _core.StartCoroutine(StateSaver());
        }

        public void StartJump()
        {
            _core.Rb.velocity = new Vector2(_core.Rb.velocity.x, 0);
            _core.Rb.velocity = new Vector2(_core.Rb.velocity.x, jumpForce);

            isJumpStarted = true;
        }

        protected override void LogicUpdate()
        {
            base.LogicUpdate();

            if (_core.Rb.velocity.y < 1f && _core.Rb.velocity.y > 0.5f)
            {
                _core.Rb.velocity = new Vector2(_core.Rb.velocity.x, 0);
                _core.Rb.gravityScale = lowJumpMultiplier;
            }

            if ((_core.inputManager.IsJumpCanceled || !_core.inputManager.IsJumpHeld) && _core.Rb.velocity.y > 0)
            {
                _core.Rb.velocity = new Vector2(_core.Rb.velocity.x, 0);
                _core.Rb.gravityScale = lowJumpMultiplier;
            }
        }

        protected override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
        }

        protected override void Exit()
        {
            base.Exit();
        }

        protected override void LookForNextState()
        {
            //todo: i cant quite figure out but maybe it can softlock the state. PROPERLY TEST IT BEFORE PUBLISHING
            if (!isJumpStarted)
                return;

            CheckNextStates();
        }

        IEnumerator StateSaver()
        {
            yield return new WaitForSeconds(0.04f);
            if(_core.GetMoveState() == _core.jumpState)
                StartJump();
        }
    }
}