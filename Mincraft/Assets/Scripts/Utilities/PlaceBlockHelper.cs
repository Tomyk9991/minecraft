using Core.Builder;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Math;
using UnityEngine;

namespace Utilities
{
    public class PlaceBlockHelper
    {
        public Int3 latestGlobalClickInt;
        public Int3 dir;
        public Int3 blockPos;
        public Block currentBlock;
        private ChunkJobManager chunkJobManager;
        
        public Int3 LocalPosition;
        public Int3 dirPlusOne;

        public Vector3 latestGlobalClick;

        private int chunkSize = 16;

        public PlaceBlockHelper()
        {
            chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;
        }

        public Block BlockAt(Chunk currentChunk, Int3 localPos)
        {
            return currentChunk.Blocks[localPos.X, localPos.Y, localPos.Z];
        }
        
        public Block HandleAddBlock(Chunk currentChunk, Int3 localPos, BlockUV acceptance = BlockUV.None)
        {
            Block removedBlock;

            removedBlock = currentChunk.Blocks[localPos.X, localPos.Y, localPos.Z];

            if (removedBlock.ID != acceptance && acceptance != BlockUV.None)
                return removedBlock;

            currentChunk.AddBlockPersistent(currentBlock, localPos);

            if (MathHelper.BorderBlock(localPos))
            {
                GetDirections(localPos, ref dir);

                if (dir.X != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.X == -1 ? Chunk.Directions[4] : Chunk.Directions[5]);
                    RelativeToLocalBlockMinusOneX(localPos, ref blockPos);

                    neighbourChunk.AddBlockPersistent(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk, ChunkJobPriority.High);
                }

                if (dir.Y != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.Y == -1 ? Chunk.Directions[3] : Chunk.Directions[2]);
                    RelativeToLocalBlockMinusOneY(localPos, ref blockPos);

                    neighbourChunk.AddBlockPersistent(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk, ChunkJobPriority.High);
                }

                if (dir.Z != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.Z == -1 ? Chunk.Directions[1] : Chunk.Directions[0]);
                    RelativeToLocalBlockMinusOneZ(localPos, ref blockPos);

                    neighbourChunk.AddBlockPersistent(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk, ChunkJobPriority.High);
                }
            }

            chunkJobManager.RecalculateChunk(currentChunk, ChunkJobPriority.High);

            return removedBlock;
        }

        public void GetDirectionPlusOne(in Int3 localPos, ref Int3 dir)
        {
            dir.X = 0;
            dir.Y = 0;
            dir.Z = 0;

            if (localPos.X < 0) dir.X = -1;
            if (localPos.X >= chunkSize) dir.X = 1;

            if (localPos.Y < 0) dir.Y = -1;
            if (localPos.Y >= chunkSize) dir.Y = 1;

            if (localPos.Z < 0) dir.Z = -1;
            if (localPos.Z >= chunkSize) dir.Z = 1;
        }

        public void GetDirections(in Int3 localPos, ref Int3 dir)
        {
            dir.X = 0;
            dir.Y = 0;
            dir.Z = 0;

            if (localPos.X == 0) dir.X = -1;
            if (localPos.X >= 15) dir.X = 1;

            if (localPos.Y == 0) dir.Y = -1;
            if (localPos.Y >= 15) dir.Y = 1;

            if (localPos.Z == 0) dir.Z = -1;
            if (localPos.Z >= 15) dir.Z = 1;
        }

        public void RelativeToLocalBlockMinusOneX(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;

            if (relativeBlockPos.X == 15) blockPos.X = -1;
            if (relativeBlockPos.X == 0) blockPos.X = 16;
        }

        public void RelativeToLocalBlockMinusOneY(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;

            if (relativeBlockPos.Y == 15) blockPos.Y = -1;
            if (relativeBlockPos.Y == 0) blockPos.Y = 16;
        }

        public void RelativeToLocalBlockMinusOneZ(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;

            if (relativeBlockPos.Z == 15) blockPos.Z = -1;
            if (relativeBlockPos.Z == 0) blockPos.Z = 16;
        }

        public void GlobalToRelativeBlock(Vector3 globalBlockPos, Int3 globalChunkPos, ref Int3 globalToRel)
        {
            globalToRel.X = (int) globalBlockPos.x - globalChunkPos.X;
            globalToRel.Y = (int) globalBlockPos.y - globalChunkPos.Y;
            globalToRel.Z = (int) globalBlockPos.z - globalChunkPos.Z;
        }
    }
}
