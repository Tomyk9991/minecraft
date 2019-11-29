using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Builder
{
    public class MeshModifier
    {
        public void SetMesh(GameObject g, MeshData meshData, MeshData colliderData)
        {
            var refMesh = g.GetComponent<MeshFilter>().sharedMesh;
            
            refMesh.Clear();
            
            refMesh.indexFormat = IndexFormat.UInt32;
            refMesh.SetVertices(meshData.Vertices);
            refMesh.SetUVs(0, meshData.UVs);
            refMesh.SetColors(meshData.Colors);
            refMesh.subMeshCount = 2;

            refMesh.SetTriangles(meshData.Triangles, 0);
            refMesh.SetTriangles(meshData.TransparentTriangles, 1);
            refMesh.RecalculateNormals();


            var colliderReference = g.GetComponent<MeshCollider>();
            var collRefMesh = colliderReference.sharedMesh;
            
            collRefMesh.Clear();
            
            collRefMesh.name = g.transform.position.ToString();
            collRefMesh.indexFormat = IndexFormat.UInt32;
            collRefMesh.vertices = colliderData.Vertices.ToArray();
            collRefMesh.triangles = colliderData.Triangles.ToArray();

            colliderReference.sharedMesh = collRefMesh;
        }
    }
}
