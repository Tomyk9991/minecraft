using UnityEngine;

using Core.Builder;
using Core.UI.Console;

namespace Core.Player
{
    public class SelectedBlockVisualizer : MonoBehaviour, IConsoleToggle
    {
        [Header("References")]
        [SerializeField] Camera cameraRef = null;
        
        [SerializeField] private GameObject[] gameObjects = null;
        [SerializeField] private LayerMask layerMask = 0;
        public float RaycastHitable
        {
            get => raycastHitable;
            set => raycastHitable = value;
        }
        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        [SerializeField] private float raycastHitable = 1000f;
        
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);

        private void Update()
        {
            Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);
            if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable, layerMask))
            {
                for (int i = 0; i < gameObjects.Length; i++)
                    gameObjects[i].SetActive(true);

                Vector3 blockPos = MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal;
                transform.position = blockPos + Vector3.one / 2;
            }
            else
            {
                for (int i = 0; i < gameObjects.Length; i++)
                    gameObjects[i].SetActive(false);
            }
        }
    }
}
