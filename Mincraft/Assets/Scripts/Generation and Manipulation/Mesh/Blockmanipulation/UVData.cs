using System;
using UnityEngine;

public struct UVData
{
    private const float epsilon = 0.005f;
    public float TileX;
    public float TileY;
    public float SizeX;
    public float SizeY;
    
    public UVData(float tileX, float tileY, float sizeX, float sizeY)
    {
        this.TileX = tileX + epsilon;
        this.TileY = tileY + epsilon;
        this.SizeX = sizeX - epsilon * 2f;
        this.SizeY = sizeY - epsilon * 2f;
    }
}
