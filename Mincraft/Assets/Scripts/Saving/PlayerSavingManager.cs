using System;
using System.IO;
using System.Text;
using Core.Managers;
using Core.Player.Systems;
using UnityEngine;
using Utilities;

namespace Core.Saving
{
    public class PlayerSavingManager : MonoBehaviour
    {
        public static void Save(ItemData[] items)
        {
            string inventoryPath = Path.Combine(GameManager.CurrentWorldPath, "Inventory.json");

            Wrapper<ItemData> data = new Wrapper<ItemData> {items = items};
            File.WriteAllBytes(inventoryPath, Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true)));
        }

        public static bool Load(out ItemData[] items)
        {
            string inventoryPath = Path.Combine(GameManager.CurrentWorldPath, "Inventory.json");
            if (File.Exists(inventoryPath))
            {
                string json = "";
                try
                {
                    json = File.ReadAllText(inventoryPath);
                }
                catch (Exception)
                {
                    Debug.Log($"Could not load inventory from {inventoryPath}");
                }

                if (json != "")
                {
                    try
                    {
                        Wrapper<ItemData> wrapper = JsonUtility.FromJson<Wrapper<ItemData>>(json);
                        items = wrapper.items;
                        return true;
                    }
                    catch (Exception)
                    {
                        Debug.Log("Formatting went wrong");
                    }
                }
            }

            items = null;
            return false;
        }

        [Serializable]
        public class Wrapper<T>
        {
            public T[] items;
        }
    }
}
