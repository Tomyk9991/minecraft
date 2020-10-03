using System;
using Core.Builder;

namespace Core.Saving
{
    [Serializable]
    public class ChunkData
    {
        public Block[] Blocks;

        public ChunkData(Block[] blocks)
        {
            this.Blocks = blocks;
        }
    }
}
