using UnityEngine;

namespace Utilities
{
    public class CenterMouseRaycaster
    {
        public RaycastHit RayCastHit;
        private static readonly Vector3 centerScreenNormalized = new Vector3(.5f, .5f, .0f);

        private Camera cameraRef;
        private float rayDistance;
        private int hitmask;
        
        public CenterMouseRaycaster(Camera cameraRef, float rayDistance, int hitmask)
        {
            this.cameraRef = cameraRef;
            this.rayDistance = rayDistance;
            this.hitmask = hitmask;
        }

        public bool Raycast()
        {
            Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);
            return Physics.Raycast(ray, out RayCastHit, rayDistance, hitmask);
        }
    }
}