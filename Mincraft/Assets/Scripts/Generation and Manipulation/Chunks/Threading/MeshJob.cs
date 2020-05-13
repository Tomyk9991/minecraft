using System;
using Core.Builder;

namespace Core.Chunks.Threading
{
    public class MeshJob
    {
        public Chunk Chunk { get; set; }
        public MeshData MeshData { get; set; }
        public MeshData ColliderData { get; set; }

        public MeshJob(Chunk chunk)
        {
            this.Chunk = chunk ?? throw new Exception("Chunk is null");
            this.MeshData = new MeshData();
            this.ColliderData = new MeshData();
        }
    }
}