using System;
using System.Diagnostics;
using System.Linq;
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
        public bool CanFaceInDifferentDirections() => UVDictionary.CanFaceInDifferentDirections(this.ID);
        public bool Is3DSprite() => UVDictionary.RenderingTechnique(this.ID) == Builder.RenderingTechnique.Sprite3D;
        public RenderingTechnique RenderingTechnique() => UVDictionary.RenderingTechnique(this.ID);
        
        public float MeshOffset() => UVDictionary.MeshOffsetID(this.ID);
        public float TransparencyLevel() => UVDictionary.TransparencyLevelID(this.ID);

        public static Block Empty() => emptyBlock;

        public override string ToString()
        {
            return $"Block: {{ID: {this.ID.ToString()}}}";
        }
    }
    
    public enum BlockDirection : short
    {
        Forward = 0,
        Back = 1,
        Left = 4,
        Right = 5
    }
}