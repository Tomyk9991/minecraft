
using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Builder;
using Core.Builder.Generation;
using Core.Managers;
using Core.Math;
using Core.Saving;
using Core.Saving.Serializers;
using Test;

namespace Core.Chunking
{
    public class Chunk : Context<Chunk>
    {
        //Lighting
        private static float lightFalloff = 0.08f;

        //References
        public GameObject CurrentGO { get; set; }

        //Positions
        public Int3 LocalPosition { get; set; } //Zwischen [(0, 0, 0) und (15, 15, 15)]
        public Int3 GlobalPosition { get; set; } //Zwischen [(-int, -int, -int) und (int, int, int)]

        //Chunkdata
        private Block[] blocks;
        private List<TreeTrunk> trunkBuffer;

        //Helper properties
        public ChunkState ChunkState { get; set; }
        private static int chunkSize;


        //Simplex noise
        private float smoothness = 0;
        private float steepness = 0;
        private int seed = -1;

        private IStructureBuilder treeGenerator;

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
            trunkBuffer = new List<TreeTrunk>();
            chunkSize = 0x10;
            smoothness = WorldSettings.NoiseSettings.Smoothness;
            steepness = WorldSettings.NoiseSettings.Steepness;
            seed = WorldSettings.NoiseSettings.Seed;

            treeGenerator = new OakTreeGenerator(new Int2(4, 6), new Int2(2, 4));
            blocks = new Block[chunkSize * chunkSize * chunkSize];

            if (noise == null)
                noise = new FastNoise(this.seed);
        }

        public Chunk(string s)
        {
            chunkSize = 16;
            blocks = new Block[chunkSize * chunkSize * chunkSize];
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
                ChunkPosition = this.GlobalPosition,
                localBlocks = this.blocks,
                //local blocks auch!
                //Later here we will continue with biom settings
            };
        }

        public override Chunk Caster(object data)
        {
            var helper = (ChunkSerializeHelper) data;
            this.LocalPosition = helper.ChunkPosition;
            //globalblocks auch
            this.blocks = helper.localBlocks;

            return this;
        }

        #endregion


        public Block GetBlock(int x, int y, int z)
        {
            return blocks[GetFlattenIndex(x, y, z)];
        }

        public Block[] GetBlocks() => blocks;

        public bool IsNotEmpty(int x, int y, int z)
        {
            Block currentBlock = blocks[GetFlattenIndex(x, y, z)];
            return currentBlock.ID != (int) BlockUV.Air;
        }

        /// <summary>
        /// Release the current gameObject corresponding to this chunk
        /// </summary>
        public void ReleaseGameObject()
        {
            ChunkGameObjectPool.Instance.SetGameObjectToUnsed(this.CurrentGO);
            this.CurrentGO = null;
        }

        /// <summary>
        /// Get block
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blockPos"></param>
        /// <returns></returns>
        private Block GetNeigbourAt(int index, Int3 blockPos)
        {
            Int3 local = blockPos;
            Int3 direction = directions[index];
//            Chunk chunkNeighbour = chunkNeighbours[index];
            Chunk chunkNeighbour = CalculateNeighbour(index);

            Block block = new Block
            {
                ID = (int) BlockUV.Stone
            };


            switch (index)
            {
                case 0: //Forward
                {
                    if ((local + direction).Z < chunkSize)
                    {
                        block = blocks[
                            GetFlattenIndex(local.X + direction.X, local.Y + direction.Y,
                                local.Z + direction.Z)];
                    }
                    else
                    {
                        if (chunkNeighbour != null)
                        {
                            block = chunkNeighbour.GetBlock(blockPos.X, blockPos.Y, 0);
                        }
                    }

                    break;
                }
                case 1: //Back
                {
                    if ((local + direction).Z >= 0)
                    {
                        block = blocks[
                            GetFlattenIndex(local.X + direction.X, local.Y + direction.Y,
                                local.Z + direction.Z)];
                    }
                    else
                    {
                        if (chunkNeighbour != null)
                        {
                            block = chunkNeighbour.GetBlock(blockPos.X, blockPos.Y, chunkSize - 1);
                        }
                    }

                    break;
                }
                case 2: //Up
                {
                    if ((local + direction).Y < chunkSize)
                    {
                        block = blocks[
                            GetFlattenIndex(local.X + direction.X, local.Y + direction.Y,
                                local.Z + direction.Z)];
                    }
                    else
                    {
                        if (chunkNeighbour != null)
                        {
                            block = chunkNeighbour.GetBlock(blockPos.X, 0, blockPos.Z);
                        }
                    }

                    break;
                }
                case 3: //Down
                {
                    if ((local + direction).Y >= 0)
                    {
                        block = blocks[
                            GetFlattenIndex(local.X + direction.X, local.Y + direction.Y,
                                local.Z + direction.Z)];
                    }
                    else
                    {
                        if (chunkNeighbour != null)
                        {
                            block = chunkNeighbour.GetBlock(blockPos.X, chunkSize - 1, blockPos.Z);
                        }
                    }

                    break;
                }
                case 4: //Left
                {
                    if ((local + direction).X >= 0)
                    {
                        block = blocks[
                            GetFlattenIndex(local.X + direction.X, local.Y + direction.Y,
                                local.Z + direction.Z)];
                    }
                    else
                    {
                        if (chunkNeighbour != null)
                        {
                            block = chunkNeighbour.GetBlock(chunkSize - 1, blockPos.Y, blockPos.Z);
                        }
                    }

                    break;
                }
                case 5: //Right
                {
                    if ((local + direction).X < chunkSize)
                    {
                        block = blocks[
                            GetFlattenIndex(local.X + direction.X, local.Y + direction.Y,
                                local.Z + direction.Z)];
                    }
                    else
                    {
                        if (chunkNeighbour != null)
                        {
                            block = chunkNeighbour.GetBlock(0, blockPos.Y, blockPos.Z);
                        }
                    }

                    break;
                }
            }

            return block;
        }

        public Block[] Neighbours(Int3 blockPos)
        {
            return new[]
            {
                GetNeigbourAt(0, blockPos),
                GetNeigbourAt(1, blockPos),
                GetNeigbourAt(2, blockPos),
                GetNeigbourAt(3, blockPos),
                GetNeigbourAt(4, blockPos),
                GetNeigbourAt(5, blockPos)
            };
        }

        private int GetFlattenIndex(int x, int y, int z)
            => x + chunkSize * (y + chunkSize * z);

        public override string ToString()
        {
            return GlobalPosition.ToString();
        }

        public Chunk CalculateNeighbour(int i)
        {
            return ChunkBuffer.GetChunk(this.LocalPosition + directions[i]);
        }

        public Chunk[] GetNeigbours()
        {
            return new[]
            {
                CalculateNeighbour(0),
                CalculateNeighbour(1),
                CalculateNeighbour(2),
                CalculateNeighbour(3),
                CalculateNeighbour(4),
                CalculateNeighbour(5),
            };
            //return chunkNeighbours;
        }

        /// <summary>
        /// Generates the landscape for the chunk, based on it's position
        /// </summary>
        /// <returns>Returns a state, if the chunk has only air in it, or not</returns>
        public void GenerateBlocks()
        {
            //Terrain generation
            for (int x = this.GlobalPosition.X; x < this.GlobalPosition.X + 16; x++)
            {
                for (int z = this.GlobalPosition.Z; z < this.GlobalPosition.Z + 16; z++)
                {
                    Biom biom = BiomFinder.Find(noise.GetCellular(x * smoothness, z * smoothness));
                        ChunkColumnGen(x, z, biom);
                }
            }
        }

        public void GenerateAdditionalBlocks()
        {
            foreach (var treeTrunk in trunkBuffer)
                treeGenerator.Generate(this, treeTrunk.Biom, treeTrunk.X, treeTrunk.Y, treeTrunk.Z);
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
                        Block b = blocks[GetFlattenIndex(x, y, z)];
                        if (b.ID > 0 && b.TransparentcyLevel() < lightRay) //If it's not air
                            lightRay = b.TransparentcyLevel();

                        b.GlobalLightPercent = lightRay;
                        blocks[GetFlattenIndex(x, y, z)] = b;

                        if (lightRay > lightFalloff)
                            litVoxels.Enqueue(new Int3(x, y, z));
                    }
                }
            }

            while (litVoxels.Count > 0)
            {
                Int3 currentPosition = litVoxels.Dequeue();
                Block currentBlock =
                    blocks[GetFlattenIndex(currentPosition.X, currentPosition.Y, currentPosition.Z)];

                for (int p = 0; p < 6; p++)
                {
                    Int3 nPos = currentPosition + directions[p];
                    if (nPos.X > 0 && nPos.X < 16 &&
                        nPos.Y > 0 && nPos.Y < 16 &&
                        nPos.Z > 0 && nPos.Z < 16)
                    {
                        Block neighbourBlock = GetNeigbourAt(p, currentPosition);
                        if (neighbourBlock.GlobalLightPercent < currentBlock.GlobalLightPercent - lightFalloff)
                        {
                            float result = currentBlock.GlobalLightPercent - lightFalloff;
                            blocks[GetFlattenIndex(nPos.X, nPos.Y, nPos.Z)].GlobalLightPercent = result;
                            if (result > lightFalloff)
                            {
                                litVoxels.Enqueue(neighbourBlock.Position);
                            }
                        }
                    }
                }
            }
        }
        
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

            
            for (int y = this.GlobalPosition.Y; y < this.GlobalPosition.Y + 16; y++)
            {
                int caveChance = GetNoise(x, y, z, biom.caveFrequency, caveSize * 5);
                if (y <= lowerHeight && caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y,
                        z - this.GlobalPosition.Z));
                    block.SetID((int) biom.lowerLayerBlock);
                    this.AddBlock(block);
                }
                else if (y <= midHeight && caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y,
                        z - this.GlobalPosition.Z));
                    block.SetID((int) biom.midLayerBlock);
                    this.AddBlock(block);
                }
                else if (y <= topHeight && caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y,
                        z - this.GlobalPosition.Z));
                    block.SetID((int) biom.topLayerBlock);
                    this.AddBlock(block);
                }

                if (treeValue > (1f - biom.treeProbability) && y == topHeight + 1)
                {
                    trunkBuffer.Add(new TreeTrunk(biom, 
                        x - this.GlobalPosition.X,
                        y - this.GlobalPosition.Y,
                        z - this.GlobalPosition.Z));
                }
            }
        }

        public static int GetNoise(int x, int y, int z, float scale, int max)
            => Mathf.FloorToInt((noise.GetSimplexFractal(x * scale, y * scale, z * scale) + 1f) * (max / 2f));


        public void LoadChunk(Chunk chunk)
        {
            this.LocalPosition = chunk.LocalPosition;
            this.blocks = chunk.blocks;
        }

        public void SaveChunk()
        {
            GameManager.Instance.SavingJob.AddToSavingQueue(this);
        }

        private struct TreeTrunk
        {
            internal Biom Biom { get; set; }
            internal int X { get; set; }
            internal int Y { get; set; }
            internal int Z { get; set; }
            
            public TreeTrunk(Biom biom, int x, int y, int z)
            {
                Biom = biom;
                X = x;
                Y = y;
                Z = z;
            }
        }
    }
}