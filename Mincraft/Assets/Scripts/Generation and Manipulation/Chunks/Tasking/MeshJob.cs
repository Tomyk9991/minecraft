using System;
using System.Collections.Generic;
using System.Linq;
using Core.Builder;
using Core.Math;
using Core.Performance.Parallelisation;
using UnityEngine;

namespace Core.Chunking.Threading
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
        
        public void ExecuteJob()
        {
            if (this.Chunk.GetBlocks().All(block => block.Equals(Block.Empty())))
            {
                this.MeshData = new MeshData(new List<Vector3>(0), new List<int>(0), new List<int>(0), new List<Vector2>(0), new List<Color>(0));
                this.ColliderData = new MeshData(new List<Vector3>(0), new List<int>(0), null, null, null);
                return;
            }

            if (WorldSettings.CalculateShadows)
            {
                this.Chunk.CalculateLight();
                this.MeshData = MeshBuilder.Combine(this.Chunk);
                this.ColliderData = GreedyMesh.ReduceMesh(this.Chunk);
            }
            else
            {
                //Parallelize den Process
                this.MeshData = MeshBuilder.Combine(this.Chunk);
                this.ColliderData = GreedyMesh.ReduceMesh(this.Chunk);
            }
            
            this.Chunk.ChunkState = ChunkState.Generated;
        }
    }
}