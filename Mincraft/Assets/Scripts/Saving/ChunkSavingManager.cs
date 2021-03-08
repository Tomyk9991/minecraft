using System;
using System.IO;
using System.Text;
using Core.Chunks;
using Core.Managers;
using Core.Math;
using UnityEngine;

namespace Core.Saving
{
    public class ChunkSavingManager : SavingManager
    {
        public override void Save(SavingContext ctx)
        {
            Chunk context = (Chunk) ctx;
            
            ChunkData dataToSerialize = new ChunkData(context.Blocks.RawData);
            string chunkPath = Path.Combine(GameManager.CurrentWorldPath, context.GlobalPosition.ToString());
            File.WriteAllBytes(chunkPath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(dataToSerialize, false)));
        }

        public override bool Load(FileIdentifier fileIdentifier, out OutputContext chunk)
        {
            Int3 globalPos = ((ChunkFileIdentifier) fileIdentifier).globalPos;
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

    public struct ChunkFileIdentifier : FileIdentifier
    {
        public Int3 globalPos { get; set; }

        public ChunkFileIdentifier(Int3 globalPos)
        {
            this.globalPos = globalPos;
        }
    }
}
