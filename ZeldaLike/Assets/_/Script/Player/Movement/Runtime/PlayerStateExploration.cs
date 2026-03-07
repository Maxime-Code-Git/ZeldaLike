using UnityEngine;

namespace PlayerMovement
{
    public class PlayerStateExploration : PlayerState
    {
        private PlayerBrain _brain;

        private float _velocityY = 0f;
        private Vector3 _currentMovement = Vector3.zero;
        private Vector3 _velocityRef = Vector3.zero;
        private bool _wasFocusPressed = false;

        public override void Enter(PlayerBrain brain)
        {
            base.Enter(brain);
            _brain = brain;
        }
        
        public override void Update()
        {
            // Handle input and movement logic for exploration state
            Vector2 input = _brain.InputHandler.MoveInput;

            if (input != Vector2.zero)
            {
                _brain.Verbose("Player is moving in exploration state with input: " + input);
            }

            bool isFocusPressed = _brain.InputHandler.FocusTriggered;

            // On ne déclenche l'action que sur la "première frame" où le bouton est appuyé
            if (isFocusPressed && !_wasFocusPressed)
            {
                // 1. On lance le scanner depuis la Caméra !
                if (_brain.focusScanner.TryFindTarget(_brain.mainCamera.transform))
                {
                    _brain.Verbose("CIBLE VERROUILLÉE : " + _brain.focusScanner.currentTarget.name);
                    
                    // PROCHAINE ÉTAPE : On changera d'état ici !
                    _brain.ChangeState(new PlayerStateFocus()); 
                }
                else
                {
                    // 2. Si on n'a rien trouvé, c'est un recentrage classique à la Zelda !
                    _brain.Verbose("RIEN TROUVÉ : Recentrage de la caméra dans le dos du joueur !");
                    _brain.RecenterCamera();
                }
            }
        }
        public override void FixedUpdate()
        {
            // 1. On récupère les inputs
            Vector2 input = _brain.InputHandler.MoveInput;

            // ATTENTION : On a supprimé le "if (input == Vector2.zero) return;" ici !

            // 2. Calcul de la direction voulue (Target Direction) par rapport à la caméra
            Vector3 camForward = _brain.mainCamera.transform.forward;
            Vector3 camRight = _brain.mainCamera.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 targetDirection = (camForward * input.y + camRight * input.x).normalized;
            
            // La vitesse cible (zéro si on lâche le stick, moveSpeed si on pousse)
            Vector3 targetMovement = targetDirection * _brain.moveSpeed;

            // 3. UPGRADE DU FEELING : On lisse le mouvement pour créer de l'inertie
            _currentMovement = Vector3.SmoothDamp(_currentMovement, targetMovement, ref _velocityRef, _brain.moveSmoothTime);

            // 4. GRAVITÉ
            if (_brain.controller.isGrounded)
            {
                if (_velocityY < 0f)
                {
                    _velocityY = -2f;
                }
            }
            else
            {
                _velocityY += Physics.gravity.y * 2f * Time.fixedDeltaTime;
            }

            // 5. Assemblage final
            Vector3 finalMovement = _currentMovement + (Vector3.up * _velocityY);
            _brain.controller.Move(finalMovement * Time.fixedDeltaTime);

            // 6. Rotation fluide (Seulement si on touche le stick, sinon on garde la dernière rotation)
            if (input != Vector2.zero && targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, targetRotation, _brain.rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
