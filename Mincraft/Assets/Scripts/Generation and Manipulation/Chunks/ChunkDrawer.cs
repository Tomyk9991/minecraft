using System;
using System.Collections.Generic;
using Attributes;
using Core.Builder;
using Core.Chunks.Threading;
using Core.Math;
using Core.Player.Interaction;
using Core.Rendering;
using Extensions;
using UnityEngine;

namespace Core.Chunks
{
    public class ChunkDrawer : SingletonBehaviour<ChunkDrawer>
    {
        public ChunkGameObjectPool GoPool { get; set; }

        [SerializeField] private int drawsPerFrame = 0;

        [SerializeField, ArrayElementTitle("BlockUV")]
        private List<InstancedMeshMaterialPair> customMeshes = null;

        private List<InstancedRenderer> renderers;


        private ChunkJobManager _chunkJobManager;

        private int jobsDoneInFrame = 0;

        private void Start()
        {
            GoPool = ChunkGameObjectPool.Instance;
            _chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;

            renderers = new List<InstancedRenderer>();

            foreach (var mmp in customMeshes)
            {
                var ir = new InstancedRenderer(mmp.Mesh, mmp.Material, mmp.BlockUV,
                    mmp.LocalPositionOffset.localPosition, mmp.LocalPositionOffset.localScale);
                renderers.Add(ir);
            }
            
            RemoveBlock.OnRemoveBlock += OnRemoveBlock;
        }

        private void OnRemoveBlock(BlockUV blockUV, Vector3 globalPosition)
        {
            List<InstancedRenderer> tempRenderers = renderers.FindAll(r => r.BlockType == blockUV);

            foreach (InstancedRenderer renderer in tempRenderers)
            {
                renderer.RemoveWithPosition(globalPosition);
            }
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
            DrawCustomMeshes();
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

            HandleCustomMeshes(drawingChunk, drawingChunk.Blocks.Where(block 
                => block.RenderingTechnique() == RenderingTechnique.CustomMesh)
            );
            MeshModifier.SetMesh(drawingChunk.CurrentGO, task.MeshData, task.ColliderData);
        }

        private void DrawCustomMeshes()
        {
            foreach (var pair in renderers)
                pair.Render();
        }

        private void HandleCustomMeshes(Chunk chunk, List<(Int3, Block)> blockSubSet)
        {
            foreach ((Int3 localPosition, Block block) in blockSubSet)
            {
                List<InstancedRenderer> tempRenderers = renderers.FindAll(r => r.BlockType == block.ID);

                foreach (InstancedRenderer renderer in tempRenderers)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(
                        chunk.GlobalPosition + localPosition.ToVector3() + renderer.LocalOffset,
                        Quaternion.identity,
                        renderer.LocalScale
                    );

                    renderer.AddUnique(matrix);
                }
            }
        }
        
    }

    [Serializable]
    public class InstancedMeshMaterialPair : MeshMaterialPair
    {
        public BlockUV BlockUV;
        public Transform LocalPositionOffset;
    }
}