using System.Collections.Generic;

public static class UVDictionary
{
    private static UVData[] notFoundData = new UVData[6]
    {
        new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
        new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
        new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
        new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
        new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
        new UVData(7f / 16f, 1f / 16f, 1f / 16f, 1f / 16f),
    };
    private static Dictionary<BlockUV, UVData[]> dictionary = new Dictionary<BlockUV, UVData[]>();

    public static void Init()
    {
        dictionary.Add(BlockUV.Grass, new []
        {
            new UVData(3f / 16f, 15f / 16f, 1f / 16f, 1f / 16f), //Forward
            new UVData(3f / 16f, 15f / 16f, 1f / 16f, 1f / 16f), //Back
            new UVData(12f / 16f, 3f / 16f, 1f / 16f, 1f / 16f), //Up
            new UVData(2f / 16f, 15f / 16f, 1f / 16f, 1f / 16f), //Down
            new UVData(3f / 16f, 15f / 16f, 1f / 16f, 1f / 16f), //left
            new UVData(3f / 16f, 15f / 16f, 1f / 16f, 1f / 16f) //right
        });
        dictionary.Add(BlockUV.Stone, new []
        {
            new UVData(1f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(1f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(1f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(1f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(1f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(1f / 16f, 15f / 16f, 1f / 16f, 1f / 16f)
        });
        dictionary.Add(BlockUV.Dirt, new []
        {
            new UVData(2f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(2f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(2f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(2f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(2f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
            new UVData(2f / 16f, 15f / 16f, 1f / 16f, 1f / 16f),
        });
    }

    public static void Clear()
    {
        dictionary.Clear();
    }

    public static UVData[] GetValue(BlockUV id)
    {
        if (dictionary.TryGetValue(id, out UVData[] result))
        {
            return result;
        }

        return null;
    }
}