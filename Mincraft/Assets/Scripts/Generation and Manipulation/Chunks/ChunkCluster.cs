using System;
using Core.Math;

namespace Core.Chunking
{
    public class ChunkCluster
    {
        public Int3 Position { get; set; }
        public int Count { get; private set; }
        public Chunk[] Chunks => _chunks;

        //Index x, y, z in local Space of chunks
        private Chunk[] _chunks;
        
        private static Int3 dimensionSize = new Int3(16, 16, 16);

        public ChunkCluster(Int3 dimensionSize)
        {
            dimensionSize = ChunkCluster.dimensionSize;
            _chunks = new Chunk[dimensionSize.X * dimensionSize.Y * dimensionSize.Z];
        }

        /// <summary>
        /// Expects a Chunk in local space relative to the ChunkCluster 
        /// </summary>
        /// <param name="chunk"></param>
        public void AddChunk(Chunk chunk)
        {
            Int3 pos = chunk.LocalPosition;

            if (pos.X >= 0 && pos.X < dimensionSize.X &&
                pos.Y >= 0 && pos.Y < dimensionSize.Y &&
                pos.Z >= 0 && pos.Z < dimensionSize.Z)
            {
                _chunks[FlattenIdx(pos.X, pos.Y, pos.Z)] = chunk;
                Count++;
            }
            else
                throw new IndexOutOfRangeException();
        }

        public void RemoveChunk(int x, int y, int z)
        {
            if (x >= 0 && x < dimensionSize.X &&
                y >= 0 && y < dimensionSize.Y &&
                z >= 0 && z < dimensionSize.Z)
            {
                _chunks[FlattenIdx(x, y, z)] = null;
                Count--;
            }
            else
                throw new IndexOutOfRangeException();
        }

        public void RemoveChunk(Int3 localChunkPosition)
            => RemoveChunk(localChunkPosition.X, localChunkPosition.Y, localChunkPosition.Z);

        public Chunk GetChunk(int x, int y, int z)
            => _chunks[FlattenIdx(x, y, z)];
        
        public Chunk GetChunk(Int3 pos)
            => GetChunk(pos.X, pos.Y, pos.Z);

        private int FlattenIdx(int x, int y, int z)
            => x + dimensionSize.X * (y + dimensionSize.Z * z);

        public override string ToString()
            => Position.ToString();
    }
}
