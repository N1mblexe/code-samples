using UnityEngine;
using System.Collections.Generic;
using System;
using Player.HSM;
using Player.States;
using System.Collections;

namespace Player
{
    public enum ActionPriority
    {
        NONE = 0,
        LOW = 10,
        MEDIUM = 20,
        HIGH = 30,
        CRITICAL = 40,
        UNSTOPPABLE = 100
    }
    public class Controller : MonoBehaviour
    {
        private Dictionary<Type, State> _states = new Dictionary<Type, State>();

        public Animator Anim { get; private set; }
        public Animator weaponAnimator;
        public Rigidbody2D Rb { get; private set; }
        public InputManager inputManager { get; private set; }
        public Collider2D Collider { get; private set; }
        public SpriteRenderer sr { get; private set; }
        public HealthManager healthManager { get; private set; }

        private State _moveState;
        private State _handState;

        public IdleState idleState;
        public RunState runState;
        public DashState dashState;
        public JumpState jumpState;
        public AirState airState;

        public EmptyState emptyState;
        public AttackState attackState;
        public ParryState parryState;

        [SerializeField] private float dashCD = 1;
        [SerializeField] private float coyoteTime = .25f;
        public bool canCoyoteJump = true;
        public bool canAirDash = true;
        private bool dashAvailable = true;

        public bool CanDash() { return dashAvailable && canAirDash; }

        #region Ground
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = .2f;

        public bool isGrounded() { return GroundChecker.IsGrounded(); }
        #endregion
        void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Anim = GetComponent<Animator>();
            inputManager = GetComponent<InputManager>();
            Collider = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            healthManager = GetComponent<HealthManager>();

            InitializeStates();
            RegisterStates();

            ChangeMoveState<IdleState>();
            ChangeHandState<EmptyState>();
        }

        void InitializeStates()
        {
            idleState.SetCore(this);
            runState.SetCore(this);
            dashState.SetCore(this);
            jumpState.SetCore(this);
            airState.SetCore(this);

            emptyState.SetCore(this);
            attackState.SetCore(this);
            parryState.SetCore(this);
        }

        void RegisterStates()
        {
            RegisterState(idleState);
            RegisterState(runState);
            RegisterState(dashState);
            RegisterState(jumpState);
            RegisterState(airState);

            RegisterState(emptyState);
            RegisterState(attackState);
            RegisterState(parryState);
        }

        private void RegisterState(State state)
        {
            if (!_states.ContainsKey(state.GetType()))
                _states.Add(state.GetType(), state);
        }

        void Update()
        {
            Debug.Log($"Current state: {_moveState}");
            _moveState?.OnLogicUpdate();
            _handState?.OnLogicUpdate();

            if (Input.GetKeyDown(KeyCode.O))
                GetComponent<IDamagable>().Damage(new GameObject(), 1);
        }

        void FixedUpdate()
        {
            _moveState?.OnPhysicsUpdate();
            _handState?.OnPhysicsUpdate();
        }

        public void ChangeMoveState<T>() where T : State
        {
            if (!_states.TryGetValue(typeof(T), out State newState))
                return;

            if (newState == dashState && CanDash())
            {
                if (_moveState == airState || _moveState == jumpState)
                    canAirDash = false;

                bool state = TryChangeState<T>(ref _moveState);

                if (state)
                    StartCoroutine(StartDashCooldown());

                return;
            }
            else if (newState == dashState && !CanDash())
                return;

            TryChangeState<T>(ref _moveState);
        }

        public void ChangeHandState<T>() where T : State
        {
            TryChangeState<T>(ref _handState);
        }

        private bool TryChangeState<T>(ref State currentSlot) where T : State
        {
            if (!_states.TryGetValue(typeof(T), out State newState))
            {
                Debug.LogWarning($"State {typeof(T)} kayıtlı değil!");
                return false;
            }

            if (currentSlot == newState) return false;

            if (currentSlot == null)
            {
                currentSlot = newState;
                currentSlot.OnEnter();
                return false;
            }

            //if (currentSlot.GetData().actionPriority > newState.GetData().actionPriority)
            //    throw new System.Exception("Priority problem");

            currentSlot.OnExit();
            currentSlot = newState;
            currentSlot.OnEnter();
            return true;
        }
        public State GetMoveState() => _moveState;
        public State GetHandState() => _handState;
        public bool IsLookingRight() { return (transform.localScale.x == 1) ? true : false; }
        public float GetFacedAxis() { return transform.localScale.x; }

        public IEnumerator StartDashCooldown()
        {
            dashAvailable = false;
            yield return new WaitForSeconds(dashCD);
            dashAvailable = true;
        }

        public void StartCoyoteTime() => StartCoroutine(CoyoteThread());
        private IEnumerator CoyoteThread()
        {
            canCoyoteJump = true;
            yield return new WaitForSeconds(coyoteTime);
            canCoyoteJump = false;
        }
    }
}