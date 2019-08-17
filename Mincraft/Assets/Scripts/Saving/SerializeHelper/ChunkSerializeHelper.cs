using Core.Builder;
using Core.Math;

namespace Core.Saving.Serializers
{
    [System.Serializable]
    public class ChunkSerializeHelper
    {
        public Int3 ChunkPosition;
        public Block[] localBlocks;
    }
}
