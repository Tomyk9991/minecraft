using System;
using Core.Math;

namespace Core.Builder
{
    [System.Serializable]
    public struct Block
    {
        private static Block emptyBlock = new Block { ID = -1500, GlobalLightPercent = 0f };
        public short ID { get; set; }// For UV-Setting
        public float GlobalLightPercent { get; set; }

        public Block(short ID = -1)
        {
            this.ID = ID;
            //this.Position = position;
            this.GlobalLightPercent = 0f;
        }

        public void SetID(short id)
        {
            this.ID = id;
        }

        //TODO: Add this to the actual block object for performance
        public bool IsTransparent()
            => UVDictionary.IsTransparentID((BlockUV)this.ID);

        //TODO: Add this to the actual block object for performance
        public bool IsSolid()
            => UVDictionary.IsSolidID((BlockUV)this.ID);

        public float MeshOffset()
            => UVDictionary.MeshOffsetID((BlockUV)this.ID);

        public static Block Empty() => emptyBlock;
//        public static Block Empty()
//        {
//            return new Block(new Int3(0, 0, 0))
//            {
//                ID = -15000
//            };
//        }

        public float TransparentcyLevel()
            => UVDictionary.TransparencyLevelID((BlockUV)this.ID);
    }
}