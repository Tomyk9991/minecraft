using System.Collections;
using System.Collections.Generic;
using Core.Builder;
using UnityEngine;

namespace Core.Player.Interaction
{
    public class DroppedItemInformation : MonoBehaviour
    {
        public bool IsBlock { get; private set; }
        public Block Block { get; set; }
        
        public void FromBlock(Block block)
        {
            this.IsBlock = true;
            this.Block = block;
            Mesh mesh = transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            
            Vector2[] uvs = new Vector2[24];
            UVData[] currentUVData = UVDictionary.GetValue(block.ID);
            
            for (int i = 0; i < 24; i += 4)
            {
                UVData uvData = currentUVData[i / 4];
                uvs[i + 0] = new Vector2(uvData.TileX, uvData.TileY);
                uvs[i + 1] = new Vector2(uvData.TileX + uvData.SizeX, uvData.TileY);
                uvs[i + 2] = new Vector2(uvData.TileX, uvData.TileY + uvData.SizeY);
                uvs[i + 3] = new Vector2(uvData.TileX + uvData.SizeX, uvData.TileY + uvData.SizeY);
            }
            
            mesh.SetUVs(0, uvs);
        }
    }
}
