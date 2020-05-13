using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Builder
{
    public static class MeshModifier
    {
        public static void SetMesh(GameObject g, MeshData meshData, MeshData colliderData)
        {
            var meshReference = g.GetComponent<MeshFilter>();
            var refMesh = meshReference.sharedMesh;
            
            refMesh.Clear();

            refMesh.indexFormat = meshData.Vertices.Count >= 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            
            refMesh.SetVertices(meshData.Vertices);
            refMesh.SetUVs(0, meshData.UVs);
            
            refMesh.subMeshCount = 2;
            refMesh.SetTriangles(meshData.Triangles, 0);
            refMesh.SetTriangles(meshData.TransparentTriangles, 1);
            
            refMesh.RecalculateNormals();

            var colliderReference = g.GetComponent<MeshCollider>();
            var collRefMesh = colliderReference.sharedMesh;
            
            collRefMesh.Clear();
            
            collRefMesh.indexFormat = colliderData.Vertices.Count >= 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            collRefMesh.SetVertices(colliderData.Vertices);
            collRefMesh.SetTriangles(colliderData.Triangles, 0);
            
            // colliderReference.sharedMesh = collRefMesh;
        }
    }
}
