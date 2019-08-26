using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Core.Math;

namespace Core.Chunking
{
    public static class ChunkClusterDictionary
    {
        private static ConcurrentDictionary<Int3, ChunkCluster> dictionary = new ConcurrentDictionary<Int3, ChunkCluster>();
        private static object _mutex = new object();

        private static ChunkCluster _clusterCache;

        /// <summary>
        /// Adds a Chunk to an internal Chunk Dictionary
        /// </summary>
        /// <param name="key">Chunkcluster position</param>
        /// <param name="value">The chunk must have a local space positioning</param>
        public static ChunkCluster Add(Int3 key, Chunk value)
        {
            ChunkCluster cluster = dictionary.GetOrAdd(key, (pos) =>
            {
                lock (_mutex)
                {
                    ChunkCluster cc = new ChunkCluster(Int3.One * 16)
                    {
                        Position = key
                    };
                    
                    return cc;
                }
            });

            cluster.AddChunk(value);
            return cluster;
        }

        //public static int Count { get; private set; }
        public static int Count => dictionary.Count;

        public static void Remove(Int3 key)
        {
            if (!dictionary.TryRemove(key, out var value))
                throw new Exception($"Removing an item {key}, that does not exist");
        }

        public static ChunkCluster GetValue(Int3 key) 
            => dictionary.TryGetValue(key, out ChunkCluster value) ? value : null;

        public static void Clear()
        {
            dictionary.Clear();
        }
        
        public static List<ChunkCluster> GetActiveChunkClusters()
        {
            return dictionary.Values.ToList();
        }

        
        /// <summary>
        /// Get the current Chunk based on the global chunk position
        /// </summary>
        /// <param name="chunkGlobalPosition">Expects a chunk in global space</param>
        /// <returns>Chunk at global position</returns>
        public static Chunk GetChunkAt(Int3 chunkGlobalPosition)
        {
            // (16, 0, 16) => (0, 0, 0)
            int clusterX = MathHelper.MultipleFloor(chunkGlobalPosition.X, 16 * 16);
            int clusterY = MathHelper.MultipleFloor(chunkGlobalPosition.Y, 16 * 16);
            int clusterZ = MathHelper.MultipleFloor(chunkGlobalPosition.Z, 16 * 16);
            
            return dictionary.TryGetValue(new Int3(clusterX, clusterY, clusterZ), out ChunkCluster cluster) 
                ? cluster.GetChunk((chunkGlobalPosition.X - clusterX) / 16, (chunkGlobalPosition.Y - clusterY) / 16, (chunkGlobalPosition.Z - clusterZ) / 16) 
                : null;
        }

        public static bool Contains(Int3 chunkGlobalPos)
        {
            Int3 clusterPos = new Int3(MathHelper.MultipleFloor(chunkGlobalPos.X, 16 * 16),
                MathHelper.MultipleFloor(chunkGlobalPos.Y, 16 * 16), MathHelper.MultipleFloor(chunkGlobalPos.Z, 16 * 16));

            if (_clusterCache != null)
            {
                if (_clusterCache.Position == clusterPos)
                {
                    return _clusterCache.GetChunk((chunkGlobalPos.X - clusterPos.X) / 16, (chunkGlobalPos.Y - clusterPos.Y) / 16,
                        (chunkGlobalPos.Z - clusterPos.Z) / 16) != null;
                }
            }
            
            bool state = dictionary.TryGetValue(clusterPos, out ChunkCluster cluster) 
                         && (cluster.GetChunk((chunkGlobalPos.X - clusterPos.X) / 16,
                            (chunkGlobalPos.Y - clusterPos.Y) / 16,
                            (chunkGlobalPos.Z - clusterPos.Z) / 16) != null);

            _clusterCache = cluster;

            return state;
        }
    }
}
