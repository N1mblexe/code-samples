using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public abstract class FSM<TContext> : MonoBehaviour
    {
        protected TContext Context { get; private set; }
        private readonly Dictionary<Type, IState> states = new Dictionary<Type, IState>();
        private IState currentState;

        public void Initialize(TContext context)
        {
            Context = context;
            InitializeStates();
        }

        protected abstract void InitializeStates();

        protected void AddState(IState state)
        {
            states[state.GetType()] = state;
        }

        public void ChangeState<T>() where T : IState
        {
            if (states.TryGetValue(typeof(T), out IState nextState))
            {
                currentState?.OnExit();
                currentState = nextState;
                currentState?.OnEnter();
            }
        }

        protected virtual void Update() => currentState?.OnUpdate();
        protected virtual void FixedUpdate() => currentState?.OnFixedUpdate();
        protected virtual void LateUpdate() => currentState?.OnLateUpdate();
    }
}