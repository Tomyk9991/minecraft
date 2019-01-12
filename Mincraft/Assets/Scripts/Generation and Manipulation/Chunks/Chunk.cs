using UnityEngine;

public class Chunk : IChunk
{
    public GameObject CurrentGO { get; set; }
    public Int3 Position { get; set; }

    public Int3 lowerBound, higherBound;
    
    private bool boundsCalculated = false;
    private Block[] blocks;
    private static int chunkSize;

    private IChunk[] chunkNeigbours;
    
    private static Int3[] directions = 
    {
        Int3.Forward,
        Int3.Back,
        Int3.Up,
        Int3.Down,
        Int3.Left,
        Int3.Right
    };
    
    #region Interface implementation

    public Chunk()
    {
        chunkSize = ChunkGenerator.GetMaxSize;
        blocks = new Block[chunkSize * chunkSize * chunkSize];
        chunkNeigbours = new IChunk[6];
    }

    public void AddBlock(Block block)
    {
        Int3 local = GetLocalPosition(block.Position);
        int index = GetFlattenIndex(local);
        blocks[index] = block;
    }

    public IChunk TryAddBlock(Block block, Vector3 normal)
    {
        Int3 local = GetLocalPosition(block.Position);
        int index = GetFlattenIndex(local);

        if (index >= chunkSize * chunkSize * chunkSize || index < 0)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                if (directions[i] == Int3.ToInt3(normal))
                    return chunkNeigbours[i];
            }

            return null;
        }

        AddBlock(block);
        return this;
    }

    public void RemoveBlock(Int3 blockPos)
    {
        int index = GetFlattenIndex(GetLocalPosition(blockPos));
        blocks[index] = null;
    }
    
    public int BlockCount() => blocks.Length;

    public void GenerateChunk()
    {
    }
    
    public Block GetBlock(Int3 position)
    {
        int index = GetFlattenIndex(GetLocalPosition(position));
        return blocks[index];
    }
    
    public Block[] GetBlocks() => blocks;
    
    public (Int3 lowerBound, Int3 higherBound) GetChunkBounds()
    {
        if (!boundsCalculated)
            GetChunkBoundsCalc();

        return (lowerBound, higherBound);
    }
    
    public void CalculateNeigbours()
    {
        for (int i = 0; i < chunkNeigbours.Length; i++)
        {
            chunkNeigbours[i] = ChunkDictionary.GetValue(this.Position + (directions[i] * chunkSize));
        }
    }

    /// <summary>
    /// Returns Block's neigbours
    /// </summary>
    /// <param name="index"></param>
    /// <param name="blockPos">Rufe Block mit globaler Position auf</param>
    /// <returns></returns>
    public bool GetNeigbourAt(int index, Int3 blockPos)
    {
        Int3 local = GetLocalPosition(blockPos);
        switch (index)
        {
            case 0:
            {
                if ((local + directions[0]).Z < chunkSize)
                {
                    return blocks[GetFlattenIndex(local + directions[0])] != null;
                }
                else
                {
                    if (chunkNeigbours[0] != null)
                    {
                        return chunkNeigbours[0].GetBlock(new Int3(blockPos.X, blockPos.Y, chunkNeigbours[0].Position.Z)) != null;
                    }
                }
                break;
            }
            case 1:
                if ((local + directions[1]).Z >= 0)
                {
                    return blocks[GetFlattenIndex(local + directions[1])] != null;
                }
                else
                {
                    if (chunkNeigbours[1] != null)
                    {
                        return chunkNeigbours[1].GetBlock(new Int3(blockPos.X, blockPos.Y, chunkNeigbours[1].Position.Z + chunkSize - 1)) != null;
                    }
                }
                break;
            case 2:
                if ((local + directions[2]).Y < chunkSize)
                {
                    return blocks[GetFlattenIndex(local + directions[2])] != null;
                }
                else
                {
                    if (chunkNeigbours[2] != null)
                    {
                        return chunkNeigbours[2].GetBlock(new Int3(blockPos.X, chunkNeigbours[2].Position.Y, blockPos.Z)) != null;
                    }
                }
                break;
            case 3:
                if ((local + directions[3]).Y >= 0)
                {
                    return blocks[GetFlattenIndex(local + directions[3])] != null;
                }
                else
                {
                    if (chunkNeigbours[3] != null)
                    {
                        return chunkNeigbours[3].GetBlock(new Int3(blockPos.X, chunkNeigbours[3].Position.Y + chunkSize - 1, blockPos.Z)) != null;
                    }
                }
                break;
            case 4:
                if ((local + directions[4]).X >= 0)
                {
                    return blocks[GetFlattenIndex(local + directions[4])] != null;
                }
                else
                {
                    if (chunkNeigbours[4] != null)
                    {
                        return chunkNeigbours[4].GetBlock(new Int3(chunkNeigbours[4].Position.X + chunkSize - 1, blockPos.Y, blockPos.Z)) != null;
                    }
                }
                break;
            case 5:
                if ((local + directions[5]).X < chunkSize)
                {
                    return blocks[GetFlattenIndex(local + directions[5])] != null;
                }
                else
                {
                    if (chunkNeigbours[5] != null)
                    {
                        return chunkNeigbours[5].GetBlock(new Int3(chunkNeigbours[5].Position.X, blockPos.Y, blockPos.Z)) != null;
                    }
                }
                break;
        }
        
        return false;
    }
    
    #endregion
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="blockPos">Rufe Block mit globaler Position auf</param>
    /// <returns></returns>
    public bool[] BoolNeigbours(Int3 blockPos)
    {
        return new []
        {
            GetNeigbourAt(0, blockPos),
            GetNeigbourAt(1, blockPos),
            GetNeigbourAt(2, blockPos),
            GetNeigbourAt(3, blockPos),
            GetNeigbourAt(4, blockPos),
            GetNeigbourAt(5, blockPos)
        };
    }
    
    private void GetChunkBoundsCalc()
    {
        boundsCalculated = true;
        int maxSize = ChunkGenerator.GetMaxSize;
        int half = maxSize / 2;

        lowerBound = new Int3(Mathf.FloorToInt(Position.X),
            Mathf.FloorToInt(Position.Y),
            Mathf.FloorToInt(Position.Z));

        higherBound = new Int3(Mathf.FloorToInt(Position.X + maxSize),
            Mathf.FloorToInt(Position.Y + maxSize),
            Mathf.FloorToInt(Position.Z + maxSize));
    }

    private int GetFlattenIndex(Int3 localPosition)
        => localPosition.X + chunkSize * (localPosition.Y + chunkSize * localPosition.Z);

    //TODO Kann sein, dass die Origin des Chunks nicht unten Links, sondern
    //in der Mitte ist
    private Int3 GetLocalPosition(Int3 globalPosition)
        => globalPosition - this.Position;
}
