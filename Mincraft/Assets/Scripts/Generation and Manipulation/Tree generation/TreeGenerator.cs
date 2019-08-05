using UnityEngine;

public abstract class TreeGenerator
{
    /// <summary>
    /// Adds an block to the corresponding Chunk automatically
    /// </summary>
    /// <param name="block">The blockposition has to be in global space</param>
    /// <param name="chunk">The chunk, you're setting the initial tree plant</param>
    public void SetBlock(Block block, Chunk chunk)
    {
        //May cache the most common chunks to boost the performance
        //Chunk c = chunk.TryAddBlockFromGlobal(block, out Int3 chunkPosition);

        Chunk c = chunk.GetChunkFromGlobalBlock(block, out Int3 chunkPosition);

        if (c == null)
        {
            Chunk tempChunk = new Chunk
            {
                Position = chunkPosition
            };

            block.Position -= tempChunk.Position;
            tempChunk.AddBlock(block);

            tempChunk.AddedToDick = true;
        }
        else
        {
            block.Position -= chunkPosition;
            c.AddBlock(block);
        }

        //if (c != chunk)
        //{
        //    if (c == null)
        //    {
        //        if (!HashSetPositionChecker.Contains(chunkPosition))
        //        {
        //            Chunk tempChunk = new Chunk
        //            {
        //                Position = chunkPosition
        //            };

        //            block.Position -= tempChunk.Position;
        //            tempChunk.AddBlock(block);

        //            ChunkJob chunkJob = new ChunkJob(tempChunk, true);
        //            ChunkJobManager.ChunkJobManagerUpdaterInstance.Add(chunkJob);
        //        }
        //    }
        //    else
        //    {
        //        ChunkJob chunkJob = new ChunkJob(c, true);
        //        ChunkJobManager.ChunkJobManagerUpdaterInstance.Add(chunkJob);
        //    }
        //}
    }

    public virtual void Generate(Chunk c, int x, int y, int z) { }
}
