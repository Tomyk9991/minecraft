using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : Context<Chunk>
{
    //TODO, wenn der Chunk sich durch Simulation (z.B. Gras ist gewachsen) verändert, soll ebenfalls gespeichert werden
    public GameObject CurrentGO { get; set; }
    public Int3 Position { get; set; }
    public bool AddedToDick { get; set; }
    public bool AddedToHash { get; set; }


    private Int3 lowerBound, higherBound;

    private bool boundsCalculated = false;
    private Block[] blocks;
    private static int chunkSize;

    private Chunk[] chunkNeigbours;

    //Simplex noise
    private float smoothness = 0;
    private float steepness = 0;
    private int seed = -1;

    private TreeGenerator treeGenerator;

    private static Int3[] directions =
    {
        Int3.Forward, // 0
        Int3.Back, // 1
        Int3.Up, // 2
        Int3.Down, // 3
        Int3.Left, // 4
        Int3.Right // 5
    };


    public Chunk()
    {
        chunkSize = ChunkSettings.ChunkSize;
        smoothness = ChunkSettings.SimplexNoiseSettings.Smoothness;
        steepness = ChunkSettings.SimplexNoiseSettings.Steepness;
        seed = ChunkSettings.SimplexNoiseSettings.Seed;
        treeGenerator = new OakTreeGenerator(new Int2(4, 6), new Int2(2, 4));


        blocks = new Block[chunkSize * chunkSize * chunkSize];
        chunkNeigbours = new Chunk[6];
    }

    public Chunk(bool test) //Test
    {
        chunkSize = 16;
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
        var helper = (ChunkSerializeHelper)data;
        this.Position = helper.ChunkPosition;
        this.blocks = helper.localBlocks;

        return this;
    }
    #endregion

    /// <summary>
    /// Fügt den Block zum Chunk hinzu. Wenn dieser Block nicht mehr zum Chunk gehört, wird dieser zum nächsten
    /// benachbarten Chunk hinzugefügt
    /// </summary>
    /// <param name="block">Block, welcher eine globale Position hat</param>
    /// <returns>Gibt den Chunk zurück, wo der Block hinzugefügt wurde. Ist dieser Chunk nicht vorhanden, wird null zurückgegeben</returns>
    public Chunk GetChunkFromGlobalBlock(Block block, out Int3 chunkPosition)
    {
        if (this.Position.X - block.Position.X >= 0 && this.Position.X - block.Position.X < chunkSize
         && this.Position.Y - block.Position.Y >= 0 && this.Position.Y - block.Position.Y < chunkSize
         && this.Position.Z - block.Position.Z >= 0 && this.Position.Z - block.Position.Z < chunkSize)
        {
            chunkPosition = this.Position;
            return this;
        }

        int chunkX = Mathf.FloorToInt(block.Position.X / (float) chunkSize) * chunkSize;
        int chunkY = Mathf.FloorToInt(block.Position.Y / (float) chunkSize) * chunkSize;
        int chunkZ = Mathf.FloorToInt(block.Position.Z / (float) chunkSize) * chunkSize;

        chunkPosition = new Int3(chunkX, chunkY, chunkZ);

        return ChunkDictionary.GetValue(chunkPosition);
    }

    public void RemoveBlockAsGlobal(Int3 globalBlockPos)
    {
        int index = GetFlattenIndex(globalBlockPos.X - this.Position.X, globalBlockPos.Y - this.Position.Y, globalBlockPos.Z - this.Position.Z);
        blocks[index] = Block.Empty();
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
        return currentBlock.ID != (int)BlockUV.Air;
    }

    public (Int3 lowerBound, Int3 higherBound) GetChunkBounds()
    {
        if (!boundsCalculated)
            GetChunkBoundsCalc();

        return (lowerBound, higherBound);
    }

    /// <summary>
    /// Can only be called from the main-Thread
    /// </summary>
    public void ReleaseGameObject()
    {
        ChunkGameObjectPool.Instance.SetGameObjectToUnsed(this.CurrentGO);
        this.CurrentGO = null;
    }

    /// <summary>
    /// Returns Block's neigbours
    /// </summary>
    /// <param name="index"></param>
    /// <param name="blockPos">Rufe Block mit globaler Position auf</param>
    /// <returns></returns>
    private bool GetNeigbourAt(int index, Int3 blockPos, BlockUV uv)
    {
        Int3 local = blockPos;
        Block block = new Block
        {
            ID = (int) BlockUV.Air
        };

        switch (index)
        {
            case 0: //Forward
                {
                    if ((local + directions[0]).Z < chunkSize)
                    {
                        block = blocks[GetFlattenIndex(local.X + directions[0].X, local.Y + directions[0].Y, local.Z + directions[0].Z)];
                    }
                    else
                    {
                        if (chunkNeigbours[0] != null)
                        {
                            block = chunkNeigbours[0].GetBlock(blockPos.X, blockPos.Y, 0);
                        }
                    }
                    break;
                }
            case 1: //Back
                if ((local + directions[1]).Z >= 0)
                {
                    block = blocks[GetFlattenIndex(local.X + directions[1].X, local.Y + directions[1].Y, local.Z + directions[1].Z)];
                }
                else
                {

                    if (chunkNeigbours[1] != null)
                    {
                        block = chunkNeigbours[1].GetBlock(blockPos.X, blockPos.Y, chunkSize - 1);
                    }
                }
                break;
            case 2: //Up
                if ((local + directions[2]).Y < chunkSize)
                {
                    block = blocks[GetFlattenIndex(local.X + directions[2].X, local.Y + directions[2].Y, local.Z + directions[2].Z)];
                }
                else
                {
                    if (chunkNeigbours[2] != null)
                    {
                        block = chunkNeigbours[2].GetBlock(blockPos.X, 0, blockPos.Z);
                    }
                }
                break;
            case 3: //Down
                if ((local + directions[3]).Y >= 0)
                {
                    block = blocks[GetFlattenIndex(local.X + directions[3].X, local.Y + directions[3].Y, local.Z + directions[3].Z)];
                }
                else
                {
                    if (chunkNeigbours[3] != null)
                    {
                        block = chunkNeigbours[3].GetBlock(blockPos.X, chunkSize - 1, blockPos.Z);
                    }
                }
                break;
            case 4: //Left
                if ((local + directions[4]).X >= 0)
                {
                    block = blocks[GetFlattenIndex(local.X + directions[4].X, local.Y + directions[4].Y, local.Z + directions[4].Z)];
                }
                else
                {
                    if (chunkNeigbours[4] != null)
                    {
                        block = chunkNeigbours[4].GetBlock(chunkSize - 1, blockPos.Y, blockPos.Z);
                    }
                }
                break;
            case 5: //Right
                if ((local + directions[5]).X < chunkSize)
                {
                    block = blocks[GetFlattenIndex(local.X + directions[5].X, local.Y + directions[5].Y, local.Z + directions[5].Z)];
                }
                else
                {
                    if (chunkNeigbours[5] != null)
                    {
                        block = chunkNeigbours[5].GetBlock(0, blockPos.Y, blockPos.Z);
                    }
                }
                break;
        }

        return !(block.ID == (int)uv || block.IsTransparent());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blockPos">Rufe Block mit globaler Position auf</param>
    /// <returns></returns>
    public bool[] BoolNeigbours(Int3 blockPos)
    {
        return new[]
        {
            GetNeigbourAt(0, blockPos, BlockUV.Air),
            GetNeigbourAt(1, blockPos, BlockUV.Air),
            GetNeigbourAt(2, blockPos, BlockUV.Air),
            GetNeigbourAt(3, blockPos, BlockUV.Air),
            GetNeigbourAt(4, blockPos, BlockUV.Air),
            GetNeigbourAt(5, blockPos, BlockUV.Air)
        };
    }

    private void GetChunkBoundsCalc()
    {
        boundsCalculated = true;
        int maxSize = ChunkSettings.ChunkSize;

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

    public void CalculateNeigbours()
    {
        for (int i = 0; i < chunkNeigbours.Length; i++)
        {
            chunkNeigbours[i] = ChunkDictionary.GetValue(this.Position + (directions[i] * chunkSize));
        }
    }

    public Chunk[] GetNeigbours()
    {
        return chunkNeigbours;
    }
    
        public void GenerateBlocks() // TODO: Make this based on biom. Maybe virtual or pass in IBiom?
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float height = OctavePerlin((x + this.Position.X + seed) * smoothness, (z + this.Position.Z + seed) * smoothness, 8, 0.5f) * steepness;

                    if (y + this.Position.Y < height - 1)
                    {
                        Block b = new Block(new Int3(x, y, z));
                        b.SetID((int)BlockUV.Dirt);
                        this.AddBlock(b);
                    }
                    else if (y + this.Position.Y < height)
                    {
                        Block b = new Block(new Int3(x, y, z));
                        b.SetID((int)BlockUV.Grass);
                        this.AddBlock(b);
                    }
                    else if (y + this.Position.Y == (int)height + 1)
                    {
                        float TESTZOOMLEVEL = 1.08f;

                        float result = Mathf.PerlinNoise((x + this.Position.X + seed) * TESTZOOMLEVEL, (z + this.Position.Z + seed) * TESTZOOMLEVEL);
                        if (result > 0.89f) //93
                        {
                            //Irgendwas bei der Tree-Generation kaputt. Rechnet in zwei verschiende Chunks 2 mal das selbe Resultat
                            List<ChunkJob> jobs = treeGenerator.Generate(this, x, y, z);

                            for (int i = 0; i < jobs.Count; i++)
                            {
//                                Debug.Log(jobs[i].Chunk.Position);
                                ChunkJobManager.ChunkJobManagerUpdaterInstance.Add(jobs[i]);
                            }
                            
                        }
                    }
                }
            }
        }
    }

    private float OctavePerlin(float x, float y, int octaves, float persistence)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }
    

    public void LoadChunk(Chunk chunk)
    {
        this.Position = chunk.Position;
        this.blocks = chunk.blocks;
    }

    public void SaveChunk()
    {
        GameManager.Instance.SavingJob.AddToSavingQueue(this);
    }
}
 