using System.Collections.Generic;
using UnityEngine;

namespace Core.Builder
{
    public class MeshBlockAdder : IMeshAdder
    {
        private static Vector3[] directions =
            {Vector3.forward, Vector3.zero, Vector3.up, Vector3.zero, Vector3.zero, Vector3.right};

        private static Vector3[] offset1 =
            {Vector3.right, Vector3.right, Vector3.right, Vector3.right, Vector3.forward, Vector3.forward};

        private static Vector3[] offset2 =
            {Vector3.up, Vector3.up, Vector3.forward, Vector3.forward, Vector3.up, Vector3.up};

        private static int[] tri1 = {1, 0, 2, 1, 2, 3};
        private static int[] tri2 = {0, 1, 2, 2, 1, 3};
        private static int[] tris = {1, 0, 0, 1, 1, 0};
        
        private static int[] forwardMasks = {0, 0, 0, 0, 1, 0};
        private static int[] backMasks = {0, 0, 0, 0, 1, 1};
        private static int[] sideWaysMasks = {0, 0, 0, 0, 0, 0};
        
        public void AddMesh(MeshAdderParameter parameters)
        {
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                if (parameters.BoolNeighbours[faceIndex] == false)
                {
                    int vc = parameters.Vertices.Count;

                    #region Meshoffsets

                    Vector3 dir = directions[faceIndex];
                    Vector3 off1 = offset1[faceIndex];
                    Vector3 off2 = offset2[faceIndex];

                    Vector3 meshOffsetForwardBack = new Vector3(0f, 0f, parameters.MeshOffset);
                    Vector3 meshOffsetLeftRight = new Vector3(parameters.MeshOffset, 0f, 0f);

                    switch (faceIndex)
                    {
                        case 0: //Forward
                            parameters.Vertices.Add(dir + parameters.BlockPos - meshOffsetForwardBack);
                            parameters.Vertices.Add(dir + off1 + parameters.BlockPos - meshOffsetForwardBack);
                            parameters.Vertices.Add(dir + off2 + parameters.BlockPos - meshOffsetForwardBack);
                            parameters.Vertices.Add(dir + off1 + off2 + parameters.BlockPos - meshOffsetForwardBack);
                            break;
                        case 1: //Back
                            parameters.Vertices.Add(dir + parameters.BlockPos + meshOffsetForwardBack);
                            parameters.Vertices.Add(dir + off1 + parameters.BlockPos + meshOffsetForwardBack);
                            parameters.Vertices.Add(dir + off2 + parameters.BlockPos + meshOffsetForwardBack);
                            parameters.Vertices.Add(dir + off1 + off2 + parameters.BlockPos + meshOffsetForwardBack);
                            break;
                        case 2: //Up
                            parameters.Vertices.Add(dir + parameters.BlockPos);
                            parameters.Vertices.Add(dir + off1 + parameters.BlockPos);
                            parameters.Vertices.Add(dir + off2 + parameters.BlockPos);
                            parameters.Vertices.Add(dir + off1 + off2 + parameters.BlockPos);
                            break;
                        case 3: // Down
                            parameters.Vertices.Add(dir + parameters.BlockPos);
                            parameters.Vertices.Add(dir + off1 + parameters.BlockPos);
                            parameters.Vertices.Add(dir + off2 + parameters.BlockPos);
                            parameters.Vertices.Add(dir + off1 + off2 + parameters.BlockPos);
                            break;
                        case 4: //Left
                            parameters.Vertices.Add(dir + parameters.BlockPos + meshOffsetLeftRight);
                            parameters.Vertices.Add(dir + off1 + parameters.BlockPos + meshOffsetLeftRight);
                            parameters.Vertices.Add(dir + off2 + parameters.BlockPos + meshOffsetLeftRight);
                            parameters.Vertices.Add(dir + off1 + off2 + parameters.BlockPos + meshOffsetLeftRight);
                            break;
                        case 5: //Right
                            parameters.Vertices.Add(dir + parameters.BlockPos - meshOffsetLeftRight);
                            parameters.Vertices.Add(dir + off1 + parameters.BlockPos - meshOffsetLeftRight);
                            parameters.Vertices.Add(dir + off2 + parameters.BlockPos - meshOffsetLeftRight);
                            parameters.Vertices.Add(dir + off1 + off2 + parameters.BlockPos - meshOffsetLeftRight);
                            break;
                    }

                    #endregion


                    if (!parameters.Transparent)
                    {
                        for (int k = 0; k < 6; k++)
                        {
                            parameters.Triangles.Add(vc + (tris[faceIndex] == 0 ? tri1[k] : tri2[k]));
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 6; k++)
                        {
                            parameters.TransparentTriangles.Add(
                                vc + (tris[faceIndex] == 0 ? tri1[k] : tri2[k]));
                        }
                    }


                    int orientedFaceIndex = parameters.Block.Direction == BlockDirection.Forward
                        ? faceIndex
                        : CalculateOrientedFaceIndex(faceIndex, parameters.Block.Direction);

                    UVData uvdata = parameters.CurrentUVData[orientedFaceIndex];

                    Vector2 zero = new Vector2(uvdata.TileX, uvdata.TileY);
                    Vector2 one = new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY);
                    Vector2 two = new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY);
                    Vector2 three = new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY + uvdata.SizeY);

                    #region Add UV

                    switch (faceIndex)
                    {
                        case 2:
                        case 3:
                            switch (parameters.Block.Direction)
                            {
                                case BlockDirection.Forward:
                                    parameters.Uvs.Add(zero);
                                    parameters.Uvs.Add(one);
                                    parameters.Uvs.Add(two);
                                    parameters.Uvs.Add(three);
                                    break;
                                case BlockDirection.Back:
                                    parameters.Uvs.Add(three);
                                    parameters.Uvs.Add(two);
                                    parameters.Uvs.Add(one);
                                    parameters.Uvs.Add(zero);
                                    break;
                                case BlockDirection.Left:
                                    parameters.Uvs.Add(two);
                                    parameters.Uvs.Add(zero);
                                    parameters.Uvs.Add(three);
                                    parameters.Uvs.Add(one);
                                    break;
                                case BlockDirection.Right:
                                    parameters.Uvs.Add(one);
                                    parameters.Uvs.Add(three);
                                    parameters.Uvs.Add(zero);
                                    parameters.Uvs.Add(two);
                                    break;
                                default:
                                    parameters.Uvs.Add(zero);
                                    parameters.Uvs.Add(one);
                                    parameters.Uvs.Add(two);
                                    parameters.Uvs.Add(three);
                                    break;
                            }

                            break;
                        default:
                            parameters.Uvs.Add(zero);
                            parameters.Uvs.Add(one);
                            parameters.Uvs.Add(two);
                            parameters.Uvs.Add(three);
                            break;
                    }

                    #endregion
                }
            }
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
    }
}