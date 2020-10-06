namespace Core.Saving
{
    [System.Serializable]
    public class ItemData
    {
        public int x;
        public int y;
        public int ItemID;
        public uint amount;

        public ItemData(int itemId, int x, int y, uint amount)
        {
            ItemID = itemId;
            this.x = x;
            this.y = y;
            this.amount = amount;
        }
    }
}
