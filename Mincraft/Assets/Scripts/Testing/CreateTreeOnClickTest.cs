using Core.Chunking;
using Core.Math;
using UnityEngine;

namespace Testing
{
    public class CreateTreeOnClickTest : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
//                Chunk c = ChunkBuffer.GetChunkFromGlobal(0, 0, 0, playerPos);
//                StructureBuilder builder = new StructureBuilder();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(new Vector3(.5f, 7 + .5f, .5f),  Vector3.one);
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