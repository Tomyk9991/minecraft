using System;
using Core.Builder;
using Core.Builder.Generation;
using Core.Managers;
using Core.Math;
using Core.Saving;
using Core.StructureGeneration;
using UnityEngine;
using Utilities;

namespace Core.Chunks
{
    public class Chunk : SavingContext
    {
        //References
        public GameObject CurrentGO { get; set; }
        public ChunkColumn ChunkColumn { get; set; }

        //Positions
        public Int3 LocalPosition { get; set; } //Zwischen [(0, 0, 0) und (15, 15, 15)]
        public Int3 GlobalPosition { get; set; } //Zwischen [(-int, -int, -int) und (int, int, int)]

        //Chunkdata
        private ExtendedArray3D<Block> blocks;
        
        /// <summary>
        /// Determines, if the current chunk is generated or loaded from the disk
        /// </summary>
        private bool loaded = false;
        private Block[] blockNeighbours;
        private static Pool<ExtendedArray3D<Block>> BlockArrayPool;

        //Helper properties
        private static int chunkSize;

        //Structure Building
        private StructureBuilder[] Builders =>
        new StructureBuilder [] {
            new TreeBuilder(),
            new LawnRemover()
        };


        //Simplex noise
        private static float smoothness = 0;
        private static int seed = -1;

        public static Int3[] Directions { get; } =
        {
            Int3.Forward, // 0
            Int3.Back, // 1
            Int3.Up, // 2
            Int3.Down, // 3
            Int3.Left, // 4
            Int3.Right // 5
        };

        private static FastNoise noise;

        public Chunk()
        {
            chunkSize = 0x10;
            
            smoothness = WorldSettings.NoiseSettings.Smoothness;
            seed = WorldSettings.NoiseSettings.Seed;

            if (noise == null)
            {
                noise = new FastNoise(seed);
                BlockArrayPool = new Pool<ExtendedArray3D<Block>>(ChunkBuffer.ChunksInTotal, () => new ExtendedArray3D<Block>(chunkSize, 1));
            }
            
            blocks = BlockArrayPool.GetNext();
            blockNeighbours = new Block[6];
        }

        public void AddBlock(Block block, Int3 pos)
            => blocks[pos.X, pos.Y, pos.Z] = block;

        public void AddBlockPersistent(Block block, Int3 pos)
        {
            AddBlock(block, pos);
            ResourceIO.Save<Chunk>(this);
            // ChunkSavingManager.Save(this);
        }
        

        public ExtendedArray3D<Block> Blocks
        {
            get => blocks;
            set => blocks = value;
        }

        public bool IsNotEmpty(int x, int y, int z)
        {
            Block currentBlock = blocks[x, y, z];
            return currentBlock.ID != BlockUV.Air && currentBlock.ID != BlockUV.None && currentBlock.IsSolid();
        }

        /// <summary>
        /// Release the current gameObject corresponding to this chunk
        /// </summary>
        public void ReleaseGameObject()
        {
            ChunkGameObjectPool.Instance.SetGameObjectToUnused(this.CurrentGO);
            BlockArrayPool.Add(this.blocks);

            if (CurrentGO != null)
            {
                if (this.CurrentGO.TryGetComponent(out ChunkReferenceHolder holder))
                    holder.Chunk = null;
            }
            
            this.CurrentGO = null;
        }

        /// <summary>
        /// Calculates the neighbouring block
        /// </summary>
        /// <param name="index">Lookup index for the normalized 6 directions</param>
        /// <param name="local">Local block position</param>
        /// <returns>Block</returns>
        private Block GetBlockNeigbourAt(int index, Int3 local)
        {
            Int3 direction = Directions[index];
            Int3 newBlockPos = local + direction;

            return blocks[newBlockPos.X, newBlockPos.Y, newBlockPos.Z];
        }

        public Block[] GetBlockNeighbours(Int3 blockPos)
        {
            blockNeighbours[0] = GetBlockNeigbourAt(0, blockPos);
            blockNeighbours[1] = GetBlockNeigbourAt(1, blockPos);
            blockNeighbours[2] = GetBlockNeigbourAt(2, blockPos);
            blockNeighbours[3] = GetBlockNeigbourAt(3, blockPos);
            blockNeighbours[4] = GetBlockNeigbourAt(4, blockPos);
            blockNeighbours[5] = GetBlockNeigbourAt(5, blockPos);

            return blockNeighbours;
        }

        /// <summary>
        /// Generates the landscape for the chunk, based on it's position
        /// </summary>
        /// <returns>Returns a state, if the chunk has only air in it, or not</returns>
        public void GenerateBlocks()
        {
            if (ResourceIO.Load<Chunk>(new ChunkFileIdentifier(this.GlobalPosition), out OutputContext c))
            {
                ChunkData chunk = (ChunkData) c;
                
                this.blocks.RawData = chunk.Blocks;
                loaded = true;
                return;
            }
            
            //Terrain generation
            for (int x = this.GlobalPosition.X - 1; x < this.GlobalPosition.X + chunkSize + 1; x++)
            {
                for (int z = this.GlobalPosition.Z - 1; z < this.GlobalPosition.Z + chunkSize + 1; z++)
                {
                    Biom biom = BiomFinder.Find(noise.GetCellular(x * smoothness, z * smoothness));
                    ChunkColumnGen(x, z, biom);
                }
            }
        }

        // x z in global space
        private void ChunkColumnGen(int x, int z, Biom biom)
        {
            //Mountains
            int lowerHeight = biom.lowerBaseHeight;

            lowerHeight += GetNoise(x, 0, z, biom.lowerMountainFrequency, biom.lowerMountainHeight);

            int lowerMinHeight = biom.lowerMinHeight;

            //Stones
            if (lowerHeight < lowerMinHeight)
                lowerHeight = lowerMinHeight;

            lowerHeight += GetNoise(x, 0, z, biom.lowerBaseNoise, biom.lowerBasNoisHeight);

            int midHeight = lowerHeight + biom.midBaseHeight;
            midHeight += GetNoise(x, 5, z, biom.midBaseNoise, biom.midBaseNoiseHeight);

            //Dirt
            int topHeight = midHeight + biom.topLayerBaseHeight;

            topHeight += GetNoise(x, 10, z, biom.topLayerNoise, biom.topLayerNoiseHeight);

            int caveSize = biom.caveSize;
            
            float treeZoomLevel = biom.treeZoomLevel;
            float treeValue = Mathf.PerlinNoise((x + seed) * treeZoomLevel, (z + seed) * treeZoomLevel);

            float vegetationZoomLevel = biom.vegetationZoomLevel;
            float vegetationValue =
                Mathf.PerlinNoise((x + seed) * vegetationZoomLevel, (z + seed) * vegetationZoomLevel);
            

            for (int y = GlobalPosition.Y - 1; y < GlobalPosition.Y + chunkSize + 1; y++)
            {
                int caveChance = GetNoise(x, y, z, biom.caveFrequency, caveSize * 5);
                if (y <= lowerHeight && caveSize < caveChance)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    Block block = new Block();
                    block.SetID(biom.lowerLayerBlock);
                    this.AddBlock(block, pos);
                }
                else if (y <= midHeight && caveSize < caveChance)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    Block block = new Block();
                    block.SetID(biom.midLayerBlock);
                    this.AddBlock(block, pos);
                }
                else if (y <= topHeight && caveSize < caveChance)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    Block block = new Block();
                    block.SetID(biom.topLayerBlock);
                    this.AddBlock(block, pos);
                }
                else
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    Block block = new Block();
                    block.SetID((int) BlockUV.Air);
                    this.AddBlock(block, pos);
                }

                if (treeValue > 1f - biom.treeProbability && y == topHeight + 1)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);

                    //if (!MathHelper.InChunkSpace(pos)) continue;

                    Block block = new Block();
                    block.SetID(biom.treeTrunkBlock);

                    AddBlock(block, pos);
                    Int3 origin = new Int3(pos.X, pos.Y, pos.Z);
                    Builders[0].StructureOrigin.Enqueue((origin, biom));
                }

                if (vegetationValue > 1f - biom.vegetationProbability && y == topHeight + 1)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);

                    Block block = new Block();
                    block.SetID(biom.vegetationBlock);
                    
                    AddBlock(block, pos);
                    Int3 origin = new Int3(pos.X, pos.Y, pos.Z);
                    Builders[1].StructureOrigin.Enqueue((origin, biom));
                }
            }
        }

        public void GenerateStructures()
        {
            if (loaded == false)
            {
                foreach (var builder in Builders)
                {
                    while (builder.StructureOrigin.Count > 0)
                    {
                        (Int3 origin, Biom biom) = builder.StructureOrigin.Dequeue();
                        builder.Build(biom, this, origin);
                    }
                }
            }
        }

        public static int GetNoise(int x, int y, int z, float scale, int max)
            => Mathf.FloorToInt((noise.GetSimplexFractal(x * scale, y * scale, z * scale) + 1f) * (max / 2f));
        
        public override string ToString()
            => GlobalPosition.ToString();

        public Chunk ChunkNeighbour(Int3 dir)
            => ChunkBuffer.GetChunk(this.LocalPosition + dir);
    }
}