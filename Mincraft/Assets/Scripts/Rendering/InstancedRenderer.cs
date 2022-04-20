using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Rendering
{
    public abstract class InstancedRenderer
    {
        protected List<Matrix4x4> matrices;
        protected Mesh mesh;
        protected Material material;

        public void AddUnique(in Matrix4x4 matrix)
        {
            if (!matrices.Contains(matrix))
            {
                matrices.Add(matrix);
            }
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

        public virtual void Render()
        {
            int index = 0;
            while (true)
            {
                int countedElements = System.Math.Min(matrices.Count, 1023);
                
                List<Matrix4x4> segment = matrices.GetRange(index, countedElements);
                Graphics.DrawMeshInstanced(mesh, 0, material, segment);

                index += countedElements + 1;

                if (matrices.Count <= index)
                    break;
            }
        }
    }
}