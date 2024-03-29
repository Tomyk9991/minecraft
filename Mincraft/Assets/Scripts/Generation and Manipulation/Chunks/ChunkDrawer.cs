﻿using Core.Builder;
using Core.Chunks.Threading;
using Extensions;
using UnityEngine;

namespace Core.Chunks
{
    public class ChunkDrawer : SingletonBehaviour<ChunkDrawer>
    {
        public ChunkGameObjectPool GoPool { get; set; }

        [SerializeField] private int drawsPerFrame = 0;
        [SerializeField] private BeltRenderer renderer = null;


        private ChunkJobManager _chunkJobManager;

        private int jobsDoneInFrame = 0;

        private void Start()
        {
            GoPool = ChunkGameObjectPool.Instance;
            _chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;
            
            renderer.Init();
        }

        private void Update()
        {
            while (_chunkJobManager.FinishedJobsCount > 0 && jobsDoneInFrame < drawsPerFrame)
            {
                MeshJob task = _chunkJobManager.DequeueFinishedJob();
                if (task?.MeshData.Vertices != null)
                {
                    RenderCall(task);
                    jobsDoneInFrame++;
                }
            }

            jobsDoneInFrame = 0;
            renderer.DrawCustomMeshes();
        }

        private void RenderCall(MeshJob task)
        {
            var drawingChunk = task.Chunk;

            if (drawingChunk.CurrentGO == null)
            {
                drawingChunk.CurrentGO = GoPool.GetNextUnusedChunk();
                drawingChunk.CurrentGO.name = drawingChunk.GlobalPosition.ToString();
                drawingChunk.CurrentGO.transform.position = drawingChunk.GlobalPosition.ToVector3();
                drawingChunk.CurrentGO.GetComponent<ChunkReferenceHolder>().Chunk = drawingChunk;
            }

            renderer.HandleCustomMeshes(drawingChunk, drawingChunk.SpecialRenderingBlocks);
            MeshModifier.SetMesh(drawingChunk.CurrentGO, task.MeshData, task.ColliderData);
        }
    }
}