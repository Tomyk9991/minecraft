using Core.Builder;
using Core.UI;
using UnityEngine;

namespace Core.Player
{
    public class SelectedBlockVisualizer : MonoBehaviour, IConsoleToggle, IFullScreenUIToggle
    {
        [Header("References")]
        [SerializeField] Camera cameraRef = null;
        
        [SerializeField] private GameObject[] outlineGameGameObjects = null;
        [SerializeField] private LayerMask layerMask = 0;
        
        public float RaycastDistance
        {
            get => raycastDistance;
            set => raycastDistance = value;
        }
        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        [SerializeField] private float raycastDistance = 1000f;
        
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);
        private RaycastHit hitResult;

        private Vector3 blockPos;
        private Vector3 rayHit;
        private Ray ray;

        private void Update()
        {
            ray = cameraRef.ViewportPointToRay(centerScreenNormalized);
            if (Physics.Raycast(ray, out hitResult, RaycastDistance, layerMask))
            {
                for (int i = 0; i < outlineGameGameObjects.Length; i++)
                    outlineGameGameObjects[i].SetActive(true);

                rayHit = hitResult.point;

                blockPos = MeshBuilder.CenteredClickPositionOutSide(hitResult.point, hitResult.normal) - hitResult.normal;
                transform.position = blockPos + Vector3.one / 2;
            }
            else
            {
                for (int i = 0; i < outlineGameGameObjects.Length; i++)
                    outlineGameGameObjects[i].SetActive(false);
            }
        }
    }
}
