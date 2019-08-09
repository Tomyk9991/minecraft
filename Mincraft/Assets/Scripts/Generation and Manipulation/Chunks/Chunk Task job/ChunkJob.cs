using UnityEngine;

public class ChunkJob
{
    public bool Completed { get; set; }
    public Chunk Chunk { get; set; }
    public MeshData MeshData { get; set; }
    public MeshData ColliderData { get; set; }

    public bool HasBlocks { get; set; }


    ///// <summary>
    ///// Creates a new ChunkJob and adds it to the 
    ///// </summary>
    ///// <param name="chunkPos"></param>
    ///// <param name="addToDictionary"></param>
    //public ChunkJob(Int3 chunkPos, bool addToDictionary = true)
    //{
    //    Chunk chunk = new Chunk();
    //    chunk.Position = chunkPos;
    //    HasBlocks = false;

    //    this.Chunk = chunk;
    //}



    /// <summary>
    /// Creates internally a new Chunk with an empty block array
    /// </summary>
    /// <param name="chunkPos"></param>
    /// <returns></returns>
    public Chunk CreateChunk(Int3 chunkPos)
    {
        Chunk chunk = new Chunk
        {
            Position = chunkPos
        };
        this.HasBlocks = false;

        this.Chunk = chunk;

        return this.Chunk;
    }

    /// <summary>
    /// Calculates the mesh, so it can be drawn
    /// </summary>
    /// <param name="chunk"></param>
    public void Redraw(Chunk chunk)
    {
        this.Chunk = chunk;
    }
    ///// <summary>
    ///// Create ChunkJobs with existing chunks and their block-information
    ///// </summary>
    ///// <param name="chunk">Chunk with existing chunk-information</param>
    ///// <param name="stillGenerate">Determines, if the standard block-generation should still run on top of the existing block-information</param>
    //public ChunkJob(Chunk chunk, bool stillGenerate = false)
    //{
    //    this.Chunk = chunk;
    //    HasBlocks = !stillGenerate;


    //    if (stillGenerate && !chunk.AddedToHash)
    //    {

    //    }
    //}
}
