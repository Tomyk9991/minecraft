using Core.Builder;

namespace Core.Saving
{
    [System.Serializable]
    public struct ItemData
    {
        public int x;
        public int y;
        public int ItemID;
        public int Amount;

        public ItemData(int itemId, int x, int y, int amount)
        {
            ItemID = itemId;
            this.x = x;
            this.y = y;
            this.Amount = amount;
        }

        public static ItemData Empty => new ItemData(-1, -1, -1, -1);

        public static bool operator ==(ItemData i1, ItemData i2)
            => i1.x == i2.x &&
               i1.y == i2.y &&
               i1.Amount == i2.Amount &&
               i1.ItemID == i2.ItemID;

        public static bool operator !=(ItemData i1, ItemData i2) => !(i1 == i2);

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return $"itemID: {(BlockUV) ItemID}, Pos({x} | {y}), amount: {Amount}";
        }

        public void SetXY(int newX, int newY)
        {
            this.x = x;
            this.y = y;
        }
    }
}