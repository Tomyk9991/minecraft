namespace Core.Chunks
{
    public enum ChunkState
    {
        /// <summary>
        /// Chunk is displayed with a gameobject
        /// </summary>
        Drawn,
        
        /// <summary>
        /// Chunk is Generated and not a Chunkjob anymore
        /// </summary>
        Generated,

        /// <summary>
        /// Chunk is displayed, but its not up to date anymore
        /// </summary>
        Dirty
    }
}
