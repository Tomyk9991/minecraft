using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkCleanup
{
    private Int3 drawDistance;
    private int chunkSize;
    private List<Chunk> internalChunks;
    private object _mutex = new object();

    public ChunkCleanup(Int3 drawDistance, int chunkSize = 16)
    {
        this.drawDistance = drawDistance;
        this.chunkSize = chunkSize;
        internalChunks = new List<Chunk>();
    }
    
    public void CheckChunks(int xStart, int yStart, int zStart)
    {
        //TODO: Kann eigentlich asynchron laufen

        //TODO: mach das in einen Task + locke dann innerhalb die internalChunks-liste und füge hin
        Task.Run(() =>
        {
            var list = ChunkDictionary.GetActiveChunks();
        
            for (int i = 0; i < list.Count; i++)
            {
                Chunk c = list[i];
                if (Mathf.Abs(c.Position.X - xStart) > drawDistance.X + chunkSize ||
                    Mathf.Abs(c.Position.Y - yStart) > drawDistance.Y + chunkSize ||
                    Mathf.Abs(c.Position.Z - zStart) > drawDistance.Z + chunkSize)
                {
                    lock (_mutex)
                    {
                        internalChunks.Add(c);
                    }
                    ChunkDictionary.Remove(c.Position);
                    HashSetPositionChecker.Remove(c.Position);
                }
            };
        });
    }

    public void RemoveChunks(int batchsize = 100)
    {
        for (int i = 0; i < internalChunks.Count && i < batchsize; i++)
        {
            Chunk c = internalChunks[i];
            c.ReleaseGameObject();

            internalChunks.RemoveAt(i);
        }
    }
}
