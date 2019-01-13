public interface ICreateChunk
{
    ChunkGameObjectPool GoPool { get; set; }
    IChunk GenerateChunk(Int3 pos);
}
