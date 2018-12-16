using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUVs : MonoBehaviour
{
    [Tooltip("Quadratische Größe einer einzigen Textur")]
    public float pixelSize = 16;
    [Range(0, 15)]
    public float tileX = 1;
    [Range(0, 15)]
    public float tileY = 1;

    private static Vector2[] uvs = null;
    private static bool hasCalculatedUVs = false;

    private static Vector3[] vertices = null;
    private static int[] triangles = null;

    private void Start()
    {
        float tilePerc = 1f / pixelSize;

        float umin = tilePerc * tileX;
        float umax = tilePerc * (tileX + 1f);
        float vmin = tilePerc * tileY;
        float vmax = tilePerc * (tileY + 1f);

        Vector2[] blocksUVs = new Vector2[24];
        uvs = this.GetComponent<MeshFilter>().mesh.uv;

        for (int i = 0; i < uvs.Length; i++)
        {
            float x = Mathf.Approximately(uvs[i].x, 0f) ? umin : umax;
            float y = Mathf.Approximately(uvs[i].y, 0f) ? vmin : vmax;
            
            blocksUVs[i] = new Vector2(x, y);
        }

        this.GetComponent<MeshFilter>().mesh.uv = blocksUVs;
    }

    public static Vector2[] GetStandardUVs()
    {
        if (!hasCalculatedUVs)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uvs = temp.GetComponent<MeshFilter>().mesh.uv;
            Destroy(temp);
            hasCalculatedUVs = true;
            return uvs;
        }

        return uvs;

    }

    public static (Vector3[] vertices, int[] triangles) GetStandardMeshFilter()
    {
        if (vertices != null && triangles != null)
        {
            return (vertices, triangles);
        }
        
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vertices = temp.GetComponent<MeshFilter>().mesh.vertices;
        triangles = temp.GetComponent<MeshFilter>().mesh.triangles;
        Destroy(temp);

        return (vertices, triangles);
    }
}
