using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class ModifyMesh
{
    private static Vector2[] uvs = null;

    public static void RemoveBlockFromMesh(Transform currentChunk, Block objectToRemove)
    {
        currentChunk.GetComponent<IChunk>().RemoveBlock(objectToRemove);
        CombineForAll(currentChunk.gameObject, remove: true);
    }

    public static Vector3 CenteredClickPosition(int[] triangles, Vector3[] vertices, Vector3 normal, int triangleIndex)
    {
        (Vector3 shared1, Vector3 shared2) = GetHypotenuse(triangles, vertices, triangleIndex);
        Vector3 hypotMid = shared1 + ((shared2 - shared1)) / 2.0f;

        return (hypotMid - normal.normalized * .5f);
    }

    private static (Vector3, Vector3) GetHypotenuse(int[] triangles, Vector3[] vertices, int hitTri)
    {
        Vector3 p0 = vertices[triangles[hitTri * 3 + 0]];
        Vector3 p1 = vertices[triangles[hitTri * 3 + 1]];
        Vector3 p2 = vertices[triangles[hitTri * 3 + 2]];

        float edge1 = Vector3.Distance(p0, p1);
        float edge2 = Vector3.Distance(p0, p2);
        float edge3 = Vector3.Distance(p1, p2);

        Vector3 shared1;
        Vector3 shared2;

        if (edge1 > edge2 && edge1 > edge3)
        {
            shared1 = p0;
            shared2 = p1;
        }
        else if (edge2 > edge1 && edge2 > edge3)
        {
            shared1 = p0;
            shared2 = p2;
        }
        else
        {
            shared1 = p1;
            shared2 = p2;
        }

        return (shared1, shared2);
    }

    public static Vector3 CenteredClickPositionOutSide(Vector3 hitPoint, Vector3 hitNormal)
    {
        Vector3 blockPos = hitPoint + hitNormal / 2.0f;

        blockPos.x = (float) Math.Round(blockPos.x, MidpointRounding.AwayFromZero);
        blockPos.y = (float) Math.Round(blockPos.y, MidpointRounding.AwayFromZero);
        blockPos.z = (float) Math.Round(blockPos.z, MidpointRounding.AwayFromZero);

        return blockPos;
    }

    public static void CombineForAll(GameObject currentChunk, bool remove = false)
    {
        Vector3 pos = currentChunk.transform.position;

        IChunk chunk = currentChunk.GetComponent<IChunk>();
        currentChunk.transform.position = Vector3.zero;
        currentChunk.SetActive(false);

        if (uvs == null)
            uvs = SetUVs.GetStandardUVs();

        Block[] blocks = chunk.GetBlocks();
        CombineInstance[] combine = new CombineInstance[blocks.Length];

        currentChunk.GetComponents<MeshCollider>().ToList().ForEach(GameObject.Destroy);

        for (int i = 0; i < blocks.Length; i++)
        {
            combine[i].mesh = blocks[i].Mesh;
            combine[i].transform = Matrix4x4.TRS(blocks[i].Position, Quaternion.identity, Vector3.one);
        }

        MeshFilter refMesh = currentChunk.GetComponent<MeshFilter>();


        refMesh.mesh = new Mesh();
        refMesh.mesh.CombineMeshes(combine, true);

        List<Vector2> newMeshUVs = new List<Vector2>();

        for (int i = 0; i < blocks.Length; i++)
        {
            //add new UVs based on individual block settings
            UVSetter suv = blocks[i].UVSetter;
            float tilePerc = 1 / UVSetter.pixelSize;
            float umin = tilePerc * suv.TileX;
            float umax = tilePerc * (suv.TileX + 1);
            float vmin = tilePerc * suv.TileY;
            float vmax = tilePerc * (suv.TileY + 1);

            for (int j = 0; j < 24; j++)
            {
                float x = Mathf.Approximately(uvs[j].x, 0f) ? umin : umax;
                float y = Mathf.Approximately(uvs[j].y, 0f) ? vmin : vmax;

                newMeshUVs.Add(new Vector2(x, y));
            }
        }

        refMesh.mesh.uv = newMeshUVs.ToArray();

        refMesh.mesh.RecalculateBounds();
        refMesh.mesh.RecalculateNormals();

        currentChunk.transform.position = pos;
        currentChunk.AddComponent<MeshCollider>();
        currentChunk.gameObject.SetActive(true);
    }

    public static void Combine(Block blockToAdd, GameObject currentChunk)
    {
        Vector3 pos = currentChunk.transform.position;
        currentChunk.transform.position = Vector3.zero;

        MeshFilter meshFilter = currentChunk.GetComponent<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[2];


        currentChunk.GetComponents<MeshCollider>().ToList().ForEach(GameObject.Destroy);

        Vector2[] oldMeshUVs = currentChunk.GetComponent<MeshFilter>().mesh.uv;


        combine[0].mesh = meshFilter.sharedMesh;
        combine[0].transform = meshFilter.transform.localToWorldMatrix;
        meshFilter.gameObject.SetActive(false);

        combine[1].mesh = blockToAdd.Mesh; // TODO: Hier möglicherweise zu mesh wechseln
        Matrix4x4 m = Matrix4x4.TRS(blockToAdd.Position, Quaternion.identity, Vector3.one);
        combine[1].transform = m;


        MeshFilter refMesh = currentChunk.GetComponent<MeshFilter>();
        refMesh.mesh = new Mesh();

        refMesh.mesh.CombineMeshes(combine, true);

        //make new UV array
        Vector2[] newMeshUVs = new Vector2[oldMeshUVs.Length + 24];

//        //copy over all UVs
        for (int j = 0; j < oldMeshUVs.Length; j++)
            newMeshUVs[j] = oldMeshUVs[j];

        UVSetter suv = blockToAdd.UVSetter;
        float tilePerc = 1 / UVSetter.pixelSize;
        float umin = tilePerc * suv.TileX;
        float umax = tilePerc * (suv.TileX + 1);
        float vmin = tilePerc * suv.TileY;
        float vmax = tilePerc * (suv.TileY + 1);

        if (uvs == null)
            uvs = SetUVs.GetStandardUVs();

        int k = 0;
        for (int j = newMeshUVs.Length - 24; j < newMeshUVs.Length; j++)
        {
            float x = Mathf.Approximately(uvs[k].x, 0f) ? umin : umax;
            float y = Mathf.Approximately(uvs[k].y, 0f) ? vmin : vmax;

            newMeshUVs[j] = new Vector2(x, y);
            k++;
        }

        refMesh.mesh.uv = newMeshUVs;

        refMesh.mesh.RecalculateBounds();
        refMesh.mesh.RecalculateNormals();

        currentChunk.transform.position = pos;
        currentChunk.AddComponent<MeshCollider>();
        currentChunk.gameObject.SetActive(true);
    }
}

