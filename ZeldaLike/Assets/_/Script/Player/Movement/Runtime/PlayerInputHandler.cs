using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerMovement
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerControls _controls;
        public Vector2 MoveInput { get; private set; }
        public bool FocusTriggered { get; private set; }
        public bool ActionTriggered { get; private set; }

        private void Awake()
        {
            _controls = new PlayerControls();
            _controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            _controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;
            _controls.Player.Focus.started += ctx => FocusTriggered = true;
            _controls.Player.Focus.canceled += ctx => FocusTriggered = false;
            _controls.Player.Action.started += ctx => ActionTriggered = true;
            _controls.Player.Action.canceled += ctx => ActionTriggered = false;
        }

        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }
    }
}
