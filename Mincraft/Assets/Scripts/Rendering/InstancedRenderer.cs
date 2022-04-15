using System.Collections.Generic;
using System.Linq;
using Core.Builder;
using Core.Math;
using UnityEngine;

namespace Core.Rendering
{
    public class InstancedRenderer
    {
        private List<Matrix4x4> matrices;
        private Mesh mesh;
        private Material material;

        public BlockUV BlockType { get; set; }
        public Vector3 LocalOffset { get; set; }
        public Vector3 LocalScale { get; set; }
        
        public InstancedRenderer(Mesh mesh, Material material, BlockUV blockType, in Vector3 localOffset, in Vector3 localScale)
        {
            this.mesh = mesh;
            this.material = material;
            this.matrices = new List<Matrix4x4>();
            
            this.BlockType = blockType;
            this.LocalOffset = localOffset;
            this.LocalScale = localScale;
        }

        public void AddUnique(in Matrix4x4 matrix)
        {
            if (!matrices.Contains(matrix))
                matrices.Add(matrix);
        }

        public void Render()
        {
            // TODO: Not more than 1023 objects are possible to draw in on batch.
            // Split the DrawMeshInstanced Call in multiple, if needed
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
        }

        public void RemoveWithPosition(Vector3 globalPosition)
        {
            float closest = float.MaxValue;
            int index = -1;
            
            for (var i = 0; i < matrices.Count; i++)
            {
                Matrix4x4 matrix = matrices[i];
                Vector3 pos = new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);

                float current = (globalPosition - pos).sqrMagnitude;
                if (current < closest)
                {
                    closest = current;
                    index = i;
                }
            }

            if (index != -1)
                matrices.RemoveAt(index);
        }
    }
}