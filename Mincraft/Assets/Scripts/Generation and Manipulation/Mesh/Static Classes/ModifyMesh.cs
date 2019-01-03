using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class ModifyMesh
{
    private static Vector2[] uvs = null;
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
    
    public static MeshData Combine(IChunk chunk)
    {
        List<Block> blocks = chunk.GetBlocks();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        List<int> masterTri = MeshData.BoxTriangles;
        List<Vector3> masterVert = MeshData.BoxVertices;

        for (int i = 0; i < blocks.Count; i++)
        {
            List<Vector3> vertsInBlock = new List<Vector3>();
            List<int> triInBlock = new List<int>();

            for (int j = 0; j < masterTri.Count; j++)
            {
                int triangleOffset = (i * 24);
                triInBlock.Add(masterTri[j] + triangleOffset);
            }

            triangles.AddRange(triInBlock);

            for (int j = 0; j < masterVert.Count; j++)
            {
                vertsInBlock.Add(masterVert[j] + blocks[i].Position);
            }

            vertices.AddRange(vertsInBlock);
        }

        if (uvs == null)
            uvs = MeshData.BoxUVs;
        
        List<Vector2> newMeshUVs = new List<Vector2>();

        for (int i = 0; i < blocks.Count; i++)
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

        return new MeshData(vertices, triangles, newMeshUVs);
    }
}

