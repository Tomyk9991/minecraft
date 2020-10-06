using System.Collections.Generic;
using Core.Builder;
using UnityEngine;

namespace Core.Player.Interaction
{
    public class DroppedItemUVSetter : MonoBehaviour
    {
        public void FromBlock(Block block)
        {
            Mesh mesh = transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            
            List<Vector2> uvs = new List<Vector2>(24);
            UVData[] currentUVData = UVDictionary.GetValue(block.ID);
            
            for (int i = 0; i < 6; i++)
            {
                UVData uvData = currentUVData[i];
                uvs.Add(new Vector2(uvData.TileX, uvData.TileY));
                uvs.Add(new Vector2(uvData.TileX + uvData.SizeX, uvData.TileY));
                uvs.Add(new Vector2(uvData.TileX, uvData.TileY + uvData.SizeY));
                uvs.Add(new Vector2(uvData.TileX + uvData.SizeX, uvData.TileY + uvData.SizeY));
            }
            
            mesh.SetUVs(0, uvs);
        }
    }
}