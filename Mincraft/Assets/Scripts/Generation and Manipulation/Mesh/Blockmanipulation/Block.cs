using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Block
{
    public Vector3Int Position;
    public UVSetter UVSetter { get; set; }

    public bool[] Neighbours
    {
        get
        {
            Vector3Int compare = BlockDictionary.NotFoundVectorBasis;
            return new[]
            {
                //Up
                BlockDictionary.GetValue(this.Position + Vector3Int.up) != compare,
                //Down
                BlockDictionary.GetValue(this.Position + Vector3Int.down) != compare,
                //Left
                BlockDictionary.GetValue(this.Position + Vector3Int.left) != compare,
                //Right
                BlockDictionary.GetValue(this.Position + Vector3Int.right) != compare,
                //Forward
                BlockDictionary.GetValue(this.Position + new Vector3Int(0, 0, 1)) != compare,
                //Back
                BlockDictionary.GetValue(this.Position + new Vector3Int(0, 0, -1)) != compare,
            };
        }
    }


    public Block(Vector3Int position, float tileX = 12, float tileY = 3)
    {
        UVSetter = new UVSetter(12, 3);
        this.Position = position;
    }
}