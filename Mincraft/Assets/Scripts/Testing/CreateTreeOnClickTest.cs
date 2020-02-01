using Core.Builder;
using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;
using Core.Performance.Parallelisation;
using Core.Player;
using Core.StructureGeneration;
using UnityEngine;

namespace Testing
{
    public class CreateTreeOnClickTest : MonoBehaviour
    {
        private Int2 playerPos = Int2.Zero;
        private Vector3 chunkPos = Vector3.zero;

        private int drawDistance;
        private const int CHUNKSIZE = 0x10;
        
        private MeshJobManager _meshJobManager;

        private void Start()
        {
            PlayerMovementTracker.OnChunkPositionChanged += (x, y) =>
            {
                playerPos.X = x;
                playerPos.Y = y;
            };
            
            _meshJobManager = MeshJobManager.MeshJobManagerUpdaterInstance;
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                GameObject chunkGameObject = GameObject.Find("(-16, 0, -16)");
                
                Chunk clickedChunk = ChunkBuffer.GetChunkFromGlobal((int) chunkGameObject.transform.position.x,
                    (int) chunkGameObject.transform.position.y,
                    (int) chunkGameObject.transform.position.z, playerPos);

                TreeBuilder tb = new TreeBuilder(0, 0);
                IStructureBuilder treeBuilder = new TreeBuilder(0, 0);
                

                MeshJob job = null;
                foreach (Block block in treeBuilder.NextBlock())
                {   
                    clickedChunk.AddBlock(block);
                    
                    job = new MeshJob();
                    var chunkColumn = ChunkBuffer.GetChunkColumn(clickedChunk.LocalPosition.X, clickedChunk.LocalPosition.Z);
                    
                    job.CreateChunkFromExisting(clickedChunk, chunkColumn);
                }

                _meshJobManager.AddJob(job, JobPriority.High);
            }
        }
    }

}