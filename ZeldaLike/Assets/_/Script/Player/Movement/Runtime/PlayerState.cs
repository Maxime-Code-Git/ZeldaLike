using UnityEngine;

namespace PlayerMovement
{
    public abstract class PlayerState 
    {
        public virtual void Enter(PlayerBrain brain){}
        public virtual void Exit(PlayerBrain brain){}
        public virtual void Update(){}
        public virtual void FixedUpdate(){}
    }
}
