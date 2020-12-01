using Core.Builder;
using UnityEngine;

namespace Core.Saving
{
    [System.Serializable]
    public class ItemData
    {
        public int x;
        public int y;
        public int quickbarIndex;
        public int ItemID;
        public int Amount;

        public GameObject CurrentGameObject { get; set; }
        
        public ItemData(int itemId, int x, int y, int QuickbarIndex, int amount, GameObject go)
        {
            this.ItemID = itemId;
            this.x = x;
            this.y = y;
            this.quickbarIndex = QuickbarIndex;
            this.Amount = amount;
            this.CurrentGameObject = go;
        }

        public static ItemData Empty => new ItemData(0, 0, 0, 0, 0, null);

        public override string ToString()
        {
            return $"itemID: {(BlockUV) ItemID}, Pos({x} | {y}), amount: {Amount}";
        }

        public void SetXY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void SetIndex(int index)
        {
            this.quickbarIndex = index;
        }
    }
}