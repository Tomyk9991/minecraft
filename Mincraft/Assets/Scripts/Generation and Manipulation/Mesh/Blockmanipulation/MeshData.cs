using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    private static List<Vector3> boxVertices = new List<Vector3>();
    private static List<int> boxTriangles = new List<int>();
    
    private static Vector2[] uvs;
    private static bool hasCalculatedUVs = false;
    
    public List<Vector3> Vertices { get; set; }
    public List<int> Triangles { get; set; }
    public List<Vector2> UVs { get; set; }

    public static List<Vector3> BoxVertices
    {
        get
        {
            if (boxVertices.Count == 0)
            {
                boxVertices = new List<Vector3>()
                {
                    
                    //Vorne
                    new Vector3(0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, 0.5f),

                    //Hinten
                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f, -0.5f),

                    //Oben
                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),

                    //Unten
                    new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, -0.5f, -0.5f),

                    //Links
                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f, -0.5f),

                    //Rechts
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
                    //Vorne
                    0, 2, 3,
                    0, 3, 1,
                    //Hinten
                    8, 4, 5,
                    8, 5, 9,
                    //Oben
                    10, 6, 7,
                    10, 7, 11,
                    //Unten
                    12, 13, 14,
                    12, 14, 15,
                    //Links
                    16, 17, 18,
                    16, 18, 19,
                    //rechts
                    20, 21, 22,
                    20, 22, 23
                };
            }

            return boxTriangles;
        }
    }
    
    public static Vector2[] BoxUVs
    {
        get
        {            
            if (!hasCalculatedUVs)
            {
                hasCalculatedUVs = true;
    
                uvs = new[]
                {
                    //Vorne
                    new Vector2(0.0f, 0.0f),
                    new Vector2(1.0f, 0.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 1.0f),
                    //Hinten
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 1.0f),
                    //Oben
                    new Vector2(0.0f, 0.0f),
                    new Vector2(1.0f, 0.0f),
                    new Vector2(0.0f, 0.0f),
                    new Vector2(1.0f, 0.0f),
                    //Unten
                    new Vector2(0.0f, 0.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, 0.0f),
                    //Links
                    new Vector2(0.0f, 0.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, 0.0f),
                    //Rechts
                    new Vector2(0.0f, 0.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, 0.0f),
                };
                return uvs;
            }
    
            return uvs;
        }
    }

    public MeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Vertices = vertices;
        Triangles = triangles;
        UVs = uvs;
    }
}