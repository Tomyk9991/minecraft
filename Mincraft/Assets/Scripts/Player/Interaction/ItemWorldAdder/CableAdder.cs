using Core.Chunks;
using Extensions;
using Player.Interaction.ItemWorldAdder.Initializers;
using Player.Systems.Inventory;
using UnityEngine;

namespace Player.Interaction.ItemWorldAdder
{
    public class CableAdder : IItemWorldAdder
    {
        public Vector2Int ItemRange { get; } = new Vector2Int((int) CraftedItems.Cable, (int) CraftedItems.Cable);

        private GameObject lineRendererPrefab;
        private LineRenderer currentLineRenderer = null;
        private int handIndex = 0;

        public void Initialize(ScriptableObject initializer)
        {
            this.lineRendererPrefab = ((CableAdderInitializer) initializer).LineRendererPrefab;
        }
        
        public void OnPlace(int itemID, ChunkReferenceHolder holder, Ray ray, RaycastHit hit)
        {
            if (!this.currentLineRenderer)
            {
                GameObject go = GameObject.Instantiate(lineRendererPrefab);
                this.currentLineRenderer = go.GetComponent<LineRenderer>();
            }
            
            currentLineRenderer.AddPoint(hit.point + hit.normal * 0.05f);
            
            if (currentLineRenderer.positionCount == 0)
            {
                // handIndex = currentLineRenderer.AddPoint(handPosition.position);
            }
            // else
            // {
            //     currentLineRenderer.InsertPoint(currentLineRenderer.positionCount - 1,
            //         raycaster.RayCastHit.point);
            //     handIndex++;
            // }
            
            // if (this.currentLineRenderer)
            // {
            //     currentLineRenderer.SetPosition(handIndex, handPosition.position);
            // }
        }
    }
}