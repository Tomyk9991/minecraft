using System;

namespace Core.Builder
{
    [Serializable]
    public enum BlockUV : short
    {
        None = short.MinValue,
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
        Furnace = 10,
        RedFlower = 11,
        Lawn = 12
    }
}
