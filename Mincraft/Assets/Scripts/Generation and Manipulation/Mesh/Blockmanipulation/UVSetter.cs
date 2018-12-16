using System;
using UnityEngine;

public class UVSetter
{
    public static readonly float pixelSize = 16;

    private Vector2[] uvs;

    private float umin;
    private float umax;
    private float vmin;
    private float vmax;

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
        float tilePerc = 1f / pixelSize;

        umin = tilePerc * tileX;
        umax = tilePerc * (tileX + 1f);
        vmin = tilePerc * tileY;
        vmax = tilePerc * (tileY + 1f);

        uvs = SetUVs.GetStandardUVs();
    }

    public Vector2[] GetUVs()
    {
        Vector2[] blocksUVs = new Vector2[24];

        for (int i = 0; i < uvs.Length; i++)
        {
            float x = Mathf.Approximately(uvs[i].x, 0f) ? umin : umax;
            float y = Mathf.Approximately(uvs[i].y, 0f) ? vmin : vmax;
            
            blocksUVs[i] = new Vector2(x, y);
        }

        return blocksUVs;
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
        }
    }
}
