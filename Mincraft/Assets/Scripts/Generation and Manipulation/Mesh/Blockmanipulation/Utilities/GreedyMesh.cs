using System.Collections.Generic;
using Core.Chunks;
using UnityEngine;

namespace Core.Builder
{
    public static class GreedyMesh
    {
        public static MeshData ReduceMesh(Chunk chunk)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            const int dim = 16;

            //Sweep over 3-axes
            for (int d = 0; d < 3; d++)
            {
                int i, j, k, l, w, h;

                int u = (d + 1) % 3;
                int v = (d + 2) % 3;

                int[] x = {0, 0, 0};
                int[] q = {0, 0, 0};
                int[] mask = new int[(dim + 1) * (dim + 1)];


                q[d] = 1;

                for (x[d] = -1; x[d] < dim;)
                {
                    // Compute the mask
                    int n = 0;
                    for (x[v] = 0; x[v] < dim; ++x[v])
                    {
                        for (x[u] = 0; x[u] < dim; ++x[u], ++n)
                        {
                            int vox1 = chunk.IsNotEmpty(x[0], x[1], x[2]) ? 1 : 0;
                            int vox2 = chunk.IsNotEmpty(x[0] + q[0], x[1] + q[1], x[2] + q[2]) ? 1 : 0;

                            int a = (0 <= x[d] ? vox1 : 0);
                            int b = (x[d] < dim - 1 ? vox2 : 0);

                            if ((a != 0) == (b != 0))
                            {
                                mask[n] = 0;
                            }
                            else if ((a != 0))
                            {
                                mask[n] = a;
                            }
                            else
                            {
                                mask[n] = -b;
                            }
                        }
                    }

                    // Increment x[d]
                    ++x[d];

                    // Generate mesh for mask using lexicographic ordering
                    n = 0;
                    for (j = 0; j < dim; ++j)
                    {
                        for (i = 0; i < dim;)
                        {
                            var c = mask[n];

                            if (c != 0)
                            {
                                // compute width
                                for (w = 1; mask[n + w] == c && (i + w) < dim; ++w)
                                {
                                }

                                // compute height
                                bool done = false;
                                for (h = 1; (j + h) < dim; ++h)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        if (mask[n + k + h * dim] != c)
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

                                // add quad
                                x[u] = i;
                                x[v] = j;

                                int[] du = {0, 0, 0};
                                int[] dv = {0, 0, 0};

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

                                AddQuad(v1, v2, v3, v4, vertices, triangles);

                                for (l = 0; l < h; ++l)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        mask[n + k + l * dim] = 0;
                                    }
                                }

                                // increment counters
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

            MeshData data = new MeshData(vertices, triangles, null, null, null);

            return data;
        }

        private static void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, List<Vector3> vertices, List<int> triangles)
        {
            int i = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            triangles.Add(i + 0);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
            triangles.Add(i + 2);
            triangles.Add(i + 3);
            triangles.Add(i + 0);
        }
    }
}