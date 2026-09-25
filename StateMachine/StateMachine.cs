namespace Core.StateMachine
{
    public class StateMachine
    {
        private BaseState _currentState;

        public void ChangeState(BaseState newState)
        {
            if (_currentState == newState) return;

            _currentState?.OnExit();
            _currentState = newState;
            _currentState?.OnStart();
        }

        public void Update()
        {
            if (_currentState == null) return;

            _currentState.OnUpdate();

            var next = _currentState.CheckTransitions();
            if (next != null)
                ChangeState(next);
        }

        public BaseState GetCurrentState() => _currentState;
    }
}