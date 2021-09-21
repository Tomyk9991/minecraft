using System.Collections;
using System.Collections.Generic;
using Core.Builder;
using UnityEngine;

namespace Core.Player.Interaction
{
    public class DroppedItemInformation : MonoBehaviour
    {
        [SerializeField] private GameObject blockGFX = null;
        [SerializeField] private GameObject itemGFX = null;
        
        public bool IsBlock { get; private set; }
        public int ItemID { get; set; }
        public int Amount { get; private set; }

        
        public GameObject FromItem(int itemID, int amount)
        {
            this.IsBlock = false;
            this.ItemID = itemID;
            this.Amount = amount;
            
            blockGFX.SetActive(false);
            itemGFX.SetActive(true);

            MeshMaterialPair pair = ItemDictionary.GetMeshMaterialPair(itemID);
            
            
            itemGFX.GetComponent<MeshFilter>().mesh = pair.Mesh;
            itemGFX.GetComponent<MeshRenderer>().material = pair.Material;

            return itemGFX;
        }
        
        public GameObject FromBlock(Block block, int amount)
        {
            this.IsBlock = true;
            this.ItemID = (int) block.ID;
            this.Amount = amount;

            blockGFX.SetActive(true);
            itemGFX.SetActive(false);
            
            
            Mesh mesh = blockGFX.GetComponent<MeshFilter>().mesh;
            
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

            return blockGFX;
        }
    }
}
