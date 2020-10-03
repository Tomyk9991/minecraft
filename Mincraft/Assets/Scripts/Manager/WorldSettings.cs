using System.Collections.Generic;
using Core.Builder.Generation;
using Core.Chunks.Threading;
using Core.Math;
using Extensions;
using UnityEngine;

namespace Core.Managers
{
    public class WorldSettings : SingletonBehaviour<WorldSettings>
    {
        public static float WorldTick => Instance.worldTick;
        public static Int2 MinMaxYHeight => Instance.minMaxYHeight;
        public static NoiseSettings NoiseSettings => new NoiseSettings(2.5f, 123);
        public bool drawGizmosChunks = false;

        public List<Biom> Bioms => bioms;

        [Header("General settings")] [SerializeField]
        private float worldTick = 0.3333f;

        [Space] 
        [SerializeField] private int seed = -1;
        [SerializeField] private Int2 minMaxYHeight = Int2.Zero;

        [Header("Biom settings")] [SerializeField]
        private BiomScriptable biomSaveable = null;

        [SerializeField] private List<Biom> bioms;

        [Header("Biom Noise Settings")] 
        [SerializeField] public float smoothness = 40;
        
        private NoiseSettings noiseSettings;

        private void Start()
        {
            bioms = biomSaveable.bioms;

            if (GameManager.WorldSelected)
            {
                noiseSettings = GameManager.Instance.NoiseSettings;
                this.seed = GameManager.Instance.NoiseSettings.Seed;
                this.smoothness = GameManager.Instance.NoiseSettings.Smoothness;
                return;
            }
            
            if (seed == -1)
                seed = UnityEngine.Random.Range(-100_000, 100_000);

            noiseSettings = new NoiseSettings(smoothness, seed);
        }
    }
}