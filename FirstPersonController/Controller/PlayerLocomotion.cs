using UnityEngine;
using Data;

/// <summary>
/// Handles all player movement: walking, jumping (wind-up → thrust → airborne),
/// landing penalties, and gravity. Reads input from <see cref="InputManager"/>
/// and writes shared state to the static <see cref="Movement"/> data class.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    // ─── Constants ───────────────────────────────────────────────────────────

    /// Horizontal speed below which we snap to zero to avoid infinite deceleration.
    private const float STOP_THRESHOLD = 0.1f;

    /// Vertical speed (downward) above this on landing triggers the speed penalty.
    private const float IMPACT_THRESHOLD = 4f;

    /// Maximum impact force used when clamping the landing calculation.
    private const float MAX_IMPACT_FORCE = 25f;

    // ─── Inspector Fields ─────────────────────────────────────────────────────

    [Header("Movement")]
    public float walkSpeed      = 3f;
    public float acceleration   = 5f;
    public float deceleration   = 15f;
    [Range(0f, 1f)] public float airControl = 0.2f;

    [Header("Landing Penalty")]
    [Range(0f, 1f)] public float windUpSpeedMultiplier  = 0.3f;
    [Range(0f, 1f)] public float landSpeedMultiplier    = 0.1f;
    public float penaltyRecoverySpeed = 4f;

    [Header("Gravity & Jump")]
    public float gravity         = -19.62f;
    public float groundStickForce = -2f;
    public float jumpHeight      = 1.2f;
    public float jumpWindUpTime  = 0.25f;
    public float jumpThrustTime  = 0.15f;

    // ─── Jump State ───────────────────────────────────────────────────────────

    private enum JumpState { Grounded, WindUp, Thrusting, Airborne }

    // ─── Private State ────────────────────────────────────────────────────────

    private CharacterController _controller;
    private InputManager        _inputManager;

    private Vector3 _horizontalVelocity;
    private float   _verticalVelocity;

    private JumpState _jumpState = JumpState.Grounded;
    private float     _jumpTimer;
    private float     _targetJumpVelocity;
    private float     _penaltyMultiplier = 1f;
    private bool      _wasGrounded;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        _controller   = GetComponent<CharacterController>();
        _inputManager = GetComponent<InputManager>();
    }

    private void OnEnable()
    {
        _inputManager.OnJumpPerformedEvent += HandleJumpInput;
        _inputManager.OnMoveStartedEvent   += HandleMoveStarted;
        _inputManager.OnMoveCancelledEvent += HandleMoveCancelled;
    }

    private void OnDisable()
    {
        _inputManager.OnJumpPerformedEvent -= HandleJumpInput;
        _inputManager.OnMoveStartedEvent   -= HandleMoveStarted;
        _inputManager.OnMoveCancelledEvent -= HandleMoveCancelled;
    }

    private void Update()
    {
        UpdateVertical();
        UpdateHorizontal();

        _controller.Move((_horizontalVelocity + Vector3.up * _verticalVelocity) * Time.deltaTime);

        HandleLanding();
        SyncMovementState();
    }

    // ─── Input Callbacks ──────────────────────────────────────────────────────

    private void HandleMoveStarted()   => Movement.OnMovementStarted?.Invoke();
    private void HandleMoveCancelled() => Movement.OnMovementStopped?.Invoke();

    private void HandleJumpInput()
    {
        bool canJump = _controller.isGrounded && _jumpState == JumpState.Grounded;
        if (!canJump) return;

        _jumpState = JumpState.WindUp;
        _jumpTimer = jumpWindUpTime;
        Movement.OnJumpWindUp?.Invoke();
    }

    // ─── Vertical Movement ────────────────────────────────────────────────────

    private void UpdateVertical()
    {
        switch (_jumpState)
        {
            case JumpState.Grounded:
            case JumpState.WindUp:
                HandleGroundedVertical();
                break;

            case JumpState.Thrusting:
                TickThrust();
                break;

            case JumpState.Airborne:
                ApplyGravity();
                break;
        }
    }

    /// Manages ground-sticking and the wind-up countdown while on the ground.
    private void HandleGroundedVertical()
    {
        if (!_controller.isGrounded)
        {
            // Player walked off a ledge during wind-up — cancel it.
            _jumpState = JumpState.Airborne;
            return;
        }

        if (_verticalVelocity < 0f)
            _verticalVelocity = groundStickForce;

        if (_jumpState == JumpState.WindUp)
            TickWindUp();
    }

    /// Counts down the wind-up timer, then fires the jump thrust.
    private void TickWindUp()
    {
        _jumpTimer -= Time.deltaTime;
        if (_jumpTimer > 0f) return;

        _jumpState          = JumpState.Thrusting;
        _verticalVelocity   = 0f;
        _targetJumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        Movement.OnJumped?.Invoke();
    }

    /// Rapidly accelerates upward until the target jump velocity is reached.
    private void TickThrust()
    {
        float thrustAcceleration = _targetJumpVelocity / jumpThrustTime;
        _verticalVelocity = Mathf.MoveTowards(
            _verticalVelocity, _targetJumpVelocity,
            thrustAcceleration * Time.deltaTime
        );

        if (_verticalVelocity >= _targetJumpVelocity)
            _jumpState = JumpState.Airborne;
    }

    private void ApplyGravity() => _verticalVelocity += gravity * Time.deltaTime;

    // ─── Horizontal Movement ──────────────────────────────────────────────────

    private void UpdateHorizontal()
    {
        RecoverPenalty();

        Vector2 rawInput     = _inputManager.RawMoveInput;
        Vector3 moveDirection = (transform.right * rawInput.x + transform.forward * rawInput.y).normalized;

        float targetSpeed = GetCurrentTargetSpeed();
        Vector3 targetVelocity = moveDirection * targetSpeed;

        float lerpSpeed = ChooseLerpSpeed(moveDirection);
        _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetVelocity, lerpSpeed * Time.deltaTime);

        SnapToZeroIfStopped(moveDirection);
    }

    private void RecoverPenalty()
    {
        _penaltyMultiplier = Mathf.Lerp(_penaltyMultiplier, 1f, penaltyRecoverySpeed * Time.deltaTime);
    }

    /// Returns the effective max speed, accounting for the penalty and jump states.
    private float GetCurrentTargetSpeed()
    {
        float speed = walkSpeed * _penaltyMultiplier;

        bool isJumpTransition = _jumpState == JumpState.WindUp || _jumpState == JumpState.Thrusting;
        if (isJumpTransition)
            speed *= windUpSpeedMultiplier;

        return speed;
    }

    private float ChooseLerpSpeed(Vector3 moveDirection)
    {
        bool isMoving = moveDirection.magnitude > 0f;
        float baseSpeed = isMoving ? acceleration : deceleration;

        bool isInAir = _jumpState == JumpState.Airborne;
        return isInAir ? baseSpeed * airControl : baseSpeed;
    }

    private void SnapToZeroIfStopped(Vector3 moveDirection)
    {
        if (moveDirection.magnitude == 0f && _horizontalVelocity.sqrMagnitude < STOP_THRESHOLD)
            _horizontalVelocity = Vector3.zero;
    }

    // ─── Post-Move Events ─────────────────────────────────────────────────────

    /// Detects landing and applies a speed penalty proportional to impact velocity.
    private void HandleLanding()
    {
        bool isGroundedNow  = _controller.isGrounded;
        bool justLanded     = !_wasGrounded && isGroundedNow && _jumpState != JumpState.Thrusting;


        if (justLanded)
        {
            float impactForce = Mathf.Clamp(-_verticalVelocity, 0f, MAX_IMPACT_FORCE);
            if (impactForce > IMPACT_THRESHOLD)
                _penaltyMultiplier = landSpeedMultiplier;

            if (_jumpState == JumpState.Airborne)
                _jumpState = JumpState.Grounded;
                
            Movement.OnLanded?.Invoke(impactForce);
        }

        _wasGrounded       = isGroundedNow;
        Movement.isGrounded = isGroundedNow;
    }

    /// Writes velocity data to the shared <see cref="Movement"/> state object.
    private void SyncMovementState()
    {
        Movement.playerVelocity = _controller.velocity;
        Movement.playerSpeed    = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
    }
}