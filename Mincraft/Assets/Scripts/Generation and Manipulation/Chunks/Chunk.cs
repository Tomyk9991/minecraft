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
using Random = System.Random;

namespace Core.Chunking
{
    public class Chunk : Context<Chunk>
    {
        private static float lightFalloff = 0.08f;

        public GameObject CurrentGO { get; set; }

        //Zwischen [(0, 0, 0) und (15, 15, 15)]
        public Int3 LocalPosition { get; set; }

        //Zwischen [(-int, -int, -int) und (int, int, int)]
        public Int3 GlobalPosition { get; set; }

        public bool AddedToDick { get; set; }
        public bool AddedToHash { get; set; }
        public ChunkState ChunkState { get; set; }

        private Int3 lowerBound, higherBound;

        private bool boundsCalculated = false;
        private Block[] blocks;
        private static int chunkSize;

        private Chunk[] chunkNeighbours;

        public ChunkCluster Cluster { get; set; }

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
        private static FastNoise noise;

        public Chunk()
        {
            chunkSize = ChunkSettings.ChunkSize;
            smoothness = ChunkSettings.NoiseSettings.Smoothness;
            steepness = ChunkSettings.NoiseSettings.Steepness;
            seed = ChunkSettings.NoiseSettings.Seed;

            treeGenerator = new OakTreeGenerator(new Int2(4, 6), new Int2(2, 4));

            blocks = new Block[chunkSize * chunkSize * chunkSize];
            chunkNeighbours = new Chunk[6];

            if (noise == null)
                noise = new FastNoise(this.seed);
        }

        public void AddBlock(Block block)
        {
            int index = GetFlattenIndex(block.Position.X, block.Position.Y, block.Position.Z);
            blocks[index] = block;
        }
        
        public void InvokeToRedraw()
        {
            throw new NotImplementedException();
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

        public void RemoveBlockAsGlobal(Int3 globalBlockPos)
        {
            int index = GetFlattenIndex(globalBlockPos.X - this.LocalPosition.X, globalBlockPos.Y - this.LocalPosition.Y,
                globalBlockPos.Z - this.LocalPosition.Z);
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
            return currentBlock.ID != (int) BlockUV.Air;
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
        /// Get block
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blockPos"></param>
        /// <returns></returns>
        private Block GetNeigbourAt(int index, Int3 blockPos)
        {
            Int3 local = blockPos;
            Int3 direction = directions[index];
            Chunk chunkNeighbour = chunkNeighbours[index];
            
            Block block = new Block
            {
                ID = (int)BlockUV.Stone
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

        private void GetChunkBoundsCalc()
        {
            boundsCalculated = true;
            int maxSize = ChunkSettings.ChunkSize;

            lowerBound = new Int3(Mathf.FloorToInt(LocalPosition.X),
                Mathf.FloorToInt(LocalPosition.Y),
                Mathf.FloorToInt(LocalPosition.Z));

            higherBound = new Int3(Mathf.FloorToInt(LocalPosition.X + maxSize),
                Mathf.FloorToInt(LocalPosition.Y + maxSize),
                Mathf.FloorToInt(LocalPosition.Z + maxSize));
        }

        private int GetFlattenIndex(int x, int y, int z)
            => x + chunkSize * (y + chunkSize * z);

        public override string ToString()
        {
            return LocalPosition.ToString();
        }

        public void CalculateNeighbours()
        {
            for (int i = 0; i < chunkNeighbours.Length; i++)
            {
                Int3 curRes = this.LocalPosition + directions[i];

                if (curRes.X >= 0 && curRes.X < 16 && curRes.Y >= 0 && curRes.Y < 16 && curRes.Z >= 0 && curRes.Z < 16)
                {
                    chunkNeighbours[i] = Cluster.GetChunk(this.LocalPosition + directions[i]);
                }
                else
                {
                    chunkNeighbours[i] =
                        ChunkClusterDictionary.GetChunkAt(this.GlobalPosition + (directions[i] * chunkSize));
                }
            }
        }
        public void CalculateNeighboursNew()
        {
            for (int i = 0; i < chunkNeighbours.Length; i++)
            {
                chunkNeighbours[i] = AvailableChunks.GetChunk(this.LocalPosition + directions[i]);
            }
        }

        private int GetFlattenIndex2D(int x, int y)
            => (2 * 2 + 1) * x + y;

        public Chunk[] GetNeigbours()
        {
            return chunkNeighbours;
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

            // Tree generation
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

                        if(lightRay > lightFalloff)
                            litVoxels.Enqueue(new Int3(x, y, z));
                    }
                }
            }

            while (litVoxels.Count > 0)
            {
                Int3 currentPosition = litVoxels.Dequeue();
                Block currentBlock = blocks[GetFlattenIndex(currentPosition.X, currentPosition.Y, currentPosition.Z)];

                for (int p = 0; p < 6; p++)
                {
                    Int3 neighbourPosition = currentPosition + directions[p];

                    if (neighbourPosition.X > 0 && neighbourPosition.X < 16 && 
                        neighbourPosition.Y > 0 && neighbourPosition.Y < 16 && 
                        neighbourPosition.Z > 0 && neighbourPosition.Z < 16)
                    {
                        Block neighbourBlock = blocks[GetFlattenIndex(neighbourPosition.X, neighbourPosition.Y, neighbourPosition.Z)];
                        if (neighbourBlock.GlobalLightPercent < currentBlock.GlobalLightPercent - lightFalloff)
                        {
                            float result = currentBlock.GlobalLightPercent - lightFalloff;
                            blocks[GetFlattenIndex(neighbourPosition.X, neighbourPosition.Y, neighbourPosition.Z)].GlobalLightPercent = result;
                            if (result > lightFalloff)
                            {
                                litVoxels.Enqueue(neighbourBlock.Position);
                            }
                        }
                    }
                }
            }
        }

        public void ChunkColumnGen(int x, int z, Biom biom)
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

            //Trees
            float treeZoomLevel = biom.treeZoomLevel;
            float treeValue = Mathf.PerlinNoise((x + seed) * treeZoomLevel, (z + seed) * treeZoomLevel);

            for (int y = this.GlobalPosition.Y; y < this.GlobalPosition.Y + 16; y++)
            {
                int caveChance = GetNoise(x, y, z, biom.caveFrequency, caveSize * 5);
                if (y <= lowerHeight && caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y, z - this.GlobalPosition.Z));
                    block.SetID((int) biom.lowerLayerBlock);
                    this.AddBlock(block);
                }
                else if (y <= midHeight && caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y, z - this.GlobalPosition.Z));
                    block.SetID((int)biom.midLayerBlock);
                    this.AddBlock(block);
                }
                else if (y <= topHeight && caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y, z - this.GlobalPosition.Z));
                    block.SetID((int) biom.topLayerBlock);
                    this.AddBlock(block);
                }

                if (treeValue > (1f - biom.treeProbability) && y == topHeight + 1)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y, z - this.GlobalPosition.Z));
                    //block.SetID((int)biom.treeTrunkBlock);
                    //this.AddBlock(block);

                    if (block.Position.Y + 1 < chunkSize && block.Position.X + 1 < chunkSize)
                    {
                        block.Position.Y += 1;
                        block.SetID((int)biom.treeLeafBlock);
                        this.AddBlock(block);

                        block.Position.X += 1;
                        block.SetID((int)biom.lowerLayerBlock);
                        this.AddBlock(block);
                    }
                }
                else //Air
                {
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
    }
}
