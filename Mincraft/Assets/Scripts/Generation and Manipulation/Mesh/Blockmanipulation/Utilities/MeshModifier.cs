using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Builder
{
    public class MeshModifier
    {
        private static Vector3[] directions = {Vector3.forward, Vector3.zero, Vector3.up, Vector3.zero, Vector3.zero, Vector3.right};
        private static Vector3[] offset1 = {Vector3.right, Vector3.right, Vector3.right, Vector3.right, Vector3.forward, Vector3.forward};
        private static Vector3[] offset2 = {Vector3.up, Vector3.up, Vector3.forward, Vector3.forward, Vector3.up, Vector3.up};
        private static int[] tri1 = {1, 0, 2, 1, 2, 3};
        private static int[] tri2 = { 0, 1, 2, 2, 1, 3 };
        private static int[] tris = { 1, 0, 0, 1, 1, 0 };

        //public event EventHandler<MeshData> MeshAvailable = null;

        public void SetMesh(GameObject g, MeshData meshData, MeshData colliderData)
        {
            var refMesh = g.GetComponent<MeshFilter>();
            refMesh.mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
                vertices = meshData.Vertices.ToArray(),
                uv = meshData.UVs.ToArray(),
                subMeshCount = 2
            };

            refMesh.mesh.SetTriangles(meshData.Triangles.ToArray(), 0);
            refMesh.mesh.SetTriangles(meshData.TransparentTriangles.ToArray(), 1);


            refMesh.mesh.RecalculateNormals();

            //TODO füge MeshCollider wieder ein
            g.GetComponent<MeshCollider>().sharedMesh = null;


            Mesh colliderMesh = new Mesh();
            colliderMesh.indexFormat = IndexFormat.UInt32;
            colliderMesh.vertices = colliderData.Vertices.ToArray();
            colliderMesh.triangles = colliderData.Triangles.ToArray();

            g.GetComponent<MeshCollider>().sharedMesh = colliderMesh;
        }
    }
}
