using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System;

[System.Serializable]
public class InputDampener
{
    public float SmoothTime = 0.1f;
    public Vector2 SmoothedValue { get; private set; }
    private Vector2 velocity;

    public void UpdateDampening(Vector2 rawTarget)
    {
        SmoothedValue = Vector2.SmoothDamp(SmoothedValue, rawTarget, ref velocity, SmoothTime);
    }
}


public class InputManager : MonoBehaviour
{
    static public InputManager Instance;

    public Vector2 MoveInput => moveDampener.SmoothedValue;
    public Vector2 RawMoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public event Action OnJumpPerformedEvent;
    public event Action OnAttackPerformedEvent;
    public event Action OnAttackCancelledEvent;

    public event Action OnMoveStartedEvent;
    public event Action OnMoveCancelledEvent;

    [SerializeField] private InputDampener moveDampener = new InputDampener();
    [SerializeField] private int jumpBufferMs = 150;
    [SerializeField] private int attackBufferMs = 150;

    private InputSystem_Actions playerInput;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        playerInput = new InputSystem_Actions();

        playerInput.Player.Jump.performed += OnJumpPerformed;
        playerInput.Player.Attack.performed += OnAttackPerformed;
        playerInput.Player.Attack.canceled += OnAttackCancelled;

        playerInput.Player.Move.started += OnMoveStarted;
        playerInput.Player.Move.canceled += OnMoveCancelled;
    }

    private void OnEnable() => playerInput.Enable();
    private void OnDisable() => playerInput.Disable();

    private void Update()
    {
        RawMoveInput = playerInput.Player.Move.ReadValue<Vector2>();
        moveDampener.UpdateDampening(RawMoveInput);
        LookInput = playerInput.Player.Look.ReadValue<Vector2>();
    }

    private void OnMoveStarted(InputAction.CallbackContext ctx)
    {
        OnMoveStartedEvent?.Invoke();
    }

    private void OnMoveCancelled(InputAction.CallbackContext ctx)
    {
        OnMoveCancelledEvent?.Invoke();
    }

    private async void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        OnJumpPerformedEvent?.Invoke();
    }

    private async void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        OnAttackPerformedEvent?.Invoke();
    }

    private async void OnAttackCancelled(InputAction.CallbackContext ctx)
    {
        OnAttackCancelledEvent?.Invoke();
    }
}