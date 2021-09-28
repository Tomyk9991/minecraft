using System;
using System.IO;
using Core.Builder;
using Core.Chunks;
using Core.Saving;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Testing
{    
    public class ChunkLoadTest : MonoBehaviour
    {
        [SerializeField] private Object obj = null;
        [SerializeField] private GameObject chunkGameObject = null;
        
        private void Start()
        {
            Load(out ChunkData data);
            
            Chunk chunk = new Chunk()
            {
                Blocks = new ExtendedArray3D<Block>(data.Blocks),
                CurrentGO = chunkGameObject
            };
            
            Debug.Log(data.Blocks.Length);
            
            chunkGameObject.GetComponent<ChunkReferenceHolder>().Chunk = chunk;
            
            //MeshData meshData = MeshBuilder.Combine(chunk);
            CreateChunk(chunk);
            //MeshModifier.SetMeshTest(chunkGameObject, meshData, new MeshData());
        }

        private void CreateChunk(Chunk chunk)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (chunk.Blocks[x, y, z].ID != BlockUV.Air)
                        {
                            Instantiate(g, new Vector3(x, y, z), Quaternion.identity, transform);
                        }
                    }
                }
            }
        }
        
        public bool Load(out ChunkData chunk)
        {
            string chunkPath = "";
#if UNITY_EDITOR
            chunkPath = Path.Combine(AssetDatabase.GetAssetPath(obj));
#endif
            if (File.Exists(chunkPath))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(chunkPath);
                }
                catch (Exception)
                {
                    Debug.Log($"Could not load Testchunk from {chunkPath}");
                }
            
                if (json != "")
                {
                    try
                    {
                        chunk = JsonUtility.FromJson<ChunkData>(json);
                        return true;
                    }
                    catch (Exception)
                    {
                        Debug.Log("Formatting went wrong");
                    }
                }
            }
        
            chunk = null;
            return false;
        }
    }
}
