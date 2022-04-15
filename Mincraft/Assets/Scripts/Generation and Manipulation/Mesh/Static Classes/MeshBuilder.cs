using System.Collections.Generic;
using System.Linq;
using Core.Chunks;
using Core.Math;
using UnityEngine;

namespace Core.Builder
{
    public static class MeshBuilder
    {
        private static int chunkSize = 0x10;

        // In same order as "RenderingTechnique" enum
        private static IMeshAdder[] meshAdders =
        {
            new MeshBlockAdder(),
            new MeshGrassAdder()
        };


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
                            RenderingTechnique renderingTechnique = UVDictionary.RenderingTechnique(block.ID);
                            
                            MeshAdderParameter parameters = new MeshAdderParameter
                            {
                                Vertices = vertices,
                                Triangles = triangles,
                                TransparentTriangles = transparentTriangles,
                                Uvs = uvs,
                                CurrentUVData = currentUVData,
                                Block = block,
                                BoolNeighbours = boolNeighbours,
                                MeshOffset = meshOffset,
                                BlockPos = blockPos,
                                Transparent = transparent
                            };

                            if ((int)renderingTechnique >= 0 && (int)renderingTechnique < meshAdders.Length)
                                meshAdders[(int) renderingTechnique].AddMesh(parameters);
                        }
                    }
                }
            }

            return new MeshData(vertices, triangles, transparentTriangles, uvs, chunk.CurrentGO);
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
            RenderingTechnique renderingTechnique = UVDictionary.RenderingTechnique(block.ID);
            
            MeshAdderParameter parameters = new MeshAdderParameter
            {
                Vertices = vertices,
                Triangles = triangles,
                TransparentTriangles = transparentTriangles,
                Uvs = uvs,
                CurrentUVData = currentUVData,
                Block = block,
                BoolNeighbours = new []{false, false, false, false, false, false},
                MeshOffset = meshOffset,
                BlockPos = new Vector3(),
                Transparent = isTransparent
            };
                                
            meshAdders[(int) renderingTechnique].AddMesh(parameters);
            
            return new MeshData(vertices, triangles, transparentTriangles, uvs, normals);
        }
    }
}