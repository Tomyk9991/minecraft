using System.Collections.Generic;
using Core.Builder;
using Core.Builder.Generation;
using Core.Managers;
using Core.Math;
using Core.Saving;
using Core.Saving.Serializers;
using Core.StructureGeneration;
using UnityEngine;

namespace Core.Chunks
{
    public class Chunk : Context<Chunk>
    {
        //Lighting
        private static float lightFalloff = 0.08f;

        //References
        public GameObject CurrentGO { get; set; }
        public ChunkColumn ChunkColumn { get; set; }

        //Positions
        public Int3 LocalPosition { get; set; } //Zwischen [(0, 0, 0) und (15, 15, 15)]
        public Int3 GlobalPosition { get; set; } //Zwischen [(-int, -int, -int) und (int, int, int)]

        //Chunkdata
        private ExtendedArray3D<Block> blocks;
        private Block[] blockNeighbours;

        //Helper properties
        // public ChunkState ChunkState { get; set; }
        private static int chunkSize;
        
        //Structure Building
        private IStructureBuilder[] builders = {
            new TreeBuilder()
        };


        //Simplex noise
        private static float smoothness = 0;
        private static float steepness = 0;
        private static int seed = -1;

        private static Int3[] directions =
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
            steepness = WorldSettings.NoiseSettings.Steepness;
            seed = WorldSettings.NoiseSettings.Seed;

            blocks = new ExtendedArray3D<Block>(chunkSize, 1);
            //blocks = new Array3D<Block>(chunkSize);

            if (noise == null)
                noise = new FastNoise(seed);
            
            blockNeighbours = new Block[6];
        }

        public void AddBlock(Block block, Int3 pos) 
             => blocks[pos.X, pos.Y, pos.Z] = block;

        #region Context

        public override object Data()
        {
            return new ChunkSerializeHelper()
            {
                ChunkPosition = this.GlobalPosition,
                localBlocks = this.blocks.RawData,
                //local blocks auch!
                //Later here we will continue with biom settings
            };
        }

        public override Chunk Caster(object data)
        {
            var helper = (ChunkSerializeHelper) data;
            this.LocalPosition = helper.ChunkPosition;
            //globalblocks auch
            this.blocks = new ExtendedArray3D<Block>(helper.localBlocks);
            //this.blocks = new Array3D<Block>(helper.localBlocks);

            return this;
        }

        #endregion

        public ExtendedArray3D<Block> Blocks
        {
            get => blocks;
            set => blocks = value;
        }
        
        public bool IsNotEmpty(int x, int y, int z)
        {
            Block currentBlock = blocks[x, y, z];
            return currentBlock.ID != (int) BlockUV.Air;
        }

        /// <summary>
        /// Release the current gameObject corresponding to this chunk
        /// </summary>
        public void ReleaseGameObject()
        {
            ChunkGameObjectPool.Instance.SetGameObjectToUnused(this.CurrentGO);
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
            Int3 direction = directions[index];
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

        public void CalculateLight()
        {
            Queue<Int3> litVoxels = new Queue<Int3>();

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float lightRay = 1f;
                    for (int y = chunkSize - 1; y >= 0; y--)
                    {
                        Block b = blocks[x, y, z];
                        if (b.ID > 0 && b.TransparentcyLevel() < lightRay) //If it's not air
                            lightRay = b.TransparentcyLevel();

                        b.GlobalLightPercent = lightRay;
                        blocks[x, y, z] = b;

                        if (lightRay > lightFalloff)
                            litVoxels.Enqueue(new Int3(x, y, z));
                    }
                }
            }

            while (litVoxels.Count > 0)
            {
                Int3 currentPosition = litVoxels.Dequeue();
                Block currentBlock =
                    blocks[currentPosition.X, currentPosition.Y, currentPosition.Z];

                for (int p = 0; p < 6; p++)
                {
                    Int3 nPos = currentPosition + directions[p];
                    if (nPos.X > 0 && nPos.X < 16 &&
                        nPos.Y > 0 && nPos.Y < 16 && 
                        nPos.Z > 0 && nPos.Z < 16)
                    {
                        Block neighbourBlock = GetBlockNeigbourAt(p, currentPosition);
                        Int3 neighbourPos = currentPosition + directions[p];
                        
                        if (neighbourBlock.GlobalLightPercent < currentBlock.GlobalLightPercent - lightFalloff)
                        {
                            float result = currentBlock.GlobalLightPercent - lightFalloff;
                            var block = blocks[nPos.X, nPos.Y, nPos.Z];
                            block.GlobalLightPercent = result;
                            blocks[nPos.X, nPos.Y, nPos.Z] = block;
                            if (result > lightFalloff)
                            {
                                litVoxels.Enqueue(neighbourPos);
                            }
                        }
                    }
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
            
            
            for (int y = GlobalPosition.Y - 1; y < GlobalPosition.Y + chunkSize + 1; y++)
            {
                int caveChance = GetNoise(x, y, z, biom.caveFrequency, caveSize * 5);
                if (y <= lowerHeight && caveSize < caveChance)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    Block block = new Block();
                    block.SetID((short) biom.lowerLayerBlock);
                    this.AddBlock(block, pos);
                }
                else if (y <= midHeight && caveSize < caveChance)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    Block block = new Block();
                    block.SetID((short) biom.midLayerBlock);
                    this.AddBlock(block, pos);
                }
                else if (y <= topHeight && caveSize < caveChance)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    Block block = new Block();
                    block.SetID((short) biom.topLayerBlock);
                    this.AddBlock(block, pos);
                }
                else
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    Block block = new Block();
                    block.SetID((int) BlockUV.Air);
                    this.AddBlock(block, pos);
                }
            
                if (treeValue > (1f - biom.treeProbability) && y == topHeight + 1)
                {
                    Int3 pos = new Int3(x - GlobalPosition.X, y - GlobalPosition.Y, z - GlobalPosition.Z);
                    
                    if (!InChunkSpace(pos)) continue;
                    
                    Block block = new Block();
                    block.SetID((short) biom.treeTrunkBlock);
                    
                    AddBlock(block, pos);
                    Int3 origin = new Int3(pos.X, pos.Y, pos.Z);
                    
                    builders[0].StructureOrigin.Enqueue((origin, biom));
                }
            }
        }

        public void GenerateStructures()
        {
            var treeBuilder = builders[0];
            while (treeBuilder.StructureOrigin.Count > 0)
            {
                var tuple = treeBuilder.StructureOrigin.Dequeue();
                treeBuilder.Build(tuple.Biom, this, tuple.Origin);
            }
        }
        
        private bool InChunkSpace(Int3 pos)
            => pos.X >= 0 && pos.X < 16 &&
               pos.Y >= 0 && pos.Y < 16 &&
               pos.Z >= 0 && pos.Z < 16;

        public static int GetNoise(int x, int y, int z, float scale, int max)
            => Mathf.FloorToInt((noise.GetSimplexFractal(x * scale, y * scale, z * scale) + 1f) * (max / 2f));
        

        public void LoadChunk(Chunk chunk)
        {
            this.LocalPosition = chunk.LocalPosition;
            this.blocks = chunk.blocks;
        }

        public void SaveChunk()
            => GameManager.Instance.SavingJob.AddToSavingQueue(this);

        public override string ToString()
            => GlobalPosition.ToString();
        
        public Chunk ChunkNeighbour(Int3 dir)
            => ChunkBuffer.GetChunk(this.LocalPosition + dir);
    }
}