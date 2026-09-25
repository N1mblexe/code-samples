using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;
using UnityEngine;

namespace Enemy.Prop
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Spear : MonoBehaviour
    {
        private int damage = 0;
        private Animator animator;
        private KaralamaStateMachine.InternalSharedData internalSharedData;
        private KaralamaStateMachine.SharedData sharedData;
        private Rigidbody2D rb;
        [SerializeField] private float throwSpeed = 17.0f;
        private GameObject attachedPlayer = null;
        private bool hasHitWall = false;
        [SerializeField] private GameObject child;
        private BoxCollider2D childCollider;

        private enum State { Spear, Throw, ExploitObj };
        private State state = State.Spear;

        [SerializeField] private GameObject lanternPrefab;

        void Start()
        {
            sharedData = KaralamaStateMachine.Instance.sharedData;
            internalSharedData = KaralamaStateMachine.Instance.internalSharedData;
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            LookAt2D(sharedData.player.transform.position, 0.4f, () =>
            {
                Throw();
            });

            childCollider = child.GetComponent<BoxCollider2D>();
        }

        private void Throw()
        {
            state = State.Throw;

            rb.velocity = transform.right * throwSpeed * ((transform.localScale.x < 0) ? -1 : 1);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (state != State.Throw) return;

            if (collision.CompareTag("Player") && attachedPlayer == null)
            {
                attachedPlayer = collision.gameObject;
                attachedPlayer.transform.SetParent(transform);

                collision.gameObject.GetComponent<IDamagable>().Damage(this.gameObject , 1);

                Debug.Log("Spear hit the player!");
            }
            else if (collision.CompareTag("Wall"))
            {
                if (attachedPlayer != null)
                {
                    rb.velocity = Vector2.zero;
                    hasHitWall = true;

                    StartCoroutine(ReleasePlayerAfterDelay(2f));
                }
                else
                {
                    rb.velocity = Vector2.zero;

                    animator.SetTrigger("Collide");
                    state = State.ExploitObj;
                }
            }
        }



        private Dictionary<Vector2, GameObject> exploitDictionary;
        private Vector2? lanternPos;
        private void GoToLanternPosition()
        {
            exploitDictionary = KaralamaStateMachine.Instance.GetExploitDictionary();
            var chainDictionary = KaralamaStateMachine.Instance.GetChainDictionary();

            lanternPos = FindEmptyPos(exploitDictionary);

            if (lanternPos == null || !chainDictionary.ContainsKey(lanternPos.Value))
            {
                GameObject.Destroy(gameObject);
                return;
            }
            transform.DORotate(new Vector3(), 0.3f);

            chainDictionary[lanternPos.Value].ResetChainSmooth(.3f);

            transform.DOMove(lanternPos.Value, 0.4f).OnComplete(() =>
            {
                animator.SetTrigger("Lantern");
            });
        }

        private void CreateLantern()
        {
            var obj = Instantiate(lanternPrefab, transform.position, new Quaternion());
            exploitDictionary[lanternPos.Value] = obj;
            obj.GetComponent<HingeJoint2D>().connectedBody = KaralamaStateMachine.Instance.GetChainDictionary()[lanternPos.Value].GetLastChain().GetComponent<Rigidbody2D>();

            Destroy(this.gameObject);
        }


        private Vector2? FindEmptyPos(Dictionary<Vector2, GameObject> dictionary)
        {
            foreach (var ex in dictionary)
            {
                if (ex.Value == null)
                    return ex.Key;
            }

            return null;
        }

        private IEnumerator ReleasePlayerAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (attachedPlayer != null)
            {
                attachedPlayer.transform.SetParent(null);
                attachedPlayer = null;
            }

            animator.SetTrigger("Collide");
            state = State.ExploitObj;
        }

        public void LookAt2D(Vector2 target, float duration = 0.2f, Action onComplete = null)
        {
            Vector2 direction = target - (Vector2)transform.position;

            if (direction.sqrMagnitude < Mathf.Epsilon)
            {
                onComplete?.Invoke();
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (transform.localScale.x < 0)
                angle += 180f;

            transform.DORotate(new Vector3(0, 0, angle), duration, RotateMode.Fast)
                     .SetEase(Ease.OutSine)
                     .OnComplete(() => onComplete?.Invoke());
        }

    }
}
