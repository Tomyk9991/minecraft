using System.Collections.Generic;
using Core.Builder.Generation;
using Core.Chunking.Threading;
using Core.Managers;
using Core.Math;
using Core.Saving;
using Extensions;
using UnityEditor;
using UnityEngine;
using UnityInspector.PropertyAttributes;

namespace Core
{
    public class WorldSettings : SingletonBehaviour<WorldSettings>
    {
        public static float WorldTick => Instance.worldTick;
        public static Int2 MinMaxYHeight => Instance.minMaxYHeight;
        public static NoiseSettings NoiseSettings => Instance.biomNoiseSettings;
        public static bool CalculateShadows => Instance.calculateShadows;
        public bool drawGizmosChunks = false;

        public List<Biom> Bioms => bioms;
        
        [Header("General settings")] 
        [SerializeField] private float worldTick = 0.3333f;

        [Header("Chunksettings")]
        [SerializeField] private bool calculateShadows = true;
        [SerializeField, DrawIfTrue(nameof(calculateShadows))] private Material affectedMaterial1 = null;
        [SerializeField, DrawIfTrue(nameof(calculateShadows))] private Material affectedMaterial2 = null;
        [Space]
        
        [SerializeField] private int seed = -1;
        [SerializeField] private Int2 minMaxYHeight;

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
            ChangeMaterialLightingProperty(affectedMaterial1);
            ChangeMaterialLightingProperty(affectedMaterial2);
            
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

        private void ChangeMaterialLightingProperty(Material material)
        {
            if (calculateShadows)
                material.SetFloat("_maxGlobalLightLevel", 0.8f);
            else
                material.SetFloat("_maxGlobalLightLevel", 0.0f);
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