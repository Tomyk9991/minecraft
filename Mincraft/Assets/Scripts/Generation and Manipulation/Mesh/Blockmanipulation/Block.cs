using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Block
{
    public Vector3Int Position;
    public UVSetter UVSetter { get; set; }
    
    
    public Block(Vector3Int position, float tileX = 12, float tileY = 3)
    {
        UVSetter = new UVSetter(12, 3);
        this.Position = position;
    }
}