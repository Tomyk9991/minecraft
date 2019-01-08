using System.Collections.Generic;
using UnityEngine;

public struct MeshData
{
    public List<Vector3> Vertices { get; set; }
    public List<int> Triangles { get; set; }
    public List<Vector2> UVs { get; set; }

    public GameObject GameObject { get; private set; }
    

    public MeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, GameObject go = null)
    {
        Vertices = vertices;
        Triangles = triangles;
        UVs = uvs;
        GameObject = go;
    }
}