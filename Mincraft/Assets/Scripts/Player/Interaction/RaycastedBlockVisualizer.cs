using Core.Builder;
using Core.Chunks;
using Core.Math;
using Core.UI;
using Extensions;
using GateLogic.Impl;
using UnityEngine;
using Utilities;

namespace Core.Player
{
    public class RaycastedBlockVisualizer : MonoBehaviour, IConsoleToggle, IFullScreenUIToggle
    {
        [Header("References")]
        [SerializeField] Camera cameraRef = null;

        [SerializeField] private GameObject[] outlineGameGameObjects = null;
        [SerializeField] private LayerMask layerMask = 0;
        
        
        [Header("Colors")] 
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color digitalCircuitColor = Color.white;

        private bool isColoredNormal = true;
        private SpriteRenderer[] outlineSpriteRenderers = null;
        
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

        private PlaceBlockHelper placer;
        
        private void Start()
        {
            placer = new PlaceBlockHelper
            {
                currentBlock = {ID = BlockUV.None}
            };

            outlineSpriteRenderers = new SpriteRenderer[this.outlineGameGameObjects.Length];

            for (int i = 0; i < this.outlineSpriteRenderers.Length; i++)
                this.outlineSpriteRenderers[i] = this.outlineGameGameObjects[i].GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            ray = cameraRef.ViewportPointToRay(centerScreenNormalized);
            if (Physics.Raycast(ray, out hitResult, RaycastDistance, layerMask))
            {
                for (int i = 0; i < outlineGameGameObjects.Length; i++)
                    outlineGameGameObjects[i].SetActive(true);

                rayHit = hitResult.point;

                blockPos = MathHelper.CenteredClickPositionOutSide(hitResult.point, hitResult.normal) - hitResult.normal;
                transform.position = blockPos + Vector3.one / 2;

                if (!hitResult.transform.TryGetComponent(out ChunkReferenceHolder holder)) return;
                
                BlockUV hitBlock = ClickedBlock(hitResult, holder.Chunk);
                    
                this.isColoredNormal = hitBlock.IsCircuitBlock() 
                    ? ChangeOutlineColor(digitalCircuitColor, false, this.isColoredNormal) 
                    : ChangeOutlineColor(normalColor, true, this.isColoredNormal);
            }
            else
            {
                for (int i = 0; i < outlineGameGameObjects.Length; i++)
                    outlineGameGameObjects[i].SetActive(false);
            }
        }

        private bool ChangeOutlineColor(in Color targetColor, bool targetIsColorNormalState, bool currentIsColorNormalState)
        {
            if (targetIsColorNormalState != currentIsColorNormalState)
            {
                for (int i = 0; i < outlineGameGameObjects.Length; i++)
                    this.outlineSpriteRenderers[i].color = targetColor;
            }

            return targetIsColorNormalState;
        }
        
        
        private BlockUV ClickedBlock(in RaycastHit hit, Chunk currentChunk)
        {
            placer.latestGlobalClick = MathHelper.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal;

            placer.latestGlobalClickInt.X = (int) placer.latestGlobalClick.x;
            placer.latestGlobalClickInt.Y = (int) placer.latestGlobalClick.y;
            placer.latestGlobalClickInt.Z = (int) placer.latestGlobalClick.z;

            placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.LocalPosition);
                
            Block removedBlock;
            if (MathHelper.InChunkSpace(placer.LocalPosition))
            {
                removedBlock = placer.BlockAt(currentChunk, placer.LocalPosition);
            }
            else
            {
                placer.GetDirectionPlusOne(placer.LocalPosition, ref placer.dirPlusOne);
                currentChunk = currentChunk.ChunkNeighbour(placer.dirPlusOne);
                placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.LocalPosition);

                removedBlock = placer.BlockAt(currentChunk, placer.LocalPosition);
            }


            return removedBlock.ID;
        }
        
    }
}
