using System.Collections.Generic;
using Core.Builder;
using UnityEngine;

namespace Core.Rendering
{
    public class InstancedBeltRenderer : InstancedRenderer
    {
        public BeltID BeltType { get; set; }
        public Vector3 LocalOffset { get; set; }
        public Vector3 LocalScale { get; set; }
        
        public InstancedBeltRenderer(Mesh mesh, Material material, BeltID beltType, in Vector3 localOffset, in Vector3 localScale)
        {
            this.mesh = mesh;
            this.material = material;
            this.matrices = new List<Matrix4x4>();

            this.BeltType = beltType;
            this.LocalOffset = localOffset;
            this.LocalScale = localScale;
        }
    }
}