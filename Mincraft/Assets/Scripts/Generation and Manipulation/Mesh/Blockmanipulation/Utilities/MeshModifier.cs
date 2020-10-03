using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
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

            collRefMesh.Clear(false);
            
            collRefMesh.indexFormat = colliderData.Vertices.Count >= 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            collRefMesh.SetVertices(colliderData.Vertices);
            collRefMesh.SetTriangles(colliderData.Triangles, 0);
            
            colliderReference.sharedMesh = collRefMesh;
        }
        
        public static void SetMeshTest(GameObject g, MeshData meshData, MeshData colliderData)
        {
            var meshReference = g.GetComponent<MeshFilter>();
            var refMesh = meshReference.sharedMesh;

            if (refMesh == null)
                refMesh = new Mesh();
            else
                refMesh.Clear();
        
            refMesh.indexFormat = meshData.Vertices.Count >= 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            
            refMesh.SetVertices(meshData.Vertices);
            refMesh.SetUVs(0, meshData.UVs);
            
            refMesh.subMeshCount = 2;
            refMesh.SetTriangles(meshData.Triangles, 0);
            refMesh.SetTriangles(meshData.TransparentTriangles, 1);
            
            refMesh.RecalculateNormals();

            meshReference.sharedMesh = refMesh;
        
            var colliderReference = g.GetComponent<MeshCollider>();
            var collRefMesh = colliderReference.sharedMesh;

            if (collRefMesh == null)
                collRefMesh = new Mesh();
            else
                collRefMesh.Clear(false);

            if (colliderData.Vertices != null)
            {
                collRefMesh.indexFormat = colliderData.Vertices.Count >= 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
                collRefMesh.SetVertices(colliderData.Vertices);
                collRefMesh.SetTriangles(colliderData.Triangles, 0);
            }
            
            colliderReference.sharedMesh = collRefMesh;
        }
    }
}
