using System;
using Core.Math;
using Core.Player;

namespace Core.Builder
{
    [System.Serializable]
    public struct Block
    {
        private static Block emptyBlock = new Block { ID = BlockUV.None };
        public BlockUV ID;// For UV-Setting
        public BlockDirection Direction;
        
        public Block(BlockUV ID = BlockUV.None)
        {
            this.ID = ID;
            this.Direction = BlockDirection.Forward;
        }

        public void SetID(BlockUV id)
        {
            this.ID = id;
        }

        public bool IsTransparent() => UVDictionary.IsTransparentID(this.ID);
        public bool IsSolid() => UVDictionary.IsSolidID(this.ID);
        public float MeshOffset() => UVDictionary.MeshOffsetID(this.ID);
        public float TransparencyLevel() => UVDictionary.TransparencyLevelID(this.ID);

        public static Block Empty() => emptyBlock;
    }
    
    public enum BlockDirection : short
    {
        Forward = 0,
        Back = 1,
        Left = 4,
        Right = 5
    }
}