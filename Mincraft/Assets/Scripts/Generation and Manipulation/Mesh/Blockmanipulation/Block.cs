using System;
using Core.Math;

namespace Core.Builder
{
    [System.Serializable]
    public struct Block
    {
        private static Block emptyBlock = new Block { ID = BlockUV.None };
        public BlockUV ID { get; set; }// For UV-Setting

        public Block(BlockUV ID = BlockUV.None)
        {
            this.ID = ID;
        }

        public void SetID(BlockUV id)
        {
            this.ID = id;
        }

        public bool IsTransparent() => UVDictionary.IsTransparentID(this.ID);
        public bool IsSolid() => UVDictionary.IsSolidID(this.ID);
        public float MeshOffset() => UVDictionary.MeshOffsetID(this.ID);
        public float TransparentcyLevel() => UVDictionary.TransparencyLevelID(this.ID);

        public static Block Empty() => emptyBlock;

    }
}