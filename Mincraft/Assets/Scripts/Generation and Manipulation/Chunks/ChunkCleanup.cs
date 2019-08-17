using System.Collections.Generic;
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
        
        public void CheckChunks(int xStart, int yStart, int zStart, List<Chunk> chunks)
        {
            
            for (int i = 0; i < chunks.Count; i++)
            {
                Chunk c = chunks[i];
                
                if (Mathf.Abs(c.GlobalPosition.X - xStart) > drawDistance.X + chunkSize ||
                    Mathf.Abs(c.GlobalPosition.Y - yStart) > drawDistance.Y + chunkSize ||
                    Mathf.Abs(c.GlobalPosition.Z - zStart) > drawDistance.Z + chunkSize)
                {
                    //ChunkClusterDictionary.Remove(c.GlobalPosition);
                    HashSetPositionChecker.Remove(c.GlobalPosition);
                    c.ReleaseGameObject();
                    chunks.RemoveAt(i);
                }
            }
        }
        
    //    public void CheckChunks(int xStart, int yStart, int zStart)
    //    {
    //        var list = ChunkDictionary.GetActiveChunks();
    //
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            Chunk c = list[i];
    //            
    //            if (Mathf.Abs(c.LocalPosition.X - xStart) > drawDistance.X + chunkSize ||
    //                Mathf.Abs(c.LocalPosition.Y - yStart) > drawDistance.Y + chunkSize ||
    //                Mathf.Abs(c.LocalPosition.Z - zStart) > drawDistance.Z + chunkSize)
    //            {
    //                ChunkDictionary.Remove(c.LocalPosition);
    //                HashSetPositionChecker.Remove(c.LocalPosition);
    //                c.ReleaseGameObject();
    //            }
    //        }
    //    }
    }
}
