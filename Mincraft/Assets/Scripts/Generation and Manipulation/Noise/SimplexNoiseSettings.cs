public class SimplexNoiseSettings
{
    public float Smoothness { get; set; }
    public float Steepness { get; set; }

    public SimplexNoiseSettings(float smoothness, float steepness)
    {
        this.Smoothness = smoothness;
        this.Steepness = steepness;
    }
}
