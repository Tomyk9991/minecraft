﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Builder
{
    public struct MeshData
    {
        public List<Vector3> Vertices { get; set; }
        public List<int> Triangles { get; set; }
        public List<int> TransparentTriangles { get; set; }
        public List<Vector2> UVs { get; set; }
        public List<Vector3> Normals { get; set; }
        public GameObject GameObject { get; private set; }
        
        public MeshData(List<Vector3> vertices, List<int> triangles, List<int> transparentTriangles, List<Vector2> uvs, GameObject go = null)
        {
            this.Vertices = vertices;
            this.Triangles = triangles;
            this.TransparentTriangles = transparentTriangles;
            this.UVs = uvs;
            this.Normals = null;
            this.GameObject = go;
        }

        public MeshData(List<Vector3> vertices, List<int> triangles, List<int> transparentTriangles, List<Vector2> uVs, List<Vector3> normals, GameObject gameObject = null)
        {
            Vertices = vertices;
            Triangles = triangles;
            TransparentTriangles = transparentTriangles;
            UVs = uVs;
            Normals = normals;
            GameObject = gameObject;
        }
    }
}
