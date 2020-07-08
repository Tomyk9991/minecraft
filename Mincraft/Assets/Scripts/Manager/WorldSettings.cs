using System.Collections.Generic;
using Attributes;
using Core.Builder.Generation;
using Core.Chunks.Threading;
using Core.Managers;
using Core.Saving;
using Core.Math;
using Extensions;
using UnityEngine;

namespace Core.Managers
{
    public class WorldSettings : SingletonBehaviour<WorldSettings>
    {
        public static float WorldTick => Instance.worldTick;
        public static Int2 MinMaxYHeight => Instance.minMaxYHeight;
        public static NoiseSettings NoiseSettings => Instance.biomNoiseSettings;
        public bool drawGizmosChunks = false;

        public List<Biom> Bioms => bioms;
        
        [Header("General settings")] 
        [SerializeField] private float worldTick = 0.3333f;
        [Space]
        
        [SerializeField] private int seed = -1;
        [SerializeField] private Int2 minMaxYHeight = Int2.Zero;

        [Header("Biom settings")] 
        [SerializeField] private BiomScriptable biomSaveable = null;
        [SerializeField] List<Biom> bioms;
        [Header("Biom Noise Settings")]
        [SerializeField] public float smoothness = 40;
        [SerializeField] public float steepness = 2;
        private NoiseSettings biomNoiseSettings;

        private ContextIO<NoiseSettings> noiseIO;

        private void Start()
        {
            bioms = biomSaveable.bioms;
            
            noiseIO = GameManager.CurrentWorldName == "" ?
                new ContextIO<NoiseSettings>(ContextIO.DefaultPath) :
                new ContextIO<NoiseSettings>(ContextIO.DefaultPath + GameManager.CurrentWorldName + "/");
            
            NoiseSettings settings = noiseIO.LoadContext();

            if (settings == null)
            {
                if (seed == -1)
                    seed = UnityEngine.Random.Range(-100_000, 100_000);

                biomNoiseSettings = new NoiseSettings(smoothness, steepness, seed);
            }
            else
            {
                biomNoiseSettings = settings;
                seed = settings.Seed;
                steepness = settings.Steepness;
                smoothness = settings.Smoothness;
            }
        }

        private void OnDestroy()
        {
            if (noiseIO?.Path != ContextIO.DefaultPath)
            {
                Debug.Log("Wird gespeichert");
                noiseIO.SaveContext(NoiseSettings, "Noise");
            }
        }
    }
}