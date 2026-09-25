using System.Collections;
using System.Collections.Generic;
using Player;
using Player.HSM;
using Player.States;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class AttackState : Player.HSM.State
{
    public AttackState(Controller core) : base(core) { }

    [SerializeField] private List<string> weaponAnimNames;
    private bool isAnimFinished = false;

    private int comboCounter = 0;
    private int maxCombo = 3;

    private int interruptedAnimHash = -1;
    private State interruptedState;
    protected override void Enter()
    {
        isAnimFinished = false;
        _core.weaponAnimator.Play(Animator.StringToHash(weaponAnimNames[comboCounter]));
        _core.StartCoroutine(WaitAnimation());
        maxCombo = weaponAnimNames.Count;

        interruptedState = _core.GetMoveState();
        interruptedAnimHash = _core.Anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
        PlayAnimation(Animator.StringToHash(_data.animNames[comboCounter]));
        _core.StartCoroutine(TryDamaging());
    }

    protected override void Exit()
    {
        if (++comboCounter >= maxCombo)
            comboCounter = 0;
    }

    [SerializeField] private ContactFilter2D attackFilter;
    private List<Collider2D> hitEnemies = new List<Collider2D>();
    [SerializeField] private Collider2D weaponCollider;

    [SerializeField] private ShakeProfile hitShakeProfile;
    IEnumerator TryDamaging()
    {
        while(!weaponCollider.enabled)
            yield return null;
        
        hitEnemies = new List<Collider2D>();
        int hits = weaponCollider.OverlapCollider(attackFilter, hitEnemies);

        foreach (var enemies in hitEnemies)
        {
            Debug.Log("Hit " + enemies.name);
            if (enemies.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                if (damagable == _core.GetComponent<IDamagable>())
                    continue;
                Debug.Log("Damaging " + enemies.name);
                //TODO : change combo counter system to something more fun??
                damagable.Damage(_core.gameObject, (uint)comboCounter + 1);
                CameraShaker.Instance.Shake(hitShakeProfile);
            }
        }
    }
    
    IEnumerator WaitAnimation()
    {
        yield return null;
        float duration = _core.weaponAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration / 3);

        if (_core.GetMoveState() == interruptedState && interruptedState != _core.idleState)
            PlayAnimation(interruptedAnimHash);

        yield return new WaitForSeconds(duration / 2);
        isAnimFinished = true;
    }

    protected override void LookForNextState()
    {
        if (!isAnimFinished) return;

        _core.ChangeHandState<EmptyState>();
    }
}
