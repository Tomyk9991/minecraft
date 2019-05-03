using UnityEngine;

public class Chunk : Context<Chunk>
{
    public GameObject CurrentGO { get; set; }
    public Int3 Position { get; set; }

    private Int3 lowerBound, higherBound;
    
    private bool boundsCalculated = false;
    private Block[] blocks;
    private static int chunkSize;

    private Chunk[] chunkNeigbours;

    //Simplex noise
    private float smoothness = 0;
    private float steepness = 0;
    private int seed = -1;



    private static Int3[] directions = 
    {
        Int3.Forward, // 0
        Int3.Back, // 1
        Int3.Up, // 2
        Int3.Down, // 3
        Int3.Left, // 4
        Int3.Right // 5
    };
    
    #region Interface implementation

    public Chunk()
    {
        chunkSize = ChunkSettings.GetMaxSize;
        smoothness = ChunkSettings.SimplexNoiseSettings.Smoothness;
        steepness = ChunkSettings.SimplexNoiseSettings.Steepness;
        seed = ChunkSettings.SimplexNoiseSettings.Seed;

        
        blocks = new Block[chunkSize * chunkSize * chunkSize];
        chunkNeigbours = new Chunk[6];
    }

    public void AddBlock(Block block)
    {
        int index = GetFlattenIndex(block.Position.X, block.Position.Y, block.Position.Z);
        blocks[index] = block;
    }

    #region Context
    public override object Data()
    {
        return new ChunkSerializeHelper()
        {
            ChunkPosition = this.Position,
            localBlocks = this.blocks
            //Later here we will continue with biom settings
        };
    }

    public override Chunk Caster(object data)
    {
        var helper = (ChunkSerializeHelper) data;
        this.Position = helper.ChunkPosition;
        this.blocks = helper.localBlocks;

        return this;
    }    
    #endregion

    public Chunk TryAddBlockFromGlobal(Block block)
    {
        Chunk chunkReturn = this;

        Int3 local = new Int3(block.Position.X - this.Position.X, block.Position.Y - this.Position.Y, block.Position.Z - this.Position.Z);

        if (local.AnyAttribute(dim => dim > chunkSize - 1, out int dimension))
        {
            local = new Int3(local.X % chunkSize, local.Y % chunkSize, local.Z % chunkSize);
            switch (dimension)
            {
                case 0:
                    chunkReturn = ChunkDictionary.GetValue(this.Position + directions[5] * chunkSize);
                    break;
                case 1:
                    chunkReturn = ChunkDictionary.GetValue(this.Position + directions[2] * chunkSize);
                    break;
                case 2:
                    chunkReturn = ChunkDictionary.GetValue(this.Position + directions[0] * chunkSize);
                    break;
            }
        }
        else if (local.AnyAttribute(dim => dim < 0, out dimension))
        {
            switch (dimension)
            {
                case 0: //X
                    local = new Int3(chunkSize + local.X, local.Y, local.Z);
                    chunkReturn = ChunkDictionary.GetValue(this.Position + directions[4] * chunkSize);
                    break;
                case 1: //Y
                    local = new Int3(local.X, chunkSize + local.Y, local.Z);
                    chunkReturn = ChunkDictionary.GetValue(this.Position + directions[3] * chunkSize);
                    break;
                case 2: //Z
                    local = new Int3(local.X, local.Y, chunkSize + local.Z);
                    chunkReturn = ChunkDictionary.GetValue(this.Position + directions[1] * chunkSize);
                    break;
            }
        }

        if (chunkReturn == null)
        {
            return null;
        }

        chunkReturn.AddBlock(new Block(local) {ID = block.ID} );
        return chunkReturn;
    }

    public void RemoveBlockAsGlobal(Int3 globalBlockPos)
    {
        int index = GetFlattenIndex(globalBlockPos.X - this.Position.X, globalBlockPos.Y - this.Position.Y, globalBlockPos.Z - this.Position.Z);
        blocks[index] = Block.Empty();
    }

    private void RemoveBlock(Int3 blockPos)
    {
        int index = GetFlattenIndex(blockPos.X, blockPos.Y, blockPos.Z);
        blocks[index] = Block.Empty();
    }
    
    public int BlockCount() => blocks.Length;

    public void GenerateBlocks() // TODO: Make this based on biom. Maybe virtual or pass in IBiom?
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float yResult = y + this.Position.Y;
                for (int z = 0; z < chunkSize; z++)
                {
                    float result = SimplexNoise.Generate((x + this.Position.X + seed) * smoothness, (yResult + seed) * smoothness, (z + this.Position.Z + seed) * smoothness);

                    result *= steepness;

                    if (result > 0f) //Block or not block
                    {
                        Block b = new Block(new Int3(x, y, z));
                        b.SetID((int)BlockUV.Stone);
                        this.AddBlock(b);
                    }
                    else
                    {
                        Block b = new Block(new Int3(x, y, z));
                        this.AddBlock(b);
                    }
                }
            }
        }
    }

    public Block GetBlock(int x, int y, int z)
    {
        return blocks[GetFlattenIndex(x, y, z)];
    }
    
    public Block[] GetBlocks() => blocks;
    public bool IsNotEmpty(int x, int y, int z)
    {
        //Maybe no GetLocalPosition
        Block currentBlock = blocks[GetFlattenIndex(x, y, z)];
        return currentBlock.ID != -1;
    }

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

    public void ReleaseGameObject()
    {
        ChunkGameObjectPool.Instance.SetGameObjectToUnsed(CurrentGO);
        GameObject g = CurrentGO;
        CurrentGO = null;

        UnityEngine.Object.Destroy(g);
    }

    /// <summary>
    /// Returns Block's neigbours
    /// </summary>
    /// <param name="index"></param>
    /// <param name="blockPos">Rufe Block mit globaler Position auf</param>
    /// <returns></returns>
    public bool GetNeigbourAt(int index, Int3 blockPos)
    {
        Int3 local = blockPos;
            switch (index)
            {
                case 0:
                    {
                        if ((local + directions[0]).Z < chunkSize)
                        {
                            return blocks[GetFlattenIndex(local.X + directions[0].X, local.Y + directions[0].Y, local.Z + directions[0].Z)].ID != -1;
                        }
                        else
                        {
                            if (chunkNeigbours[0] != null)
                            {
                                return chunkNeigbours[0].GetBlock(blockPos.X, blockPos.Y, 0).ID != -1;
                            }
                        }
                        break;
                    }
                case 1:
                    if ((local + directions[1]).Z >= 0)
                    {
                        return blocks[GetFlattenIndex(local.X + directions[1].X, local.Y + directions[1].Y, local.Z + directions[1].Z)].ID != -1;
                    }
                    else
                    {
                        if (chunkNeigbours[1] != null)
                        {
                            return chunkNeigbours[1].GetBlock(blockPos.X, blockPos.Y, chunkSize - 1).ID != -1;
                        }
                    }
                    break;
                case 2:
                    if ((local + directions[2]).Y < chunkSize)
                    {
                        return blocks[GetFlattenIndex(local.X + directions[2].X, local.Y + directions[2].Y, local.Z + directions[2].Z)].ID != -1;
                    }
                    else
                    {
                        if (chunkNeigbours[2] != null)
                        {
                            return chunkNeigbours[2].GetBlock(blockPos.X, 0, blockPos.Z).ID != -1;
                        }
                    }
                    break;
                case 3:
                    if ((local + directions[3]).Y >= 0)
                    {
                        return blocks[GetFlattenIndex(local.X + directions[3].X, local.Y + directions[3].Y, local.Z + directions[3].Z)].ID != -1;
                    }
                    else
                    {
                        if (chunkNeigbours[3] != null)
                        {
                            return chunkNeigbours[3].GetBlock(blockPos.X, chunkSize - 1, blockPos.Z).ID != -1;
                        }
                    }
                    break;
                case 4:
                    if ((local + directions[4]).X >= 0)
                    {
                        return blocks[GetFlattenIndex(local.X + directions[4].X, local.Y + directions[4].Y, local.Z + directions[4].Z)].ID != -1;
                    }
                    else
                    {
                        if (chunkNeigbours[4] != null)
                        {
                            return chunkNeigbours[4].GetBlock(chunkSize - 1, blockPos.Y, blockPos.Z).ID != -1;
                        }
                    }
                    break;
                case 5:
                    if ((local + directions[5]).X < chunkSize)
                    {
                        return blocks[GetFlattenIndex(local.X + directions[5].X, local.Y + directions[5].Y, local.Z + directions[5].Z)].ID != -1;
                    }
                    else
                    {
                        if (chunkNeigbours[5] != null)
                        {
                            return chunkNeigbours[5].GetBlock(0, blockPos.Y, blockPos.Z).ID != -1;
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
        int maxSize = ChunkSettings.GetMaxSize;
        int half = maxSize / 2;

        lowerBound = new Int3(Mathf.FloorToInt(Position.X),
            Mathf.FloorToInt(Position.Y),
            Mathf.FloorToInt(Position.Z));

        higherBound = new Int3(Mathf.FloorToInt(Position.X + maxSize),
            Mathf.FloorToInt(Position.Y + maxSize),
            Mathf.FloorToInt(Position.Z + maxSize));
    }

    private int GetFlattenIndex(int x, int y, int z)
        => x + chunkSize * (y + chunkSize * z);

    public override string ToString()
    {
        return Position.ToString();
    }

    public void SetNeighbour(Chunk neighbour, int neighbourIndex)
    {
        this.chunkNeigbours[neighbourIndex] = neighbour;

        if (neighbour != null)
        {
            neighbour.chunkNeigbours[GetOppositeIndex(neighbourIndex)] = this;
        }
    }

    private int GetOppositeIndex(int index)
    {
        //Wenn index gerade, dann addiere zum index 1
        //Wenn index ungerade, subtrahiere index um 1
        if (index % 2 == 0) // Gerade
        {
            return index++;
        }
        else
        {
            return index--;
        }
    }
}
