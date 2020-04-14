using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Builder
{
    public static class MeshModifier
    {
        public static void SetMesh(GameObject g, in MeshData meshData, in MeshData colliderData)
        {
            var meshReference = g.GetComponent<MeshFilter>();
            var refMesh = meshReference.sharedMesh;
            
            refMesh.Clear();
            
            refMesh.indexFormat = IndexFormat.UInt32;
            refMesh.SetVertices(meshData.Vertices);
            refMesh.SetUVs(0, meshData.UVs);
            refMesh.SetColors(meshData.Colors);
            refMesh.subMeshCount = 2;

            refMesh.SetTriangles(meshData.Triangles, 0);
            refMesh.SetTriangles(meshData.TransparentTriangles, 1);
            refMesh.RecalculateNormals();

            meshReference.sharedMesh = refMesh;
            

            var colliderReference = g.GetComponent<MeshCollider>();
            var collRefMesh = colliderReference.sharedMesh;

            collRefMesh.Clear();
            
            collRefMesh.name = g.transform.position.ToString();
            collRefMesh.indexFormat = IndexFormat.UInt32;
            collRefMesh.SetVertices(colliderData.Vertices);
            collRefMesh.SetTriangles(colliderData.Triangles, 0);

            colliderReference.sharedMesh = collRefMesh;
        }
    }
}
