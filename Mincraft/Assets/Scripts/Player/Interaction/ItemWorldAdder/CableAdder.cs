using Core.Builder;
using Core.Chunks;
using Core.Math;
using Extensions;
using GateLogic;
using Player.Interaction.ItemWorldAdder.Initializers;
using Player.Systems.Inventory;
using UnityEngine;
using Utilities;

namespace Player.Interaction.ItemWorldAdder
{
    public class CableAdder : IItemWorldAdder
    {
        //TODO Add effect, if going in ui menu, make FOV higher, for "nice" effect
        public Vector2Int ItemRange { get; } = new Vector2Int((int) CraftedItems.Cable, (int) CraftedItems.Cable);

        private GameObject lineRendererPrefab;
        private LineRenderer currentLineRenderer = null;
        private PlaceBlockHelper placer;

        private IGate g1;
        private Chunk gateOwner;
        private IGate g2;
        
        public void Initialize(ScriptableObject initializer)
        {
            placer = new PlaceBlockHelper
            {
                currentBlock = {ID = BlockUV.None}
            };
            
            this.lineRendererPrefab = ((CableAdderInitializer) initializer).LineRendererPrefab;
        }
        
        public void OnPlace(int itemID, ChunkReferenceHolder holder, Ray ray, RaycastHit hit)
        {
            if (!this.currentLineRenderer)
            {
                GameObject go = GameObject.Instantiate(lineRendererPrefab);
                this.currentLineRenderer = go.GetComponent<LineRenderer>();
            }

            Block hitBlock = ClickedBlock(hit, holder.Chunk);
            
            if (hitBlock.IsCircuitBlock())
            {
                int index = currentLineRenderer.AddPoint(hit.point + hit.normal * 0.05f);

                if (index == 0)
                {
                    g1 = hitBlock.ToGate();
                    gateOwner = holder.Chunk;
                }
                
                if (index == 1)
                {
                    g2 = hitBlock.ToGate();
                    gateOwner.DigitalCircuitManager.AddConnection(g1, g2);
                }
            }
        }

        private Block ClickedBlock(in RaycastHit hit, Chunk currentChunk)
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


            return removedBlock;
        }
    }
}