using System;

namespace Core.Saving
{
    [Serializable]
    public class SettingsData : IDataContext
    {
        public DataContextFinder Finder => DataContextFinder.Settings;
        public int fovSlider;
        public int renderDistance;
        public float mouseSensitivity; 

        public SettingsData() { }
            
        public SettingsData(int fovSlider, int renderDistance, float mouseSensitivity)
        {
            this.fovSlider = fovSlider;
            this.renderDistance = renderDistance;
            this.mouseSensitivity = mouseSensitivity;
        }

        public override string ToString()
        {
            return "FOV-Value: " + this.fovSlider + "\nRenderdistance: " + this.renderDistance;
        }
    }
}
