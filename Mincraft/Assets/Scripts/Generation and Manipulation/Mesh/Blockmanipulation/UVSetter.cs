using System;
using UnityEngine;

public class UVSetter
{
    public static readonly float pixelSize = 16;

    private float tileX = 0f;
    private float tileY = 0f;

    public float TileX
    {
        get => tileX;
        set => tileX = value;
    }

    public float TileY
    {
        get => tileY;
        set => tileY = value;
    }


    public UVSetter(float tileX, float tileY)
    {
        this.TileX = tileX;
        this.TileY = tileY;
    }

    public void SetBlockUV(BlockUV block)
    {
        switch (block)
        {
            case BlockUV.Grass:
                this.TileX = 12;
                this.TileY = 3;
                break;
            case BlockUV.Stone:
                this.TileX = 1;
                this.TileY = 15;
                break;
            case BlockUV.Dirt:
                this.TileX = 2;
                this.TileY = 15;
                break;
            case BlockUV.Destruction1:
                this.TileX = 0;
                this.TileY = 0;
                break;
        }
    }
}
