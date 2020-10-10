using System;
using System.IO;
using System.Text;
using Core.Chunks;
using Core.Managers;
using Core.Math;
using UnityEngine;

namespace Core.Saving
{
    public class ChunkSavingManager : MonoBehaviour
    {
        public static void Save(Chunk context)
        {
            ChunkData dataToSerialize = new ChunkData(context.Blocks.RawData);
            string chunkPath = Path.Combine(GameManager.CurrentWorldPath, context.GlobalPosition.ToString());
            File.WriteAllBytes(chunkPath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(dataToSerialize, false)));
        }
        
        public static bool Load(Int3 globalPos, out ChunkData chunk)
        {
            string chunkPath = Path.Combine(GameManager.CurrentWorldPath, globalPos.ToString());
            if (File.Exists(chunkPath))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(chunkPath);
                }
                catch (Exception)
                {
                    Debug.Log($"Could not load {globalPos.ToString()} from {chunkPath}");
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
