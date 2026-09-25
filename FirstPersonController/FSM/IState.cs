namespace Core
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnLateUpdate();
        void OnExit();
    }

    public abstract class BaseState<TContext> : IState
    {
        protected readonly TContext Context;
        protected readonly FSM<TContext> machine;

        public BaseState(TContext context, FSM<TContext> fsm)
        {
            Context = context;
            machine = fsm;
        }

        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnExit() { }
    }
}