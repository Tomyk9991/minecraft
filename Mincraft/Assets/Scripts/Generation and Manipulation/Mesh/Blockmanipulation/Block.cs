using Unity.Mathematics;
using UnityEngine;

public struct Block
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

    public static Block Empty()
    {
        return new Block(new Int3(0, 0, 0))
        {
            ID = -1
        };
    }
}