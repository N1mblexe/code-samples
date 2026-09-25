using UnityEngine;

namespace Player.States
{
    [System.Serializable]
    public class EmptyState : Player.HSM.State
    {
        public EmptyState(Controller core) : base(core) { }

        protected override void Enter()
        {
            _core.weaponAnimator.Play(Animator.StringToHash(_data.animNames[0]));
        }

        protected override void LookForNextState()
        {
            if (_core.inputManager.Attack.GetTrigger())
            {
                _core.ChangeHandState<AttackState>();
            }
            if (_core.inputManager.Parry.GetTrigger())
            {
                _core.ChangeHandState<ParryState>();
            }
        }
    }
}