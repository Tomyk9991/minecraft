using Extensions;
using UnityEngine;

namespace Core.Player.Systems.Inventory
{
    public class Inventory : SingletonBehaviour<Inventory>
    {
        public int Width => width;
        public int Height => height;
        
        [SerializeField] private int width = 7;
        [SerializeField] private int height = 8;
        [SerializeField] private uint maxStackSize = 128;
    }
}
