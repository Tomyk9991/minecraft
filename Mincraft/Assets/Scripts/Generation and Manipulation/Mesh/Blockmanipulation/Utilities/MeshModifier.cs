using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MeshModifier
{
    private static Vector3[] directions = {Vector3.forward, Vector3.zero, Vector3.up, Vector3.zero, Vector3.zero, Vector3.right};
    private static Vector3[] offset1 = {Vector3.right, Vector3.right, Vector3.right, Vector3.right, Vector3.forward, Vector3.forward};
    private static Vector3[] offset2 = {Vector3.up, Vector3.up, Vector3.forward, Vector3.forward, Vector3.up, Vector3.up};
    private static int[] tri1 = {1, 0, 2, 1, 2, 3};
    private static int[] tri2 = { 0, 1, 2, 2, 1, 3 };
    private static int[] tris = { 1, 0, 0, 1, 1, 0 };

    public event EventHandler<MeshData> MeshAvailable;
    
    public Task Combine(IChunk chunk)
    {
        return Task.Run(() =>
        {
            List<Block> blocks = chunk.GetBlocks();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].RecalculateNeighbours();
		
                if (blocks[i].Neighbours.Any(state => state == false))
                {
                    var currentUVData = UVDictionary.GetValue((BlockUV) blocks[i].ID);

                    for (int j = 0; j < blocks[i].Neighbours.Length; j++)
                    {
                        if (blocks[i].Neighbours[j] == false)
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
            MeshAvailable?.Invoke(this, new MeshData(vertices, triangles, uvs, chunk.CurrentGO));
        });
//        return new MeshData(vertices, triangles, uvs);
    }
}
