public interface IRemoveChunk
{
    ChunkGameObjectPool GoPool { get; set; }
    bool CheckIfNeedsToBeRemoved(IChunk chunk);
    void RemoveChunk(IChunk chunk);
}
