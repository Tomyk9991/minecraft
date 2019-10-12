using System;
using UnityEngine;

using Core.Builder.Generation;
using Core.Managers;
using Core.Math;
using Core.Saving;
using Extensions;
using System.Collections.Generic;

namespace Core.Chunking
{
    public class ChunkSettings : SingletonBehaviour<ChunkSettings>
    {
        public static int ChunkSize => (int) Instance.chunkSize;
        public static Int2 MinMaxYHeight => Instance.minMaxYHeight;
        public static NoiseSettings NoiseSettings => Instance.biomNoiseSettings;
        public static Int3 DrawDistance => Instance.drawDistance;
        public bool drawGizmosChunks = false;

        public List<Biom> Bioms => bioms;

        [Header("Chunksettings")]
        [SerializeField] private uint chunkSize = 0;
        [SerializeField] private int seed = -1;
        [SerializeField] private Int2 minMaxYHeight;

        [Header("Biom settings")]
        [SerializeField] List<Biom> bioms;
        [Header("Biom Noise Settings")]
        [SerializeField] public float smoothness = 40;
        [SerializeField] public float steepness = 2;
        private NoiseSettings biomNoiseSettings;

        private ContextIO<NoiseSettings> noiseIO;

        [Header("Instantiation")]
        //[SerializeField] private BlockUV surface = default;
        //[SerializeField] private BlockUV bottom = default;
        [SerializeField] private Int3 drawDistance = default;


        private void OnValidate() => PlayerPrefs.SetInt(nameof(chunkSize), (int)chunkSize);

        private void Start()
        {
            if (GameManager.CurrentWorldName == "")
            {
                noiseIO = new ContextIO<NoiseSettings>(ContextIO.DefaultPath);
            }
            else
            {
                noiseIO = new ContextIO<NoiseSettings>(ContextIO.DefaultPath + GameManager.CurrentWorldName + "/");
            }
            NoiseSettings settings = noiseIO.LoadContext();


            if (settings == null)
            {
                if (seed == -1)
                    seed = UnityEngine.Random.Range(-100_000, 100_000);

                biomNoiseSettings = new NoiseSettings(smoothness, steepness, seed);

                if (drawDistance.X % chunkSize != 0 || drawDistance.Y % chunkSize != 0 || drawDistance.Z % chunkSize != 0)
                {
                    throw new Exception("Diggah, WeltSize nicht teilbar durch ChunkSize");
                }
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
            if (noiseIO.Path != ContextIO.DefaultPath)
            {
                Debug.Log("Wird gespeichert");
                noiseIO.SaveContext(NoiseSettings, "Noise");
            }
        }
    }
}