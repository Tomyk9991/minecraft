using UnityEngine;
using Core.Math;

namespace Core.Chunking
{
    public class ChunkCleanup
    {
        private Int3 drawDistance;
        private int chunkSize;

        public ChunkCleanup(Int3 drawDistance, int chunkSize = 16)
        {
            this.drawDistance = drawDistance;
            this.chunkSize = chunkSize;
        }
        
        private bool InsideDrawDistance(Int2 position, int xPos, int zPos)
        {
            return xPos - drawDistance.X <= position.X && xPos + drawDistance.X >= position.X &&
                   zPos - drawDistance.Z <= position.Y && zPos + drawDistance.Z >= position.Y;
        }
        
        public void CheckChunks(int xPlayerPos, int zPlayerPos)
        {
            var clusters = ChunkClusterDictionary.GetActiveChunkClusters();

            for (int i = 0; i < clusters.Count; i++)
            {
                Int3 clusterPos = clusters[i].Position;
                Chunk[] chunks = clusters[i].Chunks;


                for (int x = 0; x < chunkSize; x++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        Int2 globalChunkPos = new Int2(clusterPos.X + (x * chunkSize), clusterPos.Z + (z * chunkSize));
                        
                        if (!InsideDrawDistance(globalChunkPos, xPlayerPos, zPlayerPos))
                        {
                            HashSetPositionChecker.Remove(globalChunkPos);

                            for (int y = 0; y < chunkSize; y++)
                            {
                                Chunk chunk = chunks[FlattenIdx(x, y, z)];

                                if (chunk != null)
                                {
                                    chunk.ReleaseGameObject();
                                    clusters[i].RemoveChunk(x, y, z);
                                }
                            }
                        }
                    }
                }

                // Wenn das Cluster leer ist
                if (clusters[i].Count == 0)
                    ChunkClusterDictionary.Remove(clusters[i].Position);
            }
        }
        
        private int FlattenIdx(int x, int y, int z)
            => x + chunkSize * (y + chunkSize * z);
    }
}