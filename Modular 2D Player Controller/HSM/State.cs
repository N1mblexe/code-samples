using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Player.HSM
{
    [System.Serializable]
    public class State
    {
        [System.Serializable]
        public class Data
        {
            [Header("--- General ---")]
            public string actionName;
            public ActionPriority actionPriority = ActionPriority.NONE;
            [Range(0f, 1f)] public float cancelWindowStart = 1.0f;

            [Header("--- Animation ---")]
            public List<String> animNames;
            [Range(.1f, 2)] public float animSpeed = 1;

            [Header("--- Events ---")]
            public UnityEvent onEnter;
            public UnityEvent onExit;
        }
        [SerializeField] protected Data _data = new Data();
        public Data GetData() { return _data; }

        protected Controller _core;
        public void SetCore(Controller core) { _core = core; }
        public State(Controller core) { _core = core; }

        public void OnEnter() { _data.onEnter?.Invoke(); Enter(); }

        public void OnExit() { _data.onExit?.Invoke(); Exit(); }

        public void OnLogicUpdate()
        {
            LogicUpdate();
            LookForNextState();
        }

        public void OnPhysicsUpdate() => PhysicsUpdate();

        //TODO : implement animation speed
        public bool PlayAnimation(int hash)
        {
            _core.Anim.Play(hash, 0, 0);
            return true;
        }

        #region Wrapped Virtual Functions

        protected virtual void Enter() { }
        protected virtual void Exit() { }
        protected virtual void LogicUpdate() { }
        protected virtual void PhysicsUpdate() { }
        protected virtual void LookForNextState() { throw new NotImplementedException("Change the goddamn state"); }

        #endregion
    }
}