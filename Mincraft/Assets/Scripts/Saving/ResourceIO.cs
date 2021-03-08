using System;
using System.Collections.Generic;
using Core.Chunks;
using Core.Player.Systems.Inventory;

namespace Core.Saving
{
    public static class ResourceIO
    {
        private static Dictionary<Type, SavingManager> savingManagers;

        static ResourceIO()
        {
            savingManagers = new Dictionary<Type, SavingManager>
            {
                {typeof(Chunk), new ChunkSavingManager()},
                {typeof(Inventory), new PlayerSavingManager()}
            };
        }
        
        public static void Save<T>(SavingContext context)
        {
            savingManagers[typeof(T)].Save(context);
        }

        public static bool Load<T>(FileIdentifier fileIdentifier, out OutputContext context)
        {
            return savingManagers[typeof(T)].Load(fileIdentifier, out context);
        }
    }
}
