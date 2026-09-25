using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.StateMachine
{
    public abstract class BaseState
    {
        private List<StateTransition> _transitions = new List<StateTransition>();
        protected GameObject owner;
        protected Animator animator;
        protected KaralamaStateMachine.SharedData sharedData;

        private bool animationStarted = false;
        private string currentAnimationName;

        public BaseState(GameObject owner, KaralamaStateMachine.SharedData sharedData)
        {
            this.owner = owner;
            this.animator = sharedData.animator;
            this.sharedData = sharedData;
        }

        public void AddTransition(Func<bool> condition, BaseState targetState)
        {
            _transitions.Add(new StateTransition(condition, targetState));
        }

        public BaseState CheckTransitions()
        {
            foreach (var transition in _transitions)
            {
                if (transition.Condition())
                    return transition.TargetState;
            }
            return null;
        }

        public abstract void OnStart();
        public abstract void OnUpdate();
        public abstract void OnExit();

        protected void PlayAnimationAndWait(string animationName, Action onAnimationComplete)
        {
            if (string.IsNullOrEmpty(animationName))
            {
                Debug.LogError("Animation name is null or empty!");
                return;
            }

            animator.SetTrigger(animationName);
            currentAnimationName = animationName;
            animationStarted = true;
            sharedData.coroutineRunner.StartCoroutine(WaitForAnimation(onAnimationComplete));
        }

        private IEnumerator WaitForAnimation(Action onComplete, float timeout = 3f)
        {
            if (animator == null)
            {
                Debug.LogError("Animator is null!");
                yield break;
            }

            if (!animator.enabled)
            {
                Debug.LogError("Animator is disabled!");
                yield break;
            }

            if (string.IsNullOrEmpty(currentAnimationName))
            {
                Debug.LogError("Current animation name is null or empty!");
                yield break;
            }

            float elapsed = 0f;
            bool animationStarted = false;

            while (elapsed < timeout)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.IsName(currentAnimationName))
                {
                    animationStarted = true;

                    if (stateInfo.normalizedTime >= 1f)
                    {
                        Debug.Log($"Animation '{currentAnimationName}' completed.");
                        onComplete?.Invoke();
                        yield break;
                    }
                }

                if (!animationStarted)
                {
                    Debug.LogWarning($"Waiting for animation '{currentAnimationName}' to start...");
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.LogError($"Animation '{currentAnimationName}' did not finish within {timeout} seconds.");

            onComplete?.Invoke();
        }

    }
}
