namespace Core.Builder.Generation
{
    public class NoiseSettings
    {
        public float Smoothness { get; set; }
        public float Steepness { get; set; }
        public int Seed { get; set; }

        public NoiseSettings() { }

        public NoiseSettings(float smoothness, float steepness, int seed)
        {
            this.Smoothness = smoothness;
            this.Steepness = steepness;
            this.Seed = seed;
        }
    }
}
