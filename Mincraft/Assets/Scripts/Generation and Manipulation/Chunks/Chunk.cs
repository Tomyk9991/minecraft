using System;
using System.Collections.Generic;
using UnityEngine;

using Core.Builder;
using Core.Builder.Generation;
using Core.Chunking.Threading;
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

        private Chunk[] chunkNeigbours;

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
            chunkNeigbours = new Chunk[6];

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
                ChunkPosition = this.LocalPosition,
                localBlocks = this.blocks,
                //global blocks auch!
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
                        block = blocks[
                            GetFlattenIndex(local.X + directions[0].X, local.Y + directions[0].Y,
                                local.Z + directions[0].Z)];
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
                        block = blocks[
                            GetFlattenIndex(local.X + directions[1].X, local.Y + directions[1].Y,
                                local.Z + directions[1].Z)];
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
                        block = blocks[
                            GetFlattenIndex(local.X + directions[2].X, local.Y + directions[2].Y,
                                local.Z + directions[2].Z)];
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
                        block = blocks[
                            GetFlattenIndex(local.X + directions[3].X, local.Y + directions[3].Y,
                                local.Z + directions[3].Z)];
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
                        block = blocks[
                            GetFlattenIndex(local.X + directions[4].X, local.Y + directions[4].Y,
                                local.Z + directions[4].Z)];
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
                        block = blocks[
                            GetFlattenIndex(local.X + directions[5].X, local.Y + directions[5].Y,
                                local.Z + directions[5].Z)];
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

            return !(block.ID == (int) uv || block.IsTransparent());
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

        public void CalculateNeigbours()
        {
            for (int i = 0; i < chunkNeigbours.Length; i++)
            {
                Int3 curRes = this.LocalPosition + directions[i];

                if (curRes.X >= 0 && curRes.X < 16 && curRes.Y >= 0 && curRes.Y < 16 && curRes.Z >= 0 && curRes.Z < 16)
                {
                    chunkNeigbours[i] = Cluster.GetChunk(this.LocalPosition + directions[i]);
                }
                else
                {
                    chunkNeigbours[i] =
                        ChunkClusterDictionary.GetChunkAt(this.GlobalPosition + (directions[i] * chunkSize));
                }
            }
        }

        public Chunk[] GetNeigbours()
        {
            return chunkNeigbours;
        }

        /// <summary>
        /// Generates the landscape for the chunk, based on it's position
        /// </summary>
        /// <returns>Returns a state, if the chunk has only air in it, or not</returns>
        public void GenerateBlocks()
        {
            ChunkGen();
        }

        private void ChunkGen()
        {
            for (int x = this.GlobalPosition.X; x < this.GlobalPosition.X + 16; x++)
            {
                for (int z = this.GlobalPosition.Z; z < this.GlobalPosition.Z + 16; z++)
                {
                    Biom biom = BiomFinder.Find(noise.GetCellular(x * smoothness, z * smoothness));
                    ChunkColumnGen(x, z, biom);
                }
            }
        }

        public void ChunkColumnGen(int x, int z, Biom biom)
        {
            //Mountains
            int lowerHeight = biom.lowerBaseHeight;

            lowerHeight += GetNoise(x, 0, z, biom.lowerMountainFrequency, biom.lowerMountainHeight);

            //Stones
            if (lowerHeight < biom.lowerMinHeight)
                lowerHeight = biom.lowerMinHeight;

            lowerHeight += GetNoise(x, 0, z, biom.lowerBaseNoise, biom.lowerBasNoisHeight);

            int midHeight = lowerHeight + biom.midBaseHeight;
            midHeight += GetNoise(x, 5, z, biom.midBaseNoise, biom.midBaseNoiseHeight);

            //Dirt
            int topHeight = midHeight + biom.topLayerBaseHeight;
            topHeight += GetNoise(x, 10, z, biom.topLayerNoise, biom.topLayerNoiseHeight);

            for (int y = this.GlobalPosition.Y; y < this.GlobalPosition.Y + 16; y++)
            {
                int caveChance = GetNoise(x, y, z, biom.caveFrequency, 200);
                if (y <= lowerHeight && biom.caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y, z - this.GlobalPosition.Z));
                    block.SetID((int) biom.lowerLayerBlock);
                    this.AddBlock(block);
                }
                else if (y <= midHeight && biom.caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y, z - this.GlobalPosition.Z));
                    block.SetID((int)biom.midLayerBlock);
                    this.AddBlock(block);
                }
                else if (y <= topHeight && biom.caveSize < caveChance)
                {
                    Block block = new Block(new Int3(x - this.GlobalPosition.X, y - this.GlobalPosition.Y, z - this.GlobalPosition.Z));
                    block.SetID((int) biom.topLayerBlock);
                    this.AddBlock(block);
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
