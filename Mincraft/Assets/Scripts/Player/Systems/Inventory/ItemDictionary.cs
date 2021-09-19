using System.Collections.Generic;
using UnityEngine;

namespace Core.Player.Interaction
{
    public class ItemDictionary : MonoBehaviour
    {
        [SerializeField] private ItemDataScriptable scriptable = null;

        private static Dictionary<int, ItemInformation> itemDictionary = new Dictionary<int, ItemInformation>();
        

        private void Awake()
        {
            var data = scriptable.itemInformation;
            itemDictionary = new Dictionary<int, ItemInformation>(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                int index = data[i].IsBlock ? (int) data[i].BlockID : short.MaxValue + 1 + data[i].ItemID;
                itemDictionary.Add(index, data[i]);
            }
        }

        public static string GetName(int itemID)
            => itemDictionary.ContainsKey(itemID) ? itemDictionary[itemID].Name : "";

        public static Sprite GetValue(int itemID)
            => itemDictionary.ContainsKey(itemID) ? itemDictionary[itemID].Sprite : null;
    }
}