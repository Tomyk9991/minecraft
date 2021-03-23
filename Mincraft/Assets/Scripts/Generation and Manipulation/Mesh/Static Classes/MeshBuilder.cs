using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Core.Chunks;
using Core.Math;
using Extensions;
using UnityEngine;

namespace Core.Builder
{
    public static class MeshBuilder
    {
        private static int[] forwardMasks = {0, 0, 0, 0, 1, 0};
        private static int[] backMasks = {0, 0, 0, 0, 1, 1};
        private static int[] sideWaysMasks = {0, 0, 0, 0, 0, 0};

        private static Vector3[] directions =
            {Vector3.forward, Vector3.zero, Vector3.up, Vector3.zero, Vector3.zero, Vector3.right};

        private static Vector3[] offset1 =
            {Vector3.right, Vector3.right, Vector3.right, Vector3.right, Vector3.forward, Vector3.forward};

        private static Vector3[] offset2 =
            {Vector3.up, Vector3.up, Vector3.forward, Vector3.forward, Vector3.up, Vector3.up};

        private static int[] tri1 = {1, 0, 2, 1, 2, 3};
        private static int[] tri2 = {0, 1, 2, 2, 1, 3};
        private static int[] tris = {1, 0, 0, 1, 1, 0};
        private static int chunkSize = 0x10;


        public static MeshData Combine(Chunk chunk)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<int> transparentTriangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            ExtendedArray3D<Block> blocks = chunk.Blocks;
            Block[] neighbourBlocks = new Block[6];
            bool[] boolNeighbours = new bool[6];

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        Block block = blocks[x, y, z];
                        Int3 pos = new Int3(x, y, z);

                        bool transparent = block.IsTransparent();

                        if (block.ID == BlockUV.Air || block.ID == BlockUV.None)
                        {
                            continue;
                        }

                        //Check, ob dieser Block transparent ist, oder nicht
                        // Wenn es so sein sollte, bleibt das neighbours-Array mit 6 false-Werten und jede Seite wird gezeichnet
                        neighbourBlocks = chunk.GetBlockNeighbours(pos);

                        if (!transparent)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                Block currentNeighbour = neighbourBlocks[j];
                                boolNeighbours[j] = currentNeighbour.ID != BlockUV.Air &&
                                                    currentNeighbour.ID != BlockUV.None &&
                                                    !currentNeighbour.IsTransparent();
                            }
                        }
                        else
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                boolNeighbours[j] = false;
                            }
                        }

                        Vector3 blockPos = pos.ToVector3();

                        if (boolNeighbours.Any(state => state == false))
                        {
                            UVData[] currentUVData = UVDictionary.GetValue(block.ID);
                            float meshOffset = UVDictionary.MeshOffsetID(block.ID);
                            bool isBlock = !UVDictionary.Is3DSprite(block.ID);
                            
                            if (isBlock)
                            {
                                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                                {
                                    if (boolNeighbours[faceIndex] == false)
                                    {
                                        int vc = vertices.Count;

                                        #region Meshoffsets

                                        Vector3 dir = directions[faceIndex];
                                        Vector3 off1 = offset1[faceIndex];
                                        Vector3 off2 = offset2[faceIndex];

                                        Vector3 meshOffsetForwardBack = new Vector3(0f, 0f, meshOffset);
                                        Vector3 meshOffsetLeftRight = new Vector3(meshOffset, 0f, 0f);

                                        switch (faceIndex)
                                        {
                                            case 0: //Forward
                                                vertices.Add(dir + blockPos - meshOffsetForwardBack);
                                                vertices.Add(dir + off1 + blockPos - meshOffsetForwardBack);
                                                vertices.Add(dir + off2 + blockPos - meshOffsetForwardBack);
                                                vertices.Add(dir + off1 + off2 + blockPos - meshOffsetForwardBack);
                                                break;
                                            case 1: //Back
                                                vertices.Add(dir + blockPos + meshOffsetForwardBack);
                                                vertices.Add(dir + off1 + blockPos + meshOffsetForwardBack);
                                                vertices.Add(dir + off2 + blockPos + meshOffsetForwardBack);
                                                vertices.Add(dir + off1 + off2 + blockPos + meshOffsetForwardBack);
                                                break;
                                            case 2: //Up
                                                vertices.Add(dir + blockPos);
                                                vertices.Add(dir + off1 + blockPos);
                                                vertices.Add(dir + off2 + blockPos);
                                                vertices.Add(dir + off1 + off2 + blockPos);
                                                break;
                                            case 3: // Down
                                                vertices.Add(dir + blockPos);
                                                vertices.Add(dir + off1 + blockPos);
                                                vertices.Add(dir + off2 + blockPos);
                                                vertices.Add(dir + off1 + off2 + blockPos);
                                                break;
                                            case 4: //Left
                                                vertices.Add(dir + blockPos + meshOffsetLeftRight);
                                                vertices.Add(dir + off1 + blockPos + meshOffsetLeftRight);
                                                vertices.Add(dir + off2 + blockPos + meshOffsetLeftRight);
                                                vertices.Add(dir + off1 + off2 + blockPos + meshOffsetLeftRight);
                                                break;
                                            case 5: //Right
                                                vertices.Add(dir + blockPos - meshOffsetLeftRight);
                                                vertices.Add(dir + off1 + blockPos - meshOffsetLeftRight);
                                                vertices.Add(dir + off2 + blockPos - meshOffsetLeftRight);
                                                vertices.Add(dir + off1 + off2 + blockPos - meshOffsetLeftRight);
                                                break;
                                        }

                                        #endregion


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
                                                transparentTriangles.Add(
                                                    vc + (tris[faceIndex] == 0 ? tri1[k] : tri2[k]));
                                            }
                                        }


                                        int orientedFaceIndex = block.Direction == BlockDirection.Forward
                                            ? faceIndex
                                            : CalculateOrientedFaceIndex(faceIndex, block.Direction);

                                        UVData uvdata = currentUVData[orientedFaceIndex];

                                        Vector2 zero = new Vector2(uvdata.TileX, uvdata.TileY);
                                        Vector2 one = new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY);
                                        Vector2 two = new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY);
                                        Vector2 three = new Vector2(uvdata.TileX + uvdata.SizeX,
                                            uvdata.TileY + uvdata.SizeY);

                                        #region Add UV

                                        switch (faceIndex)
                                        {
                                            case 2:
                                            case 3:
                                                switch (block.Direction)
                                                {
                                                    case BlockDirection.Forward:
                                                        uvs.Add(zero);
                                                        uvs.Add(one);
                                                        uvs.Add(two);
                                                        uvs.Add(three);
                                                        break;
                                                    case BlockDirection.Back:
                                                        uvs.Add(three);
                                                        uvs.Add(two);
                                                        uvs.Add(one);
                                                        uvs.Add(zero);
                                                        break;
                                                    case BlockDirection.Left:
                                                        uvs.Add(two);
                                                        uvs.Add(zero);
                                                        uvs.Add(three);
                                                        uvs.Add(one);
                                                        break;
                                                    case BlockDirection.Right:
                                                        uvs.Add(one);
                                                        uvs.Add(three);
                                                        uvs.Add(zero);
                                                        uvs.Add(two);
                                                        break;
                                                    default:
                                                        uvs.Add(zero);
                                                        uvs.Add(one);
                                                        uvs.Add(two);
                                                        uvs.Add(three);
                                                        break;
                                                }

                                                break;
                                            default:
                                                uvs.Add(zero);
                                                uvs.Add(one);
                                                uvs.Add(two);
                                                uvs.Add(three);
                                                break;
                                        }

                                        #endregion
                                    }
                                }
                            }
                            else
                            {
                                
                                //add variance
                                int vc = vertices.Count;
                                
                                vertices.Add(new Vector3(blockPos.x + 0.146447f, blockPos.y, blockPos.z + 0.146447f));
                                vertices.Add(new Vector3(blockPos.x + 0.853553f, blockPos.y, blockPos.z + 0.853553f));
                                vertices.Add(new Vector3(blockPos.x + 0.853553f, blockPos.y + 0.8f, blockPos.z + 0.853553f));
                                vertices.Add(new Vector3(blockPos.x + 0.146447f, blockPos.y + 0.8f, blockPos.z + 0.146447f));
                                
                                transparentTriangles.Add(vc + 0);
                                transparentTriangles.Add(vc + 1);
                                transparentTriangles.Add(vc + 3);
                                
                                transparentTriangles.Add(vc + 1);
                                transparentTriangles.Add(vc + 2); 
                                transparentTriangles.Add(vc + 3);

                                vc += 4;
                                
                                vertices.Add(new Vector3(blockPos.x + 0.146447f, blockPos.y, blockPos.z + 0.146447f));
                                vertices.Add(new Vector3(blockPos.x + 0.853553f, blockPos.y, blockPos.z + 0.853553f));
                                vertices.Add(new Vector3(blockPos.x + 0.853553f, blockPos.y + 0.8f, blockPos.z + 0.853553f));
                                vertices.Add(new Vector3(blockPos.x + 0.146447f, blockPos.y + 0.8f, blockPos.z + 0.146447f));
                                
                                transparentTriangles.Add(vc + 0);
                                transparentTriangles.Add(vc + 3);
                                transparentTriangles.Add(vc + 1);
                                
                                transparentTriangles.Add(vc + 1);
                                transparentTriangles.Add(vc + 3); 
                                transparentTriangles.Add(vc + 2);
                                
                                
                                UVData uvdata = currentUVData[0];

                                
                                uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY));
                                uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY));
                                uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY));
                                uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX,
                                    uvdata.TileY + uvdata.SizeY));
                                
                                uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY));
                                uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY));
                                uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY));
                                uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX,
                                    uvdata.TileY + uvdata.SizeY));
                            }
                        }
                    }
                }
            }

            return new MeshData(vertices, triangles, transparentTriangles, uvs, chunk.CurrentGO);
        }

        private static int CalculateOrientedFaceIndex(int actualDirection, BlockDirection direction)
        {
            if (actualDirection == (int) direction)
                return 0;

            if (direction == BlockDirection.Forward)
                return actualDirection;

            switch (actualDirection)
            {
                case 0: //Forward
                    return (int) direction ^ actualDirection ^ forwardMasks[(int) direction];
                case 1: // Back
                    return (int) direction ^ actualDirection ^ backMasks[(int) direction];
                case 4: // Left
                    return (int) direction ^ actualDirection ^ sideWaysMasks[(int) direction];
                case 5: // Right
                    return (int) direction ^ actualDirection ^ sideWaysMasks[(int) direction];
            }

            return actualDirection;
        }

        public static MeshData CombineBlock(Block block)
        {
            List<Vector3> vertices = new List<Vector3>();

            List<int> triangles = new List<int>();
            List<int> transparentTriangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();


            UVData[] currentUVData = UVDictionary.GetValue(block.ID);
            float meshOffset = UVDictionary.MeshOffsetID(block.ID);
            bool isTransparent = UVDictionary.IsTransparentID(block.ID);

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                int vc = vertices.Count;

                #region Meshoffsets

                Vector3 dir = directions[faceIndex];
                Vector3 off1 = offset1[faceIndex];
                Vector3 off2 = offset2[faceIndex];
                Vector3 blockPos = new Vector3(-0.5f, -0.5f, -0.5f);

                Vector3 meshOffsetForwardBack = new Vector3(0f, 0f, meshOffset);
                Vector3 meshOffsetLeftRight = new Vector3(meshOffset, 0f, 0f);

                switch (faceIndex)
                {
                    case 0: //Forward
                        vertices.Add(dir + blockPos - meshOffsetForwardBack);
                        vertices.Add(dir + off1 + blockPos - meshOffsetForwardBack);
                        vertices.Add(dir + off2 + blockPos - meshOffsetForwardBack);
                        vertices.Add(dir + off1 + off2 + blockPos - meshOffsetForwardBack);

                        normals.Add(Vector3.forward);
                        normals.Add(Vector3.forward);
                        normals.Add(Vector3.forward);
                        normals.Add(Vector3.forward);
                        break;
                    case 1: //Back
                        vertices.Add(dir + blockPos + meshOffsetForwardBack);
                        vertices.Add(dir + off1 + blockPos + meshOffsetForwardBack);
                        vertices.Add(dir + off2 + blockPos + meshOffsetForwardBack);
                        vertices.Add(dir + off1 + off2 + blockPos + meshOffsetForwardBack);

                        normals.Add(Vector3.back);
                        normals.Add(Vector3.back);
                        normals.Add(Vector3.back);
                        normals.Add(Vector3.back);
                        break;
                    case 2: //Up
                        vertices.Add(dir + blockPos);
                        vertices.Add(dir + off1 + blockPos);
                        vertices.Add(dir + off2 + blockPos);
                        vertices.Add(dir + off1 + off2 + blockPos);

                        normals.Add(Vector3.up);
                        normals.Add(Vector3.up);
                        normals.Add(Vector3.up);
                        normals.Add(Vector3.up);
                        break;
                    case 3: // Down
                        vertices.Add(dir + blockPos);
                        vertices.Add(dir + off1 + blockPos);
                        vertices.Add(dir + off2 + blockPos);
                        vertices.Add(dir + off1 + off2 + blockPos);

                        normals.Add(Vector3.down);
                        normals.Add(Vector3.down);
                        normals.Add(Vector3.down);
                        normals.Add(Vector3.down);
                        break;
                    case 4: //Left
                        vertices.Add(dir + blockPos + meshOffsetLeftRight);
                        vertices.Add(dir + off1 + blockPos + meshOffsetLeftRight);
                        vertices.Add(dir + off2 + blockPos + meshOffsetLeftRight);
                        vertices.Add(dir + off1 + off2 + blockPos + meshOffsetLeftRight);

                        normals.Add(Vector3.left);
                        normals.Add(Vector3.left);
                        normals.Add(Vector3.left);
                        normals.Add(Vector3.left);
                        break;
                    case 5: //Right
                        vertices.Add(dir + blockPos - meshOffsetLeftRight);
                        vertices.Add(dir + off1 + blockPos - meshOffsetLeftRight);
                        vertices.Add(dir + off2 + blockPos - meshOffsetLeftRight);
                        vertices.Add(dir + off1 + off2 + blockPos - meshOffsetLeftRight);

                        normals.Add(Vector3.right);
                        normals.Add(Vector3.right);
                        normals.Add(Vector3.right);
                        normals.Add(Vector3.right);
                        break;
                }

                #endregion

                if (!isTransparent)
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


                //UVS
                UVData uvdata = currentUVData[faceIndex];
                uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY));
                uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY));
                uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY));
                uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY + uvdata.SizeY));
            }

            return new MeshData(vertices, triangles, transparentTriangles, uvs, normals);
        }
    }
}