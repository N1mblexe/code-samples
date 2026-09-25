using UnityEngine;
using DG.Tweening;
using System;

public class ChainManager : MonoBehaviour
{
    [SerializeField] public GameObject lantern;
    [SerializeField] private Rigidbody2D[] chainLinks;
    [SerializeField] private GameObject lastChain;

    public GameObject GetLastChain() { return lastChain; }

    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

    [SerializeField] private float resetDuration = 0.3f;

    void Start()
    {
        originalPositions = new Vector3[chainLinks.Length];
        originalRotations = new Quaternion[chainLinks.Length];

        for (int i = 0; i < chainLinks.Length; i++)
        {
            var rb = chainLinks[i].GetComponent<Rigidbody2D>();
            rb.drag = 1.2f;
            rb.angularDrag = 2f;
            originalPositions[i] = chainLinks[i].transform.position;
            originalRotations[i] = chainLinks[i].transform.rotation;
        }
    }

    public void ResetChainSmooth(float time)
    {
        resetDuration = time;

        foreach (var rb in chainLinks)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        for (int i = 0; i < chainLinks.Length; i++)
        {
            Rigidbody2D rb = chainLinks[i];

            rb.transform.DOMove(originalPositions[i], resetDuration).SetEase(Ease.InOutQuad);
            rb.transform.DORotateQuaternion(originalRotations[i], resetDuration).SetEase(Ease.InOutQuad);
        }

        DOVirtual.DelayedCall(resetDuration, () =>
        {
            foreach (var rb in chainLinks)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.WakeUp();
            }

            Physics2D.SyncTransforms();
        });
    }
}
