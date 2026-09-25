using System.Collections;
using Core.StateMachine;
using UnityEngine;

public class Dash : BaseState
{
    private Rigidbody2D rb;

    private Vector2 startPos;
    private Vector2 targetPos;
    private float dashDuration;
    private float elapsed = 0f;
    private float dashSpeed = 40f;

    private KaralamaStateMachine.InternalSharedData internalSharedData;

    public Dash(GameObject owner, KaralamaStateMachine.SharedData sharedData, KaralamaStateMachine.InternalSharedData internalSharedData)
        : base(owner, sharedData)
    {
        this.rb = sharedData.rigidbody;
        this.internalSharedData = internalSharedData;
    }

    public override void OnStart()
    {
        sharedData.isDashing = false;
        sharedData.idleBlackActive = false;
        sharedData.coroutineRunner.StartCoroutine(DashSequence());
    }

    private IEnumerator DashSequence()
    {
        bool animFinished = false;
        PlayAnimationAndWait("DashEnter", () => animFinished = true);
        yield return new WaitUntil(() => animFinished);

        float waitTime = UnityEngine.Random.Range(0.1f, 0.4f);
        yield return new WaitForSeconds(waitTime);

        animFinished = false;
        PlayAnimationAndWait("Dash", () => animFinished = true);

        startPos = rb.position;
        targetPos = internalSharedData.dashPositions[(internalSharedData.currentPos ? 0 : 1)].transform.position;
        internalSharedData.currentPos = !internalSharedData.currentPos;
        targetPos.y = owner.transform.position.y;

        float distance = Vector2.Distance(startPos, targetPos);
        dashDuration = distance / dashSpeed;

        sharedData.isDashing = true;
        elapsed = 0f;

        int distanceAmount = (int)(Mathf.Abs(distance) / 3f);
        float pos = startPos.x;
        bool movingRight = targetPos.x > startPos.x;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dashDuration);

            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);

            bool shouldSpawn = movingRight ? newPos.x >= pos : newPos.x <= pos;

            if (shouldSpawn)
            {
                Vector2 spawnPos = KaralamaStateMachine.Instance.transform.position;
                spawnPos.y = -0.5f; // hammer spawn pos
                GameObject.Instantiate(internalSharedData.trapPrefab, spawnPos + Vector2.up * .5f, Quaternion.identity);

                pos += movingRight ? distanceAmount : -distanceAmount;
            }

            rb.MovePosition(newPos);
            yield return null;
        }

        yield return new WaitUntil(() => animFinished);

        sharedData.isDashing = false;
        sharedData.idleBlackActive = true;
    }

    private void FaceToDashDirection()
    {
        GameObject boss = KaralamaStateMachine.Instance.gameObject;


        Vector3 localScale = boss.transform.localScale;

        if (0 < boss.transform.position.x)
            localScale.x = -Mathf.Abs(localScale.x);
        else
            localScale.x = Mathf.Abs(localScale.x);

        boss.transform.localScale = localScale;
    }

    public override void OnExit()
    {
        sharedData.isDashing = false;
    }

    public override void OnUpdate()
    { }
}
