using System;

namespace Core.Builder.Generation
{
    [Serializable]
    public class NoiseSettings
    {
        public float Smoothness;
        public int Seed;

        public NoiseSettings() { }

        public NoiseSettings(float smoothness, int seed)
        {
            this.Smoothness = smoothness;
            this.Seed = seed;
        }
    }
}
