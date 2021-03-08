using System;
using Core.Builder;
using UnityEngine;

namespace Core.Player.Interaction
{
    public class ItemDictionary : MonoBehaviour
    {
        [SerializeField] private ItemDataScriptable scriptable = null;
        private static Sprite[] spriteDictionary;
        private static string[] nameDictionary;

        private void Awake()
        {
            var data = scriptable.itemInformation;

            spriteDictionary = new Sprite[data.Count];
            nameDictionary = new string[data.Count];
            
            for (int i = 0; i < data.Count; i++)
            {
                nameDictionary[(int) data[i].EnumType] = data[i].Name;
                spriteDictionary[(int) data[i].EnumType] = data[i].Sprite;
            }
        }

        public static string GetName(BlockUV itemID)
        {
            if (itemID <= 0 || (int) itemID >= Enum.GetNames(typeof(BlockUV)).Length)
                Debug.LogError("item id kinda strange");
        
            return nameDictionary[(int) itemID];
        }

        public static Sprite GetValue(BlockUV itemId)
        {
            if (itemId <= 0 || (int) itemId >= Enum.GetNames(typeof(BlockUV)).Length)
                Debug.LogError("item id kinda strange");
            
            return spriteDictionary[(int) itemId];
        }
    }
}