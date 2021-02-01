using UnityEngine;

namespace Core.Saving
{
    [System.Serializable]
    public class ItemData
    {
        public int QuickbarIndex;
        public int ItemID;
        public int Amount;

        public GameObject CurrentGameObject { get; set; }
        
        public ItemData(int itemId, int QuickbarIndex, int amount, GameObject go)
        {
            this.ItemID = itemId;
            this.QuickbarIndex = QuickbarIndex;
            this.Amount = amount;
            this.CurrentGameObject = go;
        }

        public static ItemData Empty => new ItemData(0, 0, 0, null);
    }
}