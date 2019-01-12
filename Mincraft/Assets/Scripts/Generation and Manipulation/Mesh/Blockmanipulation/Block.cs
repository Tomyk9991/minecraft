using UnityEngine;

public class Block
{
    public Vector3Int Position;
    public int ID { get; set; }


    public Block(Vector3Int position)
    {
        ID = -1;
        this.Position = position;
    }

    public void SetID(int id)
    {
        this.ID = id;
    }
}