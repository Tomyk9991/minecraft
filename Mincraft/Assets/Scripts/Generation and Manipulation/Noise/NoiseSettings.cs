﻿using Core.Saving;
using Core.Saving.Serializers;

namespace Core.Builder.Generation
{
    public class NoiseSettings : Context<NoiseSettings>
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

        public override object Data()
        {
            return new SimplexNoiseSettingsSerializeHelper()
            {
                Smoothness = this.Smoothness,
                Seed = this.Seed,
                Steepness = this.Steepness
            };
        }

        public override NoiseSettings Caster(object data)
        {
            var helper = (SimplexNoiseSettingsSerializeHelper) data;
            this.Seed = helper.Seed;
            this.Smoothness = helper.Smoothness;
            this.Steepness = helper.Steepness;

            return this;
        }
    }
}