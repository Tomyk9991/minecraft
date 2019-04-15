using UnityEditor;

public class SimplexNoiseSettings
{
    public float Smoothness { get; set; }
    public float Steepness { get; set; }
    public int Seed { get; set; }

    public SimplexNoiseSettings(float smoothness, float steepness, int seed)
    {
        this.Smoothness = smoothness;
        this.Steepness = steepness;
        this.Seed = seed;
    }
}
