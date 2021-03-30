using Core.Builder;
using Core.Builder.Generation;
using Core.Chunks;
using Core.Math;

namespace Core.StructureGeneration
{
    public class TreeBuilder : StructureBuilder
    {
        public override void Build(Biom biom, Chunk callingChunk, in Int3 origin)
        {
            Block block = new Block();
            block.SetID(biom.treeTrunkBlock);
            Int3 pos = origin;

            //Trunk
            for (int j = 1; j < 5; j++)
            {
                pos.Y = origin.Y + j;
                base.AddBlockToChunk(callingChunk, block, pos);
            }

            Int3 leafOrigin = pos;
            //Leaves
            block.SetID(biom.treeLeafBlock);

            for (int y = -1; y < 3; y++)
            {
                pos.Y = leafOrigin.Y + y;
                for (int z = -2; z < 3; z++)
                {
                    pos.Z = leafOrigin.Z + z;
                    for (int x = -2; x < 3; x++)
                    {
                        pos.X = leafOrigin.X + x;
                        base.AddBlockToChunk(callingChunk, block, pos);
                    }
                }
            }
        }
    }
}