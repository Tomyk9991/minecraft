using UnityEditor;

public class SimplexNoiseSettings : Context<SimplexNoiseSettings>
{
    public float Smoothness { get; set; }
    public float Steepness { get; set; }
    public int Seed { get; set; }

    public SimplexNoiseSettings() { }

    public SimplexNoiseSettings(float smoothness, float steepness, int seed)
    {
        this.Smoothness = smoothness;
        this.Steepness = steepness;
        this.Seed = seed;
    }

    public override object Data()
    {
        return new SimplexNoiseSettingsSerializeHelper()
        {
            Smoothness = this.Smoothness,
            Seed = this.Seed,
            Steepness = this.Steepness
        };
    }

    public override SimplexNoiseSettings Caster(object data)
    {
        var helper = (SimplexNoiseSettingsSerializeHelper) data;
        this.Seed = helper.Seed;
        this.Smoothness = helper.Smoothness;
        this.Steepness = Steepness;

        return this;
    }
}
