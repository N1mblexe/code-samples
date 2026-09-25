using UnityEngine;

namespace Player.HSM
{
    public class Machine : MonoBehaviour
    {
        protected State m_initialState;
        private State m_currentState;

        private void Start()
        {
            ChangeState(m_initialState);
        }

        private void Update()
        {
            m_currentState.OnLogicUpdate();
        }

        private void FixedUpdate()
        {
            m_currentState.OnPhysicsUpdate();
        }

        public void ChangeState(State newState)
        {
            if (m_currentState != null)
                m_currentState.OnExit();

            m_currentState = newState;

            if (m_currentState != null)
                m_currentState.OnEnter();
        }
    }
}