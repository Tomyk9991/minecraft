using System;
using System.IO;
using System.Linq;
using Core.Builder;
using GateLogic.Impl;
using UnityEditor;
using UnityEngine;

namespace Core.Testing
{
    public class CreateTexturedCubeTest : MonoBehaviour
    {
        [SerializeField] private int width = 128;
        [SerializeField] private int height = 128;
    
        private Quaternion standardRotation = Quaternion.identity;
        private Quaternion inverseRotation = Quaternion.identity;
        
        public void Start()
        {
            standardRotation = transform.rotation;
            inverseRotation = Quaternion.Inverse(Quaternion.Inverse(standardRotation));
            
            foreach(BlockUV uv in Enum.GetValues(typeof(BlockUV)))
            {
                if (uv == BlockUV.Air || uv == BlockUV.None) continue;
                CreateChunkWith(uv);
            }
        }
    
        private void CreateChunkWith(BlockUV block)
        {
            MeshData data = MeshBuilder.CombineBlock(new Block(block));
            
            Mesh mesh = new Mesh();
            
            GetComponent<MeshFilter>().mesh = mesh;

            transform.rotation = DigitalCircuitManager.CircuitBlocks.Contains(block) ? inverseRotation : standardRotation;
            
            mesh.subMeshCount = 2;
            mesh.SetVertices(data.Vertices);
            mesh.SetTriangles(data.Triangles, 0);
            mesh.SetTriangles(data.TransparentTriangles, 1);
            mesh.SetNormals(data.Normals);
            mesh.SetUVs(0, data.UVs);

            byte[] itemBytes = CreateUnitIcon().texture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + $"/Imports/Sprites/UI/Inventory/InventorySprites/Inventory{block.ToString()}.png", itemBytes);
    
    #if UNITY_EDITOR
            AssetDatabase.Refresh();
    #endif
        }
        
        private Sprite CreateUnitIcon()
        {
            Camera c = FindObjectOfType<Camera>();
    
            c.backgroundColor = Color.magenta;
            c.clearFlags = CameraClearFlags.Color;
    
    
            RenderTexture renderTexture = new RenderTexture(width, height, 24)
            {
                anisoLevel = 4, 
                antiAliasing = 2
            };
    
            c.targetTexture = renderTexture;
            c.Render();
    
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            RenderTexture.active = null;
            c.targetTexture = null;


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (texture.GetPixel(x, y) == Color.magenta)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            
            texture.Apply();
    
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
    }
}