public interface IRemoveChunk
{
    ChunkGameObjectPool GoPool { get; set; }
    bool CheckIfNeedsToBeRemoved(Chunk chunk);
    void RemoveChunk(Chunk chunk);
}
