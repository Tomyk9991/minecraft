using Core.Builder;
using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;
using UnityEditor;
using UnityEngine;

namespace Core.Testing
{
    public class SampleChunkGenerate : MonoBehaviour
    {
        [SerializeField] private BlockUV block = BlockUV.Air;
        [SerializeField] private GameObject meshToSave = null;
        
        
        private JobManager _manager;
        
        private void Start()
        {
            ChunkBuffer.UsingChunkBuffer = false;
            
            _manager = new JobManager(0, true);

            ChunkColumn column = new ChunkColumn(Int2.Zero, Int2.Zero, -32, 2);
            Chunk chunk = new Chunk
            {
                GlobalPosition = Int3.Zero,
                LocalPosition = Int3.Zero,
                ChunkColumn = column
            };

            
            Block b = new Block();
            Int3 pos = new Int3();
            for (int z = 0; z < 16; z++)
            {
                for (int x = 0; x < 16; x++)
                {
                    pos = new Int3(x, 0, z);
                    b.ID = (short) block;
                    chunk.AddBlock(b, pos);
                }
            }

            b.ID = (int) BlockUV.Stone;
            pos = new Int3(8, 1, 8);
            chunk.AddBlock(b, pos);
            pos = new Int3(8, 2, 8);
            chunk.AddBlock(b, pos);
            pos = new Int3(8, 3, 8);
            chunk.AddBlock(b, pos);
            pos = new Int3(9, 3, 8);
            chunk.AddBlock(b, pos);
            pos = new Int3(10, 3, 8);
            chunk.AddBlock(b, pos);
            pos = new Int3(10, 2, 8);
            chunk.AddBlock(b, pos);
            pos = new Int3(10, 1, 8);
            chunk.AddBlock(b, pos);

            b.ID = (short) BlockUV.Leaf;
            pos = new Int3(13, 1, 12);
            chunk.AddBlock(b, pos);

            MeshJob job = new MeshJob(chunk);
            _manager.Add(job);
        }

//        private void Update()
//        {
//            if (Input.GetKeyDown(KeyCode.Space))
//            {
//                AssetDatabase.CreateAsset(meshToSave.GetComponent<MeshFilter>().mesh, "Assets/JumpTest.mesh");
//                AssetDatabase.CreateAsset(meshToSave.GetComponent<MeshCollider>().sharedMesh, "Assets/JumpTestCollider.mesh");
//            }
//        }
    }
}
