namespace Core.Chunking
{
    public enum ChunkState
    {
        /// <summary>
        /// Chunk is created and added to the Dictionaries
        /// </summary>
        Created,
        
        /// <summary>
        /// Chunk is displayed with a gameobject
        /// </summary>
        Drawn,
        
        /// <summary>
        /// Chunk is Generated and not a Chunkjob anymore
        /// </summary>
        Generated
    }
}
