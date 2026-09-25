using UnityEngine;

namespace Core.StateMachine
{
    public class StateMachineRunner : MonoBehaviour
    {
        private StateMachine _machine;

        public StateMachine Machine => _machine;

        protected virtual void OnAwake()
        {
            _machine = new StateMachine();
            InitializeStates();
        }

        /// <summary>
        /// State initilization function auto called from OnAwake
        /// Subclasses should implement their states under this method
        /// </summary>
        protected virtual void InitializeStates(){}

        protected virtual void OnUpdate()
        {
            _machine?.Update();
        }
    }
}
