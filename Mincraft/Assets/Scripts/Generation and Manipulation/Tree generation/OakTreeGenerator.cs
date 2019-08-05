using UnityEngine;

public class OakTreeGenerator : TreeGenerator
{
    private Int2 minMaxHeight;
    private Int2 minMaxVolume;

    public OakTreeGenerator(Int2 minMaxHeight, Int2 minMaxVolume)
    {
        this.minMaxHeight = minMaxHeight;
        this.minMaxVolume = minMaxVolume;
    }

    public override void Generate(Chunk chunk, int x, int y, int z)
    {
        x += chunk.Position.X;
        y += chunk.Position.Y;
        z += chunk.Position.Z;

        int height = (int)Map(Mathf.PerlinNoise(x * 0.9f, z * 0.9f), 0f, 1f, minMaxHeight.X + 1, minMaxHeight.Y);
        int volume = (int)Map(Mathf.PerlinNoise(x * 0.9f, z * 0.9f), 0f, 1f, minMaxVolume.X + 1, minMaxVolume.Y);

        Block block = new Block();

        volume += volume % 2 == 0 ? 1 : 0;

        block.ID = (int) BlockUV.Wood;
        for (int i = y; i < y + height; i++)
        {
            block.Position = new Int3(x, i, z);
            base.SetBlock(block, chunk);
        }

        Int3 latestPos = new Int3(x, y + height - 1, z);

        block.ID = (int)BlockUV.Leaf;
        for (int i = 0; i < volume; i++)
        {
            for (int j = 0; j < volume; j++)
            {
                for (int k = 0; k < volume; k++)
                {
                    if (LeafSpawn(i, j, k, volume))
                    {
                        block.Position = new Int3(latestPos.X - volume / 2 + i, latestPos.Y - volume / 2 + j, latestPos.Z - volume / 2 + k);
                        base.SetBlock(block, chunk);
                    }
                }
            }
        }
    }

    private bool LeafSpawn(int x, int y, int z, int volume) 
    {
        float a = Mathf.Abs(-volume / 2 + x);
        float b = Mathf.Abs(-volume / 2 + y);
        float c = Mathf.Abs(-volume / 2 + z);

        float distance = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2) + Mathf.Pow(c, 2));
        return (distance < volume / 2f && (Mathf.PerlinNoise(x * 0.9f, y * 0.9f) * volume) > distance);
    }

    private float Map(float n, float start1, float stop1, float start2, float stop2)
        => ((n - start1) / (stop1 - start1)) * (stop2 - start2) + start2;
}
