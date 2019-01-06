using System.Collections.Generic;
using UnityEngine;

public struct MeshData
{
    public List<Vector3> Vertices { get; set; }
    public List<int> Triangles { get; set; }
    public List<Vector2> UVs { get; set; }
    

    public MeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Vertices = vertices;
        Triangles = triangles;
        UVs = uvs;
    }
}