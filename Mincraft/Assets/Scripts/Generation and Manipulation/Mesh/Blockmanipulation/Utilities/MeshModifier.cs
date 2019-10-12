using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Builder
{
    public class MeshModifier
    {
        public void SetMesh(GameObject g, MeshData meshData, MeshData colliderData)
        {
            var refMesh = g.GetComponent<MeshFilter>();
            refMesh.mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
                vertices = meshData.Vertices.ToArray(),
                uv = meshData.UVs.ToArray(),
                colors = meshData.Colors.ToArray(),
                subMeshCount = 2
            };

            refMesh.mesh.SetTriangles(meshData.Triangles.ToArray(), 0);
            refMesh.mesh.SetTriangles(meshData.TransparentTriangles.ToArray(), 1);

            refMesh.mesh.RecalculateNormals();

            g.GetComponent<MeshCollider>().sharedMesh = null;


            Mesh colliderMesh = new Mesh();
            colliderMesh.indexFormat = IndexFormat.UInt32;
            colliderMesh.vertices = colliderData.Vertices.ToArray();
            colliderMesh.triangles = colliderData.Triangles.ToArray();

            g.GetComponent<MeshCollider>().sharedMesh = colliderMesh;
        }
    }
}
