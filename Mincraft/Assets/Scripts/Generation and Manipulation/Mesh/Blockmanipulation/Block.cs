using Unity.Mathematics;
using UnityEngine;

public class Block
{
    public Int3 Position;
    public int ID { get; set; }


    public Block(Int3 position)
    {
        ID = -1;
        this.Position = position;
    }

    public void SetID(int id)
    {
        this.ID = id;
    }
}