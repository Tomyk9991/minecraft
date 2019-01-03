using System;
using System.Collections.Generic;
using UnityEngine;

public class GreedyMesh
{
    private Vector3Int size;
    public GreedyMesh() => size = ChunkManager.GetMaxSize;
    
    public MeshData ReduceMesh(IGreedyChunk chunk)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> elements = new List<int>();

        Vector3Int size = ChunkManager.GetMaxSize;

        
        int[] dims = { size.x, size.y, size.z };
        for (int d = 0; d < 3; d++)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            int l = 0;
            int w = 0;
            int h = 0;

            int u = (d + 1) % 3;
            int v = (d + 2) % 3;

            int[] x = { 0, 0, 0 };
            int[] q = { 0, 0, 0 };
            int[] mask = new int[(dims[u] + 1) * (dims[v] + 1)];


            q[d] = 1;

            for (x[d] = -1; x[d] < dims[d];)
            {
                // Compute the mask
                int n = 0;
                for (x[v] = 0; x[v] < dims[v]; ++x[v])
                {
                    for (x[u] = 0; x[u] < dims[u]; ++x[u], ++n)
                    {
                        int vox1 = (int)chunk.GetField(x[0], x[1], x[2]);
                        int vox2 = (int)chunk.GetField(x[0] + q[0], x[1] + q[1], x[2] + q[2]);

                        int a = (0 <= x[d] ? vox1 : 0);
                        int b = (x[d] < dims[d] - 1 ? vox2 : 0);

                        if ((a != 0) == (b != 0))
                        {
                            mask[n] = 0;
                        }
                        else if ((a !=0))
                        {
                            mask[n] = a;
                        }
                        else
                        {
                            mask[n] = -b;
                        }
                    }
                }
                
                ++x[d];

                n = 0;
                for (j = 0; j < dims[v]; ++j)
                {
                    for (i = 0; i < dims[u];)
                    {
                        var c = mask[n];

                        if (c != 0)
                        {
                            for (w = 1; mask[n + w] == c && (i + w) < dims[u]; ++w) { }

                            bool done = false;
                            for (h = 1; (j + h) < dims[v]; ++h)
                            {
                                for (k = 0; k < w; ++k)
                                {
                                    if (mask[n + k + h * dims[u]] != c)
                                    {
                                        done = true;
                                        break;
                                    }
                                }
                                if (done)
                                {
                                    break;
                                }
                            }

                            x[u] = i;
                            x[v] = j;

                            int[] du = { 0, 0, 0 };
                            int[] dv = { 0, 0, 0 };

                            if (c > 0)
                            {
                                dv[v] = h;
                                du[u] = w;
                            }
                            else
                            {
                                du[v] = h;
                                dv[u] = w;
                            }

                            Vector3 v1 = new Vector3(x[0], x[1], x[2]);
                            Vector3 v2 = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                            Vector3 v3 = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                            Vector3 v4 = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                            AddQuad(v1, v2, v3, v4, vertices, elements);

                            for (l = 0; l < h; ++l)
                            {
                                for (k = 0; k < w; ++k)
                                {
                                    mask[n + k + l * dims[u]] = 0;
                                }
                            }
                            i += w;
                            n += w;
                        }
                        else
                        {
                            ++i;
                            ++n;
                        }
                    }
                }
            }
        }

        MeshData data = new MeshData(vertices, elements);

        return data;
    }

    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, List<Vector3> vertices, List<int> elements)
    {
        int i = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

        elements.Add(i + 0);
        elements.Add(i + 1);
        elements.Add(i + 2);
        elements.Add(i + 2);
        elements.Add(i + 3);
        elements.Add(i + 0);
    }
}

public interface IGreedyChunk
{
    double GetField(int x, int y, int z);
//    void SetNeighbour(IGreedyChunk chunk, Direction direction);
//    IGreedyChunk GetNeighbour(Direction direction);
    
    IGreedyChunk[] neighbors { get; set; }
    Field field { get; set; }
}

public class Field
{
    int x, y, z;

    public int X { get { return x; } }
    public int Y { get { return y; } }
    public int Z { get { return z; } }

    private double[] field = null;

    public Field(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;

        if (x == 0 && y == 0 && z == 0)
            Debug.LogError("Field has a size of (0, 0, 0)");
        
        field = new double[x * y * z];
    }

    public void Set(int x, int y, int z, double v)
    {
        field[GetIndex(x, y, z)] = v;
    }

    public double Get(int x, int y, int z)
    {
        if (x >= this.x) x = this.x - 1;
        if (x < 0) x = 0;
        if (y >= this.y) y = this.y - 1;
        if (y < 0) y = 0;
        if (z >= this.z) z = this.z - 1;
        if (z < 0) z = 0;

        return field[GetIndex(x, y, z)];
    }

    private int GetIndex(int x, int y, int z)
    {
        return (y * this.x * this.z) + (z * this.x) + x;
    }
}