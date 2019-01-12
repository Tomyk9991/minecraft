﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class ModifyMesh
{
	private static Vector3[] directions = {Vector3.forward, Vector3.zero, Vector3.up, Vector3.zero, Vector3.zero, Vector3.right};
	private static Vector3[] offset1 = {Vector3.right, Vector3.right, Vector3.right, Vector3.right, Vector3.forward, Vector3.forward};
	private static Vector3[] offset2 = {Vector3.up, Vector3.up, Vector3.forward, Vector3.forward, Vector3.up, Vector3.up};
	private static int[] tri1 = {1, 0, 2, 1, 2, 3};
	private static int[] tri2 = { 0, 1, 2, 2, 1, 3 };
	private static int[] tris = { 1, 0, 0, 1, 1, 0 };
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

        blockPos.x = Mathf.FloorToInt(blockPos.x);
        blockPos.y = Mathf.FloorToInt(blockPos.y);
        blockPos.z = Mathf.FloorToInt(blockPos.z);

        return blockPos;
    }

    public static MeshData Combine(IChunk chunk)
    {   
	    Block[] blocks = chunk.GetBlocks();
	    List<Vector3> vertices = new List<Vector3>();
	    List<int> triangles = new List<int>();
	    List<Vector2> uvs = new List<Vector2>();

	    bool[] neigbours = new bool[6];

	    for (int i = 0; i < blocks.Length; i++)
	    {
		    if (blocks[i] == default)
		    {
			    continue;
		    }
		    neigbours = chunk.BoolNeigbours(blocks[i].Position);
		    
		    
		    if (neigbours.Any(state => state == false))
		    {
			    var currentUVData = UVDictionary.GetValue((BlockUV) blocks[i].ID);

			    for (int j = 0; j < neigbours.Length; j++)
			    {
				    if (neigbours[j] == false)
				    {
					    int vc = vertices.Count;
					    vertices.Add(directions[j] + blocks[i].Position);
					    vertices.Add(directions[j] + offset1[j] + blocks[i].Position);
					    vertices.Add(directions[j] + offset2[j] + blocks[i].Position);
					    vertices.Add(directions[j] + offset1[j] + offset2[j] + blocks[i].Position);
	
					    for (int k = 0; k < 6; k++)
					    {
						    triangles.Add(vc + (tris[j] == 0 ? tri1[k] : tri2[k]));
					    }
	
					    uvs.Add(new Vector2(currentUVData[j].TileX, currentUVData[j].TileY));
					    uvs.Add(new Vector2(currentUVData[j].TileX + currentUVData[j].SizeX, currentUVData[j].TileY));
					    uvs.Add(new Vector2(currentUVData[j].TileX, currentUVData[j].TileY + currentUVData[j].SizeY));
					    uvs.Add(new Vector2(currentUVData[j].TileX + currentUVData[j].SizeX,currentUVData[j].TileY + currentUVData[j].SizeY));
				    }
			    }
		    }
	    }
	    
	    return new MeshData(vertices, triangles, uvs, chunk.CurrentGO);
    }
}