using UnityEngine;

namespace PlayerMovement
{
    public class PlayerFocus : MonoBehaviour
    {
        [Header("Focus Settings")]
        public float focusRange = 10f;
        public float focusRadius = 3f;
        public LayerMask focusLayerMask;

        public Transform currentTarget { get; private set; }

        public bool TryFindTarget(Transform cameraTransform)
        {
            RaycastHit hit;

            if (Physics.SphereCast(cameraTransform.position, focusRadius, cameraTransform.forward, out hit, focusRange, focusLayerMask))
            {
                currentTarget = hit.transform;
                return true;
            }
            currentTarget = null;
            return false;
        }

        public void ClearTarget()
        {
            currentTarget = null;
        }
        
    }
}
