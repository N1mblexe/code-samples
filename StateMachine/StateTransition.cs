using System;

namespace Core.StateMachine
{
    public class StateTransition
    {
        public Func<bool> Condition { get; }
        public BaseState TargetState { get; }

        public StateTransition(Func<bool> condition, BaseState targetState)
        {
            Condition = condition;
            TargetState = targetState;
        }
    }
}
