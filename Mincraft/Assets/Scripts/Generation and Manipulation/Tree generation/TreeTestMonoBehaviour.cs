using Extensions;

namespace Core.Debugging
{
    public class TreeTestMonoBehaviour : SingletonBehaviour<TreeTestMonoBehaviour>
    {
        //[SerializeField] private int amountChunksInDictionary;
        //[SerializeField] private GameObject prefab = null;

        //private ChunkJobManager chunkJobManager;
        //MeshModifier meshModifier = new MeshModifier();

        //private void Start()
        //{
        //    chunkJobManager = new ChunkJobManager(1);

        //    Chunk c = new Chunk(true)
        //    {
        //        Position = new Int3(0, 0, 0)
        //    };

        //    ChunkDictionary.Add(new Int3(0, 0, 0), c);
        //    c.AddedToDick = true;

        //    for (int x = 0; x < 16; x++)
        //    {
        //        for (int y = 0; y < 16; y++)
        //        {
        //            for (int z = 0; z < 16; z++)
        //            {
        //                if (y == 0)
        //                {
        //                    Block b = new Block(new Int3(x, y, z));
        //                    b.SetID((int)BlockUV.Grass);
        //                    c.AddBlock(b);
        //                }
        //                else
        //                {
        //                    Block b = new Block(new Int3(x, y, z));
        //                    b.SetID((int)BlockUV.Air);
        //                    c.AddBlock(b);
        //                }
        //            }
        //        }
        //    }

        //    TreeGenerator generator = new OakTreeGenerator(new Int2(5, 7), new Int2(5, 9));
        //    generator.Generate(c, 8, 0, 8);

        //    chunkJobManager.Add(new ChunkJob(c));
        //    chunkJobManager.Start();
        //}

        //private void Update()
        //{
        //    amountChunksInDictionary = ChunkDictionary.GetActiveChunks().Count;
        //    if (chunkJobManager.FinishedJobsCount > 0)
        //    {
        //        var chunkJob = chunkJobManager.DequeueFinishedJobs();
        //        GameObject go = Instantiate(prefab);
        //        meshModifier.SetMesh(go, chunkJob.MeshData, chunkJob.ColliderData);
        //        go.transform.position = chunkJob.Chunk.Position.ToVector3();
        //    }
        //}

        //private void OnDrawGizmosSelected()
        //{
        //    Gizmos.DrawWireCube(new Vector3(8, 8, 8), Vector3.one * 16);
        //}
    }
}
