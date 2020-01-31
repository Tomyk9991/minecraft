using Core.Chunking;
using Core.Math;
using Core.Player;
using UnityEngine;

namespace Testing
{
    public class CreateTreeOnClickTest : MonoBehaviour
    {
        private Int2 playerPos = Int2.Zero;
        private Vector3 chunkPos = Vector3.zero;
        
        private void Start()
        {
            PlayerMovementTracker.OnChunkPositionChanged += (x, y) =>
            {
                playerPos.X = x;
                playerPos.Y = y;
            };
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                Chunk c = ChunkBuffer.GetChunkFromGlobal(-16, 0, -16, playerPos);
//                Chunk c = ChunkBuffer.GetChunkFromGlobal(-96, 0, -96, playerPos);
//                StructureBuilder builder = new StructureBuilder();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
//            Gizmos.DrawCube(new Vector3(.5f, 7 + .5f, .5f),  Vector3.one);
            Gizmos.DrawCube(chunkPos - (Vector3.one * 8), Vector3.one * 16);
        }

        private class StructureBuilder
        {
            private Chunk initialChunk;
            private Int3 localBlockPosition;
            private Int3[] blockPositions;

            public StructureBuilder(Chunk initialChunk, Int3 localBlockPosition, Int3[] blockPositions)
            {
                this.initialChunk = initialChunk;
                this.localBlockPosition = localBlockPosition;
                this.blockPositions = blockPositions;
            }
        }
    }

}