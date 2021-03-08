using Core.Saving;

namespace Core.Player.Systems.Inventory
{
    public class QuickBar
    {
        private int size;
        public ItemData[] Items;

        public QuickBar(int size = 10)
        {
            this.Items = new ItemData[size];
            this.size = size;
        }

        public QuickBar(ItemData[] data)
        {
            this.Items = data;
            this.size = data.Length;
        }

        public ItemData this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }
    }
}