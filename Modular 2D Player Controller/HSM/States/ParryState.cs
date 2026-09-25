using System.Collections;
using Player;
using Player.States;
using UnityEngine;

[System.Serializable]
public class ParryState : Player.HSM.State
{
    public ParryState(Controller core) : base(core) { }

    [SerializeField] private string parryAnimName;
    private bool isAnimFinished = false;

    protected override void Enter()
    {
        isAnimFinished = false;
        _core.weaponAnimator.Play(Animator.StringToHash(parryAnimName));
        _core.StartCoroutine(WaitAnimation());
    }
    IEnumerator WaitAnimation()
    {
        yield return null;
        float duration = _core.weaponAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);
        isAnimFinished = true;
    }

    protected override void LookForNextState()
    {
        if (!isAnimFinished) return;

        _core.ChangeHandState<EmptyState>();
    }
}
