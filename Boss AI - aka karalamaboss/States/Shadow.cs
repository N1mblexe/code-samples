using System.Collections;
using Core.StateMachine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

//20.11.2025 : bu state'tin algoritmasından hiç memnun değilim
//20.11.2025 : inşallah boş zaman bulursam revize ederim.
//23.11.2025 : lan yoksa o gün bugün mü!!!dipnot: değilmiş :(
public class Shadow : BaseState
{
    KaralamaStateMachine.InternalSharedData internalSharedData;
    public Shadow(GameObject owner, KaralamaStateMachine.SharedData sharedData, KaralamaStateMachine.InternalSharedData internalSharedData) : base(owner, sharedData)
    {
        this.internalSharedData = internalSharedData;
    }

    [Header("Razes")]
    [SerializeField] private float razeDistances = 2f;
    [SerializeField] private Vector2[] edgeRazes = new Vector2[2];

    public override void OnExit()
    {
        sharedData.shadow = false;
    }

    private bool isShadow = false;

    public override void OnStart()
    {
        sharedData.animator.SetTrigger("ShadowEnter");

        edgeRazes[0] = internalSharedData.dashPositions[0].transform.position;//left
        edgeRazes[1] = internalSharedData.dashPositions[1].transform.position;//right
    }

    public void SetShadow()
    {
        CallEventHooks();
    }

    public override void OnUpdate()
    {
        if (!isShadow)
            return;
    }

    private void CallEventHooks()
    {
        GameObject shadowObj = KaralamaStateMachine.Instance.gameObject;

        float sumX = (internalSharedData.dashPositions[0].transform.position.x + internalSharedData.dashPositions[1].transform.position.x) / 2;
        float sumY = internalSharedData.dashPositions[0].transform.position.y;

        float distance = Vector2.Distance(shadowObj.transform.position, new Vector2(sumX, sumY));
        float duration = distance / internalSharedData.shadowSpeed;

        shadowObj.transform.DOMoveX(sumX, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            sharedData.coroutineRunner.StartCoroutine(RazeThread());
        });
    }

    private float razeYAxis = -0.9f;

    private IEnumerator RazeThread()
    {
        GameObject boss = sharedData.rigidbody.gameObject;
        uint oneSideRazeAmount = (uint)(Mathf.Abs(boss.transform.position.x - edgeRazes[0].x) / razeDistances);

        for (uint i = 0; i < oneSideRazeAmount + 2; i++)
        {
            Vector2[] positions = new Vector2[2];

            positions[0] = new Vector2(boss.transform.position.x + i * razeDistances, razeYAxis);
            positions[1] = new Vector2(boss.transform.position.x - i * razeDistances, razeYAxis);

            var temp1 = GameObject.Instantiate(internalSharedData.razePrefab, positions[0], quaternion.identity);
            var temp2 = GameObject.Instantiate(internalSharedData.razePrefab, positions[1], quaternion.identity);

            GameObject.Destroy(temp1, 1f);
            GameObject.Destroy(temp2, 1f);

            yield return new WaitForSeconds(.5f);
        }
        if (UnityEngine.Random.Range(0, 100) > 50)
            for (uint i = oneSideRazeAmount; i > 0; i--)
            {
                Vector2[] positions = new Vector2[2];

                positions[0] = new Vector2(boss.transform.position.x + i * razeDistances, razeYAxis);
                positions[1] = new Vector2(boss.transform.position.x - i * razeDistances, razeYAxis);

                var temp1 = GameObject.Instantiate(internalSharedData.razePrefab, positions[0], quaternion.identity);
                var temp2 = GameObject.Instantiate(internalSharedData.razePrefab, positions[1], quaternion.identity);

                GameObject.Destroy(temp1, 1f);
                GameObject.Destroy(temp2, 1f);

                yield return new WaitForSeconds(.5f);
            }

        // Raze bitti → Final Pos'a git
        GoToFinalPos(() =>
        {
            sharedData.idleBlackActive = true;
            sharedData.shadow = false;
            sharedData.animator.SetTrigger("ShadowExit");
        });
    }



    private void GoToFinalPos(System.Action onArrive)
    {
        Transform boss = KaralamaStateMachine.Instance.transform;

        GameObject[] positions = internalSharedData.dashPositions;
        uint index = (uint)(internalSharedData.currentPos ? 0 : 1);
        internalSharedData.currentPos = !internalSharedData.currentPos;

        Transform destination = positions[index].transform;

        float distance = Mathf.Abs(boss.position.x - destination.position.x);

        float duration = distance / internalSharedData.shadowSpeed;

        boss.DOMoveX(destination.position.x, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                onArrive?.Invoke();
            });
    }




    private bool GoTo(GameObject objToMove, Transform destination, float speed, float threshold)
    {
        float distance = Mathf.Abs(objToMove.transform.position.x - destination.position.x);
        if (distance <= threshold)
            return true;

        float duration = distance / speed;

        objToMove.transform.DOMoveX(destination.position.x, duration)
            .SetEase(Ease.Linear)
            .SetUpdate(true);

        return false;
    }

}
