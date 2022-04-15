using System.Collections.Generic;
using UnityEngine;

namespace Core.Builder
{
    public class MeshAdderParameter
    {
        public List<Vector3> Vertices { get; set; }
        public List<int> Triangles { get; set; }
        public List<int> TransparentTriangles { get; set; }
        public List<Vector2> Uvs { get; set; }
        public UVData[] CurrentUVData { get; set; }

        public Block Block { get; set; }
        
        public bool[] BoolNeighbours { get; set; }
        
        public float MeshOffset { get; set; }
        
        public Vector3 BlockPos { get; set; }
        public bool Transparent { get; set; }
    }
    
    public interface IMeshAdder
    {
        void AddMesh(MeshAdderParameter parameter);
    }
}