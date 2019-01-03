using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    private static List<Vector3> boxVertices = new List<Vector3>();
    private static List<int> boxTriangles = new List<int>();
    public List<Vector3> Vertices { get; set; }
    public List<int> Triangles { get; set; }

    public static List<Vector3> BoxVertices
    {
        get
        {
            if (boxVertices.Count == 0)
            {
                boxVertices = new List<Vector3>()
                {
                    new Vector3(0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, 0.5f),

                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f, -0.5f),

                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),

                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, -0.5f, -0.5f),

                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f, -0.5f),

                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(0.5f, -0.5f, 0.5f),
                };
            }
            return boxVertices;
        }
    }
    
    public static List<int> BoxTriangles
    {
        get
        {
            if (boxTriangles.Count == 0)
            {
                boxTriangles = new List<int>()
                {
                    0, 2, 3,
                    0, 3, 1,
                    8, 4, 5,
                    8, 5, 9,
                    10, 6, 7,
                    10, 7, 11,
                    12, 13, 14,
                    12, 14, 15,
                    16, 17, 18,
                    16, 18, 19,
                    20, 21, 22,
                    20, 22, 23
                };
            }

            return boxTriangles;
        }
    }

    public MeshData(List<Vector3> vertices, List<int> triangles)
    {
        Vertices = vertices;
        Triangles = triangles;
    }
}