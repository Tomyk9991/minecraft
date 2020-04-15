using System;
using System.IO;
using Core.Builder;
using Core.Chunking;
using Core.Math;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateTexturedCubeTest : MonoBehaviour
{
    [SerializeField] private GameObject cube = null;
    [SerializeField] private int width, height = 128;
    
            

    public void Start()
    {
        foreach(BlockUV uv in Enum.GetValues(typeof(BlockUV)))
        {
            if (uv == BlockUV.Air || uv == BlockUV.Cactus) continue;
            CreateChunkWith(uv);
        }
    }

    private void CreateChunkWith(BlockUV uv)
    {
        Chunk chunk = new Chunk("Test");
        Block b = new Block()
        {
            GlobalLightPercent = 1f,
            ID = (short) uv
        };
        
        Int3 pos = new Int3(0, 0, 0);

        chunk.AddBlock(b, pos);

        MeshData data = MeshBuilder.TestCombine(chunk);
        
        for (int i = 0; i < data.Vertices.Count; i++)
        {
            data.Vertices[i] -= new Vector3(0.5f, 0.5f, 0.5f);
        }
        
        cube.GetComponent<MeshFilter>().mesh = new Mesh()
        {
            indexFormat = IndexFormat.UInt32,
            vertices = data.Vertices.ToArray(),
            triangles = data.Triangles.ToArray(),
            uv = data.UVs.ToArray()
        };



        byte[] itemBytes = CreateUnitIcon().texture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + $"/Imports/Sprites/InventorySprites/Inventory{uv.ToString()}.png", itemBytes);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }


    private Sprite CreateUnitIcon()
    {
        Camera c = FindObjectOfType<Camera>();
        c.backgroundColor = new Color(0f, 0f, 0f, 0f);
        c.clearFlags = CameraClearFlags.Color;


        RenderTexture renderTexture = new RenderTexture(width, height, 24)
        {
            anisoLevel = 4, 
            antiAliasing = 2
        };
        c.targetTexture = renderTexture;
        c.Render();

        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        RenderTexture.active = null;
        c.targetTexture = null;
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }
}
