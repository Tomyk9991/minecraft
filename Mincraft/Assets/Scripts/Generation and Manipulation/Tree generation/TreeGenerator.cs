using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;

namespace Core.Builder.Generation
{
    public abstract class TreeGenerator
    {
        //Achte darauf nicht mehrmals den selben Chunk hinzuzufügen
        public List<ChunkJob> chunkJobs = new List<ChunkJob>();
        private List<Chunk> cachedNeighbours = new List<Chunk>();
        
        private const int chunkSize = 16;
        
        /// <summary>
        /// Adds an block to the corresponding Chunk automatically
        /// </summary>
        /// <param name="block">The blockposition has to be in global space</param>
        /// <param name="chunk">The chunk, you're setting the initial tree plant</param>
        public void SetBlock(Block block, Chunk chunk) // blockpos kommt in global space an
        {
            int x = block.Position.X - chunk.LocalPosition.X;
            int y = block.Position.Y - chunk.LocalPosition.Y;
            int z = block.Position.Z - chunk.LocalPosition.Z;
            
            //Wenn der zuzufügende Block bereits im mitgegebenen Chunk liegt
            if (x >= 0 && x < chunkSize && y >= 0 && y < chunkSize && z >= 0 && z < chunkSize)
            {
                // Wenn es der selbe Chunk ist, wie der von dem gecallt wird, dann brauche
                // ich diesen auch nicht mehr auf die Draw-Liste zu packen, weil der sich
                // ja schon drauf befindet. Von dem geht ja der eigentliche "Generate"- Call
                // aus
                block.Position.X = x;
                block.Position.Y = y;
                block.Position.Z = z;
                
                chunk.AddBlock(block);
                return;
            }

            int chunkX = Mathf.FloorToInt(block.Position.X / 16f) * 16;
            int chunkY = Mathf.FloorToInt(block.Position.Y / 16f) * 16;
            int chunkZ = Mathf.FloorToInt(block.Position.Z / 16f) * 16;
            
            Int3 chunkPos = new Int3(chunkX, chunkY, chunkZ);
            bool foundInDictionary = false;

            Chunk c = cachedNeighbours.FirstOrDefault(t => t.LocalPosition == chunkPos);

    //        Chunk c = cachedNeighbours.FirstOrDefault(cc => cc.Position == chunkPos);
            //Statt zu invertieren, einfach vorne hinzufügen

    //        if (c == null)
    //        {
    //            c = ChunkDictionary.GetValue(chunkPos);
    //            foundInDictionary = true;
    //        }

            // War nicht in den cached-chunks gewesen sein
            // Weder ein Chunk wurde von den bisherigen Blättern gecachet, noch existiert so ein Chunk überhaupt
    //        if (c == null)
    //        {
    //            ChunkJob job = new ChunkJob();
    //            c = job.CreateChunk(chunkPos);
    //            
    //            block.Position -= chunkPos;
    //            c.AddBlock(block);
    //
    //            
    //            ChunkDictionary.Add(chunkPos, c);
    //            c.AddedToDick = true;
    //            HashSetPositionChecker.Add(chunkPos);
    //            c.AddedToHash = true;
    //
    //            chunkJobs.Add(job);
    //            cachedNeighbours.Insert(0, c);
    //        }
    //        else
    //        {
    //            block.Position -= chunkPos;
    //            c.AddBlock(block);
    //
    //            if (c.IsDrawn)
    //            {
    //                //Wenn der Chunk bereits gezeichnet ist, dann muss der Chunk zum neuen Zeichnen 
    //            }
    //            
    //            chunkJobs.Add(job);
    //
    //            if (foundInDictionary)
    //                cachedNeighbours.Insert(0, c);
    //        }
        }

        public virtual List<ChunkJob> Generate(Chunk c, int x, int y, int z)
        {
            cachedNeighbours.Clear();
            cachedNeighbours.Add(c);

            chunkJobs.Clear();

            return null;
        }
    }
}
