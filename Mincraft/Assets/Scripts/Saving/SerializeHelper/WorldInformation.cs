using System;
using Core.Builder.Generation;

namespace Core.Saving
{
    [Serializable]
    public class WorldInformation : IDataContext
    {
        public DataContextFinder Finder => DataContextFinder.WorldInformation;
        [field: NonSerialized] public float Size;
        public string WorldName;
        public NoiseSettings NoiseSettings;

        public WorldInformation()
        {
            this.Size = 0;
            this.WorldName = "";
            this.NoiseSettings = null;
        }

        public WorldInformation(float size, string worldName, NoiseSettings settings)
        {
            this.Size = size;
            this.WorldName = worldName;
            this.NoiseSettings = settings;
        }
    }
}
