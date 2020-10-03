using System;

namespace Core.Saving
{
    [Serializable]
    public class SettingsData : IDataContext
    {
        public DataContextFinder Finder => DataContextFinder.Settings;
        public int fovSlider;
        public int renderDistance;

        public SettingsData() { }
            
        public SettingsData(int fovSlider, int renderDistance)
        {
            this.fovSlider = fovSlider;
            this.renderDistance = renderDistance;
        }
    }
}
