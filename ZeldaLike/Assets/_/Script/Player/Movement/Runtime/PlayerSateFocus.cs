using System;
using UnityEngine;

namespace PlayerMovement
{
    public class PlayerStateFocus : PlayerState
    {
        private PlayerBrain _brain;
        private Transform _target;
        private bool _wasActionPressed = false;

        private float _velocityY = 0f;
        private Vector3 _currentMovement = Vector3.zero;
        private Vector3 _velocityRef = Vector3.zero;

        private float _dodgeTimer = 0f;

        public override void Enter(PlayerBrain brain)
        {
            base.Enter(brain);
            _brain = brain;
            _target = _brain.focusScanner.currentTarget;
            _brain.playerAnimator.SetBool("IsFocusing", true);
            _brain.Verbose("ENTRÉE EN MODE FOCUS sur : " + _target.name);
        }
        public override void Exit(PlayerBrain brain)
        {
            base.Exit(brain);
            _brain.playerAnimator.SetBool("IsFocusing", false);
            _brain.Verbose("SORTIE DU MODE FOCUS");
        }

        public override void Update()
        {
            // 1. Sortie du Focus
            if (!_brain.InputHandler.FocusTriggered || _target == null)
            {
                _brain.focusScanner.ClearTarget();
                _brain.ChangeState(new PlayerStateExploration());
                return;
            }

            // 2. Gestion des inputs d'Action (Sauts/Esquives)
            bool isActionPressed = _brain.InputHandler.ActionTriggered;
            if (isActionPressed && !_wasActionPressed && _brain.controller.isGrounded && _dodgeTimer <= 0f)
            {
                Vector2 input = _brain.InputHandler.MoveInput;

                if (input.y < -0.5f) 
                {
                    _brain.Verbose("ACTION : BACKFLIP !");
                    StartDodge(-_brain.transform.forward, _brain.backFlipDistance, _brain.backFlipHeight);
                }
                else if (Mathf.Abs(input.x) > 0.5f)
                {
                    _brain.Verbose("ACTION : SIDE HOP !");
                    float directionSign = Mathf.Sign(input.x);
                    StartDodge(_brain.transform.right * directionSign, _brain.sideHopForce, _brain.sidehopHeight);
                }
                else if (input.y > 0.5f)
                {
                    _brain.Verbose("ACTION : DASH AVANT !");
                    StartDodge(_brain.transform.forward, _brain.strafeSpeed * 2f, _brain.sidehopHeight);
                }
                else
                {
                    _brain.Verbose("ACTION : Attaque de base !");
                }
            }
            _wasActionPressed = isActionPressed;
        }

        private void StartDodge(Vector3 direction, float force, float height)
        {
            _currentMovement = direction * force;
            _velocityY = MathF.Sqrt(height * -2f * Physics.gravity.y);
            _dodgeTimer = _brain.dodgeDuration;
        }

        public override void FixedUpdate()
        {
            if (_target == null) return;
            Vector2 input = _brain.InputHandler.MoveInput;

            // 1. Z-TARGETING : Le joueur fixe la cible (sur l'axe horizontal)
            Vector3 lookPosition = new Vector3(_target.position.x, _brain.transform.position.y, _target.position.z);
            _brain.transform.LookAt(lookPosition);

            if (_dodgeTimer > 0f)
            {
                // Si on esquive, on réduit le timer et on n'écoute PAS la manette
                _dodgeTimer -= Time.fixedDeltaTime;
            }
            else
            {
                // Si on n'esquive pas, on lit la manette et on applique l'inertie
                Vector3 targetMovement = (_brain.transform.right * input.x * _brain.strafeSpeed) + 
                                         (_brain.transform.forward * input.y * _brain.focusForwardSpeed);

                _currentMovement = Vector3.SmoothDamp(_currentMovement, targetMovement, ref _velocityRef, _brain.moveSmoothTime);
            }

            // 3. GRAVITÉ ROBUSTE
            if (_brain.controller.isGrounded)
            {
                if (_velocityY < 0f)
                {
                    _velocityY = -2f; // Colle au sol
                }
            }
            else
            {
                _velocityY += Physics.gravity.y * 2f * Time.fixedDeltaTime; // Accélération de la chute
            }

            // 4. ASSEMBLAGE ET DÉPLACEMENT
            Vector3 finalMovement = _currentMovement + (Vector3.up * _velocityY);
            _brain.controller.Move(finalMovement * Time.fixedDeltaTime);
            // 5. Animation
            _brain.playerAnimator.SetFloat("InputX", input.x);
            _brain.playerAnimator.SetFloat("InputY", input.y);
        }
    }
}