﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Core.Chunking;
using System;

namespace Core.Builder
{
	public static class MeshBuilder
	{
		private static Vector3[] directions = {Vector3.forward, Vector3.zero, Vector3.up, Vector3.zero, Vector3.zero, Vector3.right};
		private static Vector3[] offset1 = {Vector3.right, Vector3.right, Vector3.right, Vector3.right, Vector3.forward, Vector3.forward};
		private static Vector3[] offset2 = {Vector3.up, Vector3.up, Vector3.forward, Vector3.forward, Vector3.up, Vector3.up};
		private static int[] tri1 = {1, 0, 2, 1, 2, 3};
		private static int[] tri2 = { 0, 1, 2, 2, 1, 3 };
		private static int[] tris = { 1, 0, 0, 1, 1, 0 };


	    public static Vector3 CenteredClickPositionOutSide(Vector3 hitPoint, Vector3 hitNormal)
	    {
	        //Hier wird global berechnet.
	        Vector3 blockPos = hitPoint + hitNormal / 2.0f;

	        blockPos.x = Mathf.FloorToInt(blockPos.x);
	        blockPos.y = Mathf.FloorToInt(blockPos.y);
	        blockPos.z = Mathf.FloorToInt(blockPos.z);

	        //Benötigt aber lokale Berechnung

	        return blockPos;
	    }

	    public static MeshData Combine(Chunk chunk)
	    {   
		    Block[] blocks = chunk.GetBlocks();
		    List<Vector3> vertices = new List<Vector3>();
		    List<int> triangles = new List<int>();
	        List<int> transparentTriangles = new List<int>();
		    List<Vector2> uvs = new List<Vector2>();

		    bool[] neigbours = new bool[6];

		    for (int i = 0; i < blocks.Length; i++)
	        {
	            bool transparent = blocks[i].IsTransparent();
			    if (blocks[i].ID == (int) BlockUV.Air)
			    {
				    continue;
			    }


	            //Check, ob dieser Block transparent ist, oder nicht
	            // Wenn es so sein sollte, bleibt das neighbours-Array mit 6 false-Werten und jede Seite wird gezeichnet

	            neigbours = !transparent
	                ? chunk.BoolNeigbours(blocks[i].Position)
	                : new [] { false, false, false, false, false, false };


			    Vector3 blockPos = blocks[i].Position.ToVector3();
			    
			    if (neigbours.Any(state => state == false))
			    {
				    UVData[] currentUVData = UVDictionary.GetValue((BlockUV) blocks[i].ID);

				    for (int j = 0; j < 6; j++)
				    {
					    if (neigbours[j] == false)
					    {
						    int vc = vertices.Count;
						    vertices.Add(directions[j] + blockPos);
						    vertices.Add(directions[j] + offset1[j] + blockPos);
						    vertices.Add(directions[j] + offset2[j] + blockPos);
						    vertices.Add(directions[j] + offset1[j] + offset2[j] + blockPos);

	                        if (!transparent)
	                        {
						        for (int k = 0; k < 6; k++)
						        {
							        triangles.Add(vc + (tris[j] == 0 ? tri1[k] : tri2[k]));
						        }
	                        }
	                        else
	                        {
	                            for (int k = 0; k < 6; k++)
	                            {
	                                transparentTriangles.Add(vc + (tris[j] == 0 ? tri1[k] : tri2[k]));
	                            }
	                        }
		
						    uvs.Add(new Vector2(currentUVData[j].TileX, currentUVData[j].TileY));
						    uvs.Add(new Vector2(currentUVData[j].TileX + currentUVData[j].SizeX, currentUVData[j].TileY));
						    uvs.Add(new Vector2(currentUVData[j].TileX, currentUVData[j].TileY + currentUVData[j].SizeY));
						    uvs.Add(new Vector2(currentUVData[j].TileX + currentUVData[j].SizeX,currentUVData[j].TileY + currentUVData[j].SizeY));
					    }
				    }
			    }
		    }
		    
		    return new MeshData(vertices, triangles, transparentTriangles, uvs, chunk.CurrentGO);
	    }
    }
}