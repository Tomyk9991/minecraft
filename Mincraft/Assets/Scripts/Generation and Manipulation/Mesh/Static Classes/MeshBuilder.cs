using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Core.Chunking;

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

//        private static Int3[] directions =
//{
//            Int3.Forward, // 0
//            Int3.Back, // 1
//            Int3.Up, // 2
//            Int3.Down, // 3
//            Int3.Left, // 4
//            Int3.Right // 5
//        };


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
            List<Color> colors = new List<Color>();

            Block[] neighbourBlocks = new Block[6];
            bool[] boolNeighbours = new bool[6];

		    for (int i = 0; i < blocks.Length; i++)
	        {
	            bool transparent = blocks[i].IsTransparent();

                Block block = blocks[i];
			    if (block.ID == (int) BlockUV.Air)
			    {
				    continue;
			    }

                //Check, ob dieser Block transparent ist, oder nicht
                // Wenn es so sein sollte, bleibt das neighbours-Array mit 6 false-Werten und jede Seite wird gezeichnet

                neighbourBlocks = chunk.Neighbours(block.Position);
                
                if (!transparent)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        boolNeighbours[j] = !(neighbourBlocks[j].ID == (int)BlockUV.Air || neighbourBlocks[j].IsTransparent());
                    }
                }
                else
                {
                    for (int j = 0; j < 6; j++)
                    {
                        boolNeighbours[j] = false;
                    }
                }

                Vector3 blockPos = block.Position.ToVector3();
			    
			    if (boolNeighbours.Any(state => state == false))
			    {
				    UVData[] currentUVData = UVDictionary.GetValue((BlockUV) block.ID);
                    float meshOffset = UVDictionary.MeshOffsetID((BlockUV)block.ID);

				    for (int faceIndex = 0; faceIndex < 6; faceIndex++)
				    {
					    if (boolNeighbours[faceIndex] == false)
					    {
						    int vc = vertices.Count;


                            #region Meshoffsets
                            switch (faceIndex)
                            {
                                case 0: //Forward
                                    vertices.Add(directions[faceIndex] + blockPos - new Vector3(0, 0f, meshOffset));
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + blockPos - new Vector3(0, 0f, meshOffset));
                                    vertices.Add(directions[faceIndex] + offset2[faceIndex] + blockPos - new Vector3(0, 0f, meshOffset));
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + offset2[faceIndex] + blockPos - new Vector3(0, 0f, meshOffset));
                                    break;
                                case 1: //Back
                                    vertices.Add(directions[faceIndex] + blockPos + new Vector3(0, 0f, meshOffset));
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + blockPos + new Vector3(0, 0f, meshOffset));
                                    vertices.Add(directions[faceIndex] + offset2[faceIndex] + blockPos + new Vector3(0, 0f, meshOffset));
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + offset2[faceIndex] + blockPos + new Vector3(0, 0f, meshOffset));
                                    break;
                                case 2: //Up
                                    vertices.Add(directions[faceIndex] + blockPos);
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + blockPos);
                                    vertices.Add(directions[faceIndex] + offset2[faceIndex] + blockPos);
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + offset2[faceIndex] + blockPos);
                                    break;
                                case 3: // Down
                                    vertices.Add(directions[faceIndex] + blockPos);
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + blockPos);
                                    vertices.Add(directions[faceIndex] + offset2[faceIndex] + blockPos);
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + offset2[faceIndex] + blockPos);
                                    break;
                                case 4: //Left
                                    vertices.Add(directions[faceIndex] + blockPos + new Vector3(meshOffset, 0f, 0f));
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + blockPos + new Vector3(meshOffset, 0f, 0f));
                                    vertices.Add(directions[faceIndex] + offset2[faceIndex] + blockPos + new Vector3(meshOffset, 0f, 0f));
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + offset2[faceIndex] + blockPos + new Vector3(meshOffset, 0f, 0f));
                                    break;
                                case 5: //Right
                                    vertices.Add(directions[faceIndex] + blockPos - new Vector3(meshOffset, 0f, 0f));
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + blockPos - new Vector3(meshOffset, 0f, 0f));
                                    vertices.Add(directions[faceIndex] + offset2[faceIndex] + blockPos - new Vector3(meshOffset, 0f, 0f));
                                    vertices.Add(directions[faceIndex] + offset1[faceIndex] + offset2[faceIndex] + blockPos - new Vector3(meshOffset, 0f, 0f));
                                    break;
                            }
                            #endregion

                            Block neighbour = neighbourBlocks[faceIndex];
                            float lightLevel = neighbour.GlobalLightPercent;

                            colors.Add(new Color(0, 0, 0, lightLevel));
                            colors.Add(new Color(0, 0, 0, lightLevel));
                            colors.Add(new Color(0, 0, 0, lightLevel));
                            colors.Add(new Color(0, 0, 0, lightLevel));

                            if (!transparent)
	                        {
						        for (int k = 0; k < 6; k++)
						        {
							        triangles.Add(vc + (tris[faceIndex] == 0 ? tri1[k] : tri2[k]));
						        }
	                        }
	                        else
	                        {
	                            for (int k = 0; k < 6; k++)
	                            {
	                                transparentTriangles.Add(vc + (tris[faceIndex] == 0 ? tri1[k] : tri2[k]));
	                            }
	                        }
		
						    uvs.Add(new Vector2(currentUVData[faceIndex].TileX, currentUVData[faceIndex].TileY));
						    uvs.Add(new Vector2(currentUVData[faceIndex].TileX + currentUVData[faceIndex].SizeX, currentUVData[faceIndex].TileY));
						    uvs.Add(new Vector2(currentUVData[faceIndex].TileX, currentUVData[faceIndex].TileY + currentUVData[faceIndex].SizeY));
						    uvs.Add(new Vector2(currentUVData[faceIndex].TileX + currentUVData[faceIndex].SizeX,currentUVData[faceIndex].TileY + currentUVData[faceIndex].SizeY));
					    }
				    }
			    }
		    }
		    
		    return new MeshData(vertices, triangles, transparentTriangles, uvs, colors, chunk.CurrentGO);
	    }
    }
}
