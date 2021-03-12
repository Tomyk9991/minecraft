using System;
using System.Collections.Generic;
using Core.Chunks;
using Core.Player.Systems.Inventory;
using UnityEngine;

namespace Core.Saving
{
    public static class ResourceIO
    {
        private static Dictionary<Type, SavingManager> savingManagers;
        private static Dictionary<Type, OutputContext> cache;

        static ResourceIO()
        {
            savingManagers = new Dictionary<Type, SavingManager>
            {
                {typeof(Chunk), new ChunkSavingManager()},
                {typeof(Inventory), new PlayerSavingManager()}
            };
            
            cache = new Dictionary<Type, OutputContext>
            {
                { typeof(Chunk), null },
                { typeof(Inventory), null }
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

        public static bool LoadCached<T>(FileIdentifier fileIdentifier, out OutputContext context)
        {
            //If the cache has an entry, there is no need to actually load from the disk
            OutputContext cacheEntry = cache[typeof(T)];
            
            if (cacheEntry == null)
            {
                bool loaded = savingManagers[typeof(T)].Load(fileIdentifier, out context);
                
                //Saving latest load to cache
                cache[typeof(T)] = context;
                return loaded;
            }

            context = cacheEntry;
            return true;
        }
    }
}
