using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator
{
    private Int2 minMaxHeight;
    private Int2 minMaxVolume;
    private int maxWood = 0;

    /// <summary>Gets the Index of the last wood-block </summary>    
    public int MaxWood 
    { 
        get => maxWood;            
        private set
        {
            maxWood = value;
        }
    }

    public TreeGenerator(Int2 minMaxHeight, Int2 minMaxVolume)
    {
        this.minMaxHeight = minMaxHeight;
        this.minMaxVolume = minMaxVolume;
    }

    public Int3[] Generate(Int3 initialPosition)
    {
        //Ersetze Random durch Perlin noise, basierend auf der aktuellen Position, um es deterministisch zu gestalten#
        //Möglicherweise auch Random mit Seed
        int height = Random.Range(minMaxHeight.X, minMaxHeight.Y + 1);
        int volume = Random.Range(minMaxVolume.X, minMaxVolume.Y + 1);
        Int3 latestPos = initialPosition;
        var tR = new List<Int3>();

        volume += volume % 2 == 0 ? 1 : 0;

        for (int i = initialPosition.Y + 1; i < initialPosition.Y + height; i++)
        {
            
            Int3 blockPos = new Int3(initialPosition.X, i, initialPosition.Z);
            latestPos = blockPos;
            tR.Add(blockPos);
            maxWood++;
        }

        for (int i = 0; i < volume; i++)
        {
            for (int j = 0; j < volume; j++)
            {
                for (int k = 0; k < volume; k++)
                {
                    if(LeafSpawn(i, j, k, volume)) 
                    {
                        Int3 blockPos = new Int3(
                            latestPos.X - volume / 2 + i,
                            latestPos.Y - volume / 2 + j,
                            latestPos.Z - volume / 2 + k);
                        tR.Add(blockPos);
                    }
                }
            }
        }

        return tR.ToArray();
    }

    private bool LeafSpawn(int x, int y, int z, int volume) 
    {
        float a = Mathf.Abs(-volume / 2 + x);
        float b = Mathf.Abs(-volume / 2 + y);
        float c = Mathf.Abs(-volume / 2 + z);

        float distance = Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2) + Mathf.Pow(c, 2));
        return (distance < volume / 2 && Random.Range(0, volume) > distance);
    }
}
