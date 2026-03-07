using Codice.CM.Common.Tree;
using UnityEngine;

namespace PlayerMovement
{
    public class PlayerBrain : BBehaviour.Runtime.BBehaviour
    {
        public PlayerInputHandler InputHandler { get; private set; }
        public CharacterController controller { get; private set; }
        public Camera mainCamera { get; private set; }
        public PlayerFocus focusScanner { get; private set; }
        public Animator playerAnimator { get; private set; }

        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 15f;
        public float moveSmoothTime = 0.1f;

        [Header("Focus Settings")]
        public float strafeSpeed = 6f;
        public float focusForwardSpeed = 2f;

        [Header("Dodge Settings")]
        public float backFlipDistance = 8f;
        public float backFlipHeight = 4f;
        public float sideHopForce = 6f;
        public float sidehopHeight = 2f;
        public float dodgeDuration = 0.5f;

        private PlayerState _currentState;
        

        private void Awake()
        {
            InputHandler = GetComponent<PlayerInputHandler>();
            controller = GetComponent<CharacterController>();
            focusScanner = GetComponent<PlayerFocus>();
            playerAnimator = GetComponentInChildren<Animator>();
            mainCamera = Camera.main;
        }

        private void Start()
        {
            ChangeState(new PlayerStateExploration());

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void RecenterCamera()
        {
            Vector3 camForward = mainCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            if (camForward != Vector3.zero)
            {
                transform.forward = camForward;
            }
        }
        public void ChangeState(PlayerState newState)
        {
            if (_currentState != null)
            {
                _currentState.Exit(this);
            }

            _currentState = newState;

            if (_currentState != null)
            {
                _currentState.Enter(this);
            }
        }

        private void Update()
        {
            if (_currentState != null)
            {
                _currentState.Update();
            }
        }

        private void FixedUpdate()
        {
            if (_currentState != null)
            {
                _currentState.FixedUpdate();
            }
        }
    }
}
