using System.Collections;
using Dialogue;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Input buffered modular input manager class for souls like , hack and slash and similar kind of genres  
    /// </summary>
    public class InputManager : MonoBehaviour, PlayerInputSystem.IPlayerActions
    {
        private PlayerInputSystem input;
        public static InputManager Instance;
        public class InputSignal
        {
            public InputSignal(float bufferTime = 0.1f)
            {
                this.bufferTime = bufferTime;
            }
            private bool trigger;
            private float bufferTime = .1f;
            private Coroutine currentThread;
            public bool GetTrigger()
            {
                if(trigger)
                {
                    trigger = false;
                    return true;
                }

                return false; 
            }
            public void RaiseTrigger()
            {
                if(currentThread != null)
                    InputManager.Instance.StopCoroutine(currentThread);

                trigger = true;

                currentThread = InputManager.Instance.StartCoroutine(StartBuffer());
            }
            private IEnumerator StartBuffer()
            {
                trigger = true;
                yield return new WaitForSeconds(bufferTime);
                trigger = false;

                currentThread = null;
            }
        }

        public Vector2 MoveInput { get; private set; }
        public InputSignal dash;
        public InputSignal Jump;
        public InputSignal Attack;
        public InputSignal Parry;
        public bool IsJumpHeld { get; private set; }
        public bool IsJumpCanceled { get; private set; }

        private void Awake()
        {
            input = new PlayerInputSystem();
            input.Player.SetCallbacks(this);

            dash = new InputSignal();
            Jump = new InputSignal(.2f);
            Attack = new InputSignal();
            Parry = new InputSignal();

            if(Instance == null)
                Instance = this;
        }

        private void OnEnable() => input.Player.Enable();
        private void OnDisable() => input.Player.Disable();

        private void LateUpdate()
        {
            IsJumpCanceled = false;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Jump.RaiseTrigger();
                IsJumpHeld = true;
            }

            if (context.canceled)
            {
                IsJumpHeld = false;
                IsJumpCanceled = true;
            }
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                Attack.RaiseTrigger();
        }

        public void OnParry(InputAction.CallbackContext context)
        {
            if (context.performed)
                Parry.RaiseTrigger();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
                dash.RaiseTrigger();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if(context.performed)
                DialogueStarter.instance.TryStartDialogue();
                
        }
    }
}