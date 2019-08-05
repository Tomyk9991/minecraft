using System;
using System.Collections.Generic;
using UnityEngine;

public class GreedyMesh
{
    private int size;
    public GreedyMesh() => size = ChunkSettings.ChunkSize;
    public GreedyMesh(bool test) => size = 16;

    public MeshData ReduceMesh(Chunk chunk)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> elements = new List<int>();

        //Sweep over 3-axes
        for (int d = 0; d < 3; d++)
        {

            int i, j, k, l, w, h, u = (d + 1) % 3, v = (d + 2) % 3;

            int[] x = new int[3];
            int[] q = new int[3];
            int[] mask = new int[(size + 1) * (size + 1)];

            q[d] = 1;

            for (x[d] = -1; x[d] < size;)
            {

                // Compute the mask
                int n = 0;
                for (x[v] = 0; x[v] < size; ++x[v])
                {
                    for (x[u] = 0; x[u] < size; ++x[u], ++n)
                    {
                        int a = 0;
                        if (0 <= x[d])
                        {
                            a = chunk.IsNotEmpty(x[0], x[1], x[2]) == true ? 1 : 0;
                        }
                        int b = 0;
                        if (x[d] < size - 1)
                        {
                            b = chunk.IsNotEmpty(x[0] + q[0], x[1] + q[1], x[2] + q[2]) == true ? 1 : 0;
                        }
                        if (a != -1 && b != -1 && a == b)
                        {
                            mask[n] = 0;
                        }
                        else if (a > 0)
                        {
                            a = 1;
                            mask[n] = a;
                        }

                        else
                        {
                            b = 1;
                            mask[n] = -b;
                        }

                    }


                }

                // Increment x[d]
                ++x[d];

                // Generate mesh for mask using lexicographic ordering
                n = 0;
                for (j = 0; j < size; ++j)
                {
                    for (i = 0; i < size;)
                    {
                        var c = mask[n];
                        if (c > -3)
                        {
                            // Compute width
                            for (w = 1; c == mask[n + w] && i + w < size; ++w)
                                ;

                            // Compute height
                            bool done = false;
                            for (h = 1; j + h < size; ++h)
                            {
                                for (k = 0; k < w; ++k)
                                {
                                    if (c != mask[n + k + h * size])
                                    {
                                        done = true;
                                        break;
                                    }
                                }
                                if (done)
                                    break;
                            }
                            // Add quad
                            bool flip = false;
                            x[u] = i;
                            x[v] = j;
                            int[] du = new int[3];
                            int[] dv = new int[3];
                            if (c > -1)
                            {
                                du[u] = w;
                                dv[v] = h;
                            }
                            else
                            {
                                flip = true;
                                c = -c;
                                du[u] = w;
                                dv[v] = h;
                            }


                            Vector3 v1 = new Vector3(x[0], x[1], x[2]);
                            Vector3 v2 = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                            Vector3 v3 = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                            Vector3 v4 = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                            if (c > 0 && !flip)
                            {
                                AddFace(v1, v2, v3, v4, vertices, elements, 0);
                            }

                            if (flip)
                            {
                                AddFace(v4, v3, v2, v1, vertices, elements, 0);
                            }

                            // Zero-out mask
                            for (l = 0; l < h; ++l)
                                for (k = 0; k < w; ++k)
                                {
                                    mask[n + k + l * size] = 0;
                                }

                            // Increment counters and continue
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

        return new MeshData(vertices, elements, null, null);
    }
    private static void AddFace(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, List<Vector3> vertices, List<int> elements, int order)
    {
        if (order == 0)
        {
            int index = vertices.Count;

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            elements.Add(index);
            elements.Add(index + 1);
            elements.Add(index + 2);
            elements.Add(index + 2);
            elements.Add(index + 3);
            elements.Add(index);

        }

        if (order == 1)
        {
            int index = vertices.Count;

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            elements.Add(index);
            elements.Add(index + 3);
            elements.Add(index + 2);
            elements.Add(index + 2);
            elements.Add(index + 1);
            elements.Add(index);

        }
    }
}