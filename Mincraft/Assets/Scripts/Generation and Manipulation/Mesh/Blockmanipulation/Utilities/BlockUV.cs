using System;

namespace Core.Builder
{
    [Serializable]
    public enum BlockUV : short
    {
        None = -32768,
        Air = 0,
        Grass = 1,
        Stone = 2,
        Dirt = 3,
        Wood = 4,
        ProcessedWood = 5,
        Leaf = 6,
        Glass = 7,
        Sand = 8,
        Cactus = 9,
    }
}
