using UnityEngine;

namespace Interact
{
    public class TestSign : BBehaviour.Runtime.BBehaviour, IInteractable
    {
        public void Interact()
        {
            Verbose("You read the sign. It says: 'Welcome to the world of ZeldaLike!'");
        }
    }
}