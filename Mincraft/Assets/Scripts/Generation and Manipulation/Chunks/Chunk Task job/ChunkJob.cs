public class ChunkJob
{
    public bool Completed { get; set; }
    public Chunk Chunk { get; set; }
    public MeshData MeshData { get; set; }
    public MeshData ColliderData { get; set; }

    public bool HasBlocks { get; private set; }

    public ChunkJob(Int3 chunkPos, bool addToDictionary = true)
    {
        Chunk chunk = new Chunk();
        chunk.Position = chunkPos;
        HasBlocks = false;

        this.Chunk = chunk;

        if (addToDictionary)
        {
            HashSetPositionChecker.Add(chunk.Position);
        }
    }

    public ChunkJob(Chunk chunk)
    {
        this.Chunk = chunk;
        HasBlocks = true;
    }
}
