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
            
            Mesh mesh = new Mesh()
            {
                name = g.transform.position.ToString(),
                indexFormat = IndexFormat.UInt32,
                vertices = meshData.Vertices.ToArray(),
                uv = meshData.UVs.ToArray(),
                colors = meshData.Colors.ToArray(),
                subMeshCount = 2,
            };
            
            mesh.SetTriangles(meshData.Triangles.ToArray(), 0);
            mesh.SetTriangles(meshData.TransparentTriangles.ToArray(), 1);
            
            mesh.RecalculateNormals();

            refMesh.sharedMesh = mesh;


            g.GetComponent<MeshCollider>().sharedMesh = null;

            Mesh colliderMesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32,
                vertices = colliderData.Vertices.ToArray(),
                triangles = colliderData.Triangles.ToArray()
            };

            g.GetComponent<MeshCollider>().sharedMesh = colliderMesh;
        }
    }
}
