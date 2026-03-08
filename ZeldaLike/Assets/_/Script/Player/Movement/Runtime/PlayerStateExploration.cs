using UnityEngine;
using Interact;

namespace PlayerMovement
{
    public class PlayerStateExploration : PlayerState
    {
        private PlayerBrain _brain;

        private float _velocityY = 0f;
        private Vector3 _currentMovement = Vector3.zero;
        private Vector3 _velocityRef = Vector3.zero;
        private bool _wasFocusPressed = false;

        private bool _wasActionPressed = false; 
        private float _rollTimer = 0f;

        public override void Enter(PlayerBrain brain)
        {
            base.Enter(brain);
            _brain = brain;
        }
        
        public override void Update()
        {
            // 1. On récupère les inputs
            Vector2 input = _brain.InputHandler.MoveInput;

            if (input != Vector2.zero)
            {
                _brain.Verbose("Player is moving in exploration state with input: " + input);
            }

            // --- LOGIQUE DE FOCUS ---
            bool isFocusPressed = _brain.InputHandler.FocusTriggered;

            if (isFocusPressed && !_wasFocusPressed)
            {
                if (_brain.focusScanner.TryFindTarget(_brain.mainCamera.transform))
                {
                    _brain.Verbose("CIBLE VERROUILLÉE : " + _brain.focusScanner.currentTarget.name);
                    _brain.ChangeState(new PlayerStateFocus()); 
                    return; // On stoppe l'Update ici pour changer d'état proprement !
                }
                else
                {
                    _brain.Verbose("RIEN TROUVÉ : Recentrage de la caméra dans le dos du joueur !");
                    _brain.RecenterCamera();
                }
            }
            _wasFocusPressed = isFocusPressed;

            // --- NOUVEAU : BOUTON D'ACTION (Interaction ou Roulade) ---
            bool isActionPressed = _brain.InputHandler.ActionTriggered;

            // Si on appuie, qu'on est au sol, et qu'on ne roule pas déjà
            if (isActionPressed && !_wasActionPressed && _brain.controller.isGrounded && _rollTimer <= 0f)
            {
                // PRIORITÉ 1 : INTERACTION (Raycast)
                RaycastHit hit;
                // Le rayon part du torse (Vector3.up * 1f) vers l'avant sur 1.5 mètre
                Vector3 rayOrigin = _brain.transform.position + (Vector3.up * 1f);
                
                if (Physics.Raycast(rayOrigin, _brain.transform.forward, out hit, _brain.interactionDistance))
                {
                    // On vérifie si l'objet a l'interface IInteractable
                    if (hit.collider.TryGetComponent(out IInteractable interactableObj))
                    {
                        _brain.Verbose("ACTION : INTERACTION AVEC " + hit.collider.name);
                        interactableObj.Interact();
                        
                        _wasActionPressed = isActionPressed;
                        return; // TRÈS IMPORTANT : On arrête le code ici pour ne pas rouler !
                    }
                }

                // PRIORITÉ 2 & 3 : ROULADE OU ACTION SUR PLACE
                if (input != Vector2.zero)
                {
                    _brain.Verbose("ACTION : ROULADE !");
                    _brain.playerAnimator.SetTrigger("TrgRoll"); // Assure-toi d'avoir ce Trigger dans l'Animator !
                    
                    // On le pousse en avant
                    _currentMovement = _brain.transform.forward * _brain.rollForce; 
                    _rollTimer = _brain.rollDuration;
                }
                else
                {
                    _brain.Verbose("ACTION : ACTION SUR PLACE (Rien pour l'instant)");
                }
            }
            _wasActionPressed = isActionPressed;


            // --- LOGIQUE DE DÉPLACEMENT ---
            
            // NOUVEAU : On gère le timer de roulade
            if (_rollTimer > 0f)
            {
                // Pendant la roulade, on réduit le temps et on IGNORE la manette
                _rollTimer -= Time.deltaTime;
            }
            else
            {
                // Si on ne roule pas, on calcule la direction et la rotation normalement
                Vector3 camForward = _brain.mainCamera.transform.forward;
                Vector3 camRight = _brain.mainCamera.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 targetDirection = (camForward * input.y + camRight * input.x).normalized;
                Vector3 targetMovement = targetDirection * _brain.moveSpeed;

                _currentMovement = Vector3.SmoothDamp(_currentMovement, targetMovement, ref _velocityRef, _brain.moveSmoothTime);

                if (input != Vector2.zero && targetDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, targetRotation, _brain.rotationSpeed * Time.deltaTime);
                }
            }

            // --- GRAVITÉ (On la calcule toujours, même en roulant) ---
            if (_brain.controller.isGrounded)
            {
                if (_velocityY < 0f)
                {
                    _velocityY = -2f;
                }
            }
            else
            {
                _velocityY += Physics.gravity.y * 2f * Time.deltaTime;
            }

            // --- ASSEMBLAGE FINAL ---
            Vector3 finalMovement = _currentMovement + (Vector3.up * _velocityY);
            _brain.controller.Move(finalMovement * Time.deltaTime);

            // --- ANIMATION ---
            // On ne met à jour l'animation de course que si on n'est pas en train de rouler !
            if (_rollTimer <= 0f)
            {
                float currentHorizontalSpeed = new Vector3(_brain.controller.velocity.x, 0f, _brain.controller.velocity.z).magnitude;
                _brain.playerAnimator.SetFloat("Speed", currentHorizontalSpeed, 0.1f, Time.deltaTime);
            }
        }
        public override void FixedUpdate()
        {
            // Tout le code de déplacement est désormais dans Update() pour un feeling plus réactif
        }
    }
}
