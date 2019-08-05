[System.Serializable]
public struct Block
{
    public Int3 Position;
    public int ID { get; set; }// For UV-Setting

    public Block(Int3 position)
    {
        ID = -1;
        this.Position = position;
    }

    public void SetID(int id)
    {
        this.ID = id;
    }

    //TODO: Add this to the actual block object for performance
    public bool IsTransparent()
        => UVDictionary.IsTransparentID((BlockUV) this.ID);

    //TODO: Add this to the actual block object for performance
    public bool IsSolid()
        => UVDictionary.IsSolidID((BlockUV) this.ID);

    public static Block Empty()
    {
        return new Block(new Int3(0, 0, 0))
        {
            ID = 0
        };
    }
}