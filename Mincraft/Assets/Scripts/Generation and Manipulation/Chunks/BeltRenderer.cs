using System;
using System.Collections.Generic;
using Attributes;
using Core.Builder;
using Core.Math;
using Core.Player.Interaction;
using Core.Rendering;
using Extensions;
using UnityEngine;

namespace Core.Chunks
{
    [Serializable]
    public class BeltRenderer
    {
        [SerializeField, ArrayElementTitle("BeltID")]
        private List<InstancedMeshMaterialPair> customMeshes = null;
        
        private List<InstancedBeltRenderer> renderers;
        
        public void Init()
        {
            renderers = new List<InstancedBeltRenderer>();
            
            foreach (var mmp in customMeshes)
            {
                var ir = new InstancedBeltRenderer(mmp.Mesh, mmp.Material, mmp.BeltID,
                    mmp.LocalPositionOffset.localPosition, mmp.LocalPositionOffset.localScale);
                renderers.Add(ir);
            }
            
            RemoveBlock.OnRemoveBlock += OnRemoveBlock;
            ChunkBuffer.OnRemoveChunk += OnRemoveChunk;
        }

        public void OnRemoveChunk(Chunk chunk)
        {
            var blockSubSet = chunk.SpecialRenderingBlocks;
            
            foreach (var positionedBlock in blockSubSet)
            {
                OnRemoveBlock(positionedBlock.Block.ID, (chunk.GlobalPosition + positionedBlock.LocalPosition).ToVector3());
            }
        }

        public void OnRemoveBlock(BlockUV blockUV, Vector3 globalPosition)
        {
            List<InstancedBeltRenderer> tempRenderers = renderers.FindAll(r => r.BeltType.ToBlockUV() == blockUV);

            foreach (InstancedBeltRenderer renderer in tempRenderers)
            {
                renderer.RemoveWithPosition(globalPosition);
            }
        }
        
        public void DrawCustomMeshes()
        {
            foreach (var renderer in renderers)
                renderer.Render();
        }

        public void HandleCustomMeshes(Chunk chunk, List<PositionedBlock> blockSubSet)
        {
            foreach (var positionedBlock in blockSubSet)
            {
                foreach (InstancedBeltRenderer renderer in renderers)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(
                        chunk.GlobalPosition + positionedBlock.LocalPosition.ToVector3() + renderer.LocalOffset,
                        BlockDirectionToQuaternion(positionedBlock.Block.Direction),
                        renderer.LocalScale
                    );

                    renderer.AddUnique(matrix);
                }
            }
        }

        private Quaternion BlockDirectionToQuaternion(BlockDirection direction)
        {
            switch (direction)
            {
                case BlockDirection.Right:
                    return Quaternion.Euler(Vector3.up * 0.0f);
                case BlockDirection.Back:
                    return Quaternion.Euler(Vector3.up * 90.0f);
                case BlockDirection.Left:
                    return Quaternion.Euler(Vector3.up * 180.0f);
                case BlockDirection.Forward:
                    return Quaternion.Euler(Vector3.up * 270.0f);
            }

            return Quaternion.identity;
        }
    }
    
    
    [Serializable]
    public class InstancedMeshMaterialPair : MeshMaterialPair
    {
        public BeltID BeltID;
        public Transform LocalPositionOffset;
    }
}