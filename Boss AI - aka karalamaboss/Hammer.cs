using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemy.Prop
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Hammer : MonoBehaviour
    {
        private int damage = 0;
        private Animator animator;
        private KaralamaStateMachine.InternalSharedData internalSharedData;
        private KaralamaStateMachine.SharedData sharedData;
        private Rigidbody2D rb;

        [SerializeField] private GameObject distortionRing;

        private Coroutine goToPositionCoroutine;

        private enum State { Smash, Hole, Spear }
        [SerializeField] private State state = State.Smash;

        private float elapsed;
        private float dashDuration;

        void Start()
        {
            internalSharedData = KaralamaStateMachine.Instance.internalSharedData;
            sharedData = KaralamaStateMachine.Instance.sharedData;

            damage = internalSharedData.h_damage;

            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            if (state == State.Hole)
            {
                animator.SetTrigger("Hole");
                SwitchToHole(transform.position);
                return;
            }
        }

        #region Unity_Events

        [SerializeField] private float smashForce = 2000;

        public void Smash()
        {
            distortionRing.SetActive(true);

            sharedData.coroutineRunner.StartCoroutine(RazeThread());

            var dict = KaralamaStateMachine.Instance.GetExploitDictionary();

            foreach (var e in dict)
            {
                if (e.Value == null)
                    continue;

                var rb = e.Value.GetComponent<Rigidbody2D>();

                if (rb == null)
                    continue;

                Vector3 vector = (e.Value.transform.position - transform.position);
                rb.AddForce(vector * smashForce / Vector2.Distance(e.Value.transform.position, transform.position), ForceMode2D.Impulse);
            }
        }

        public void SwitchToHole()
        {
            StartCoroutine(SwitchToHoleDelayed());
        }
        public void SwitchToHole(Vector2 pos)
        {
            state = State.Hole;
            damage = internalSharedData.h_TrapDamage;
            if (Vector2.Distance(transform.position, pos) < .1f)
            {
                SetStateToTrap();
                return;
            }

            Vector2 targetPosition = (pos == null) ? sharedData.player.transform.position : pos;

            targetPosition.y = transform.position.y;

            if (goToPositionCoroutine != null)
                StopCoroutine(goToPositionCoroutine);

            goToPositionCoroutine = StartCoroutine(GoToPosition(sharedData.player.transform, speed: 40));
        }

        public IEnumerator SwitchToHoleDelayed(float time = 1)
        {
            yield return new WaitForSeconds(time);

            state = State.Hole;
            damage = internalSharedData.h_TrapDamage;

            Vector2 targetPosition = sharedData.player.transform.position;

            targetPosition.y = transform.position.y;

            if (goToPositionCoroutine != null)
                StopCoroutine(goToPositionCoroutine);

            goToPositionCoroutine = StartCoroutine(GoToPosition(sharedData.player.transform, speed: 40f));
        }


        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        #endregion
        private IEnumerator GoToPosition(Transform targetTransform, float speed)
        {
            while (true)
            {
                Vector2 currentPosition = rb.position;
                float targetX = targetTransform.position.x;
                float xDistance = Mathf.Abs(targetX - currentPosition.x);

                if (xDistance < 0.2f)
                {
                    SetStateToTrap();
                    yield break;
                }

                float direction = Mathf.Sign(targetX - currentPosition.x);
                float newX = currentPosition.x + direction * speed * Time.deltaTime;
                Vector2 newPos = new Vector2(newX, currentPosition.y);
                rb.MovePosition(newPos);

                yield return null;
            }
        }


        private float razeDistances = 1.7f;

        private Vector2 lastPos;

        private IEnumerator RazeThread()
        {
            lastPos = internalSharedData.dashPositions[(internalSharedData.currentPos) ? 0 : 1].transform.position;
            int sign = (internalSharedData.currentPos) ? -1 : 1;

            Vector2 startPos = distortionRing.transform.position;
            uint razeAmount = (uint)(Mathf.Abs(startPos.x - lastPos.x) / razeDistances);

            for (uint i = 0; i < razeAmount + 1; i++)
            {
                if (i == 0)
                    continue;

                Vector2 position = new Vector2(startPos.x + i * razeDistances * sign, -0.9f);

                var temp1 = GameObject.Instantiate(internalSharedData.razePrefab, position, quaternion.identity);

                GameObject.Destroy(temp1, 1f);

                yield return new WaitForSeconds(.2f);
            }
        }


        private void SetStateToTrap()
        {
            StopAllCoroutines();
            animator.SetTrigger("Trap");
            state = State.Spear;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") == false)
            {
                Debug.LogWarning("Player Tagı değil");
                return;
            }

            switch (state)
            {
                case State.Smash:
                    collision.gameObject.GetComponent<IDamagable>().Damage(this.gameObject , 2);
                    break;

                case State.Hole:
                    SetStateToTrap();
                    return;

                case State.Spear:
                    collision.gameObject.GetComponent<IDamagable>().Damage(this.gameObject , 1);
                    break;
            }
        }
    }
}