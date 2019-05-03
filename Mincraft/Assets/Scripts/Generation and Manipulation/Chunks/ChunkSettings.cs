using System;
using UnityEngine;

public class ChunkSettings : SingletonBehaviour<ChunkSettings>
{
    public static int GetMaxSize => (int) Instance.chunkSize;
    public static int MaxYHeight => Instance.maxYHeight;
    public static SimplexNoiseSettings SimplexNoiseSettings => Instance.simplexNoiseSettings;
    public bool drawGizmosChunks = false;

    [Header("Chunksettings")]
    [SerializeField] private uint chunkSize = 0;
    [SerializeField] private int seed = -1;
    [SerializeField] private int maxYHeight = 256;
    [SerializeField, ShowOnly] private int dictionarySize;

    [Header("Noisesettings")] //Remake this to make is depend on the biom, you're currently at
    [SerializeField] public float smoothness = 40;
    [SerializeField] public float steepness = 2;
    private SimplexNoiseSettings simplexNoiseSettings;

    [Header("Instantiation")]
    //[SerializeField] private BlockUV surface = default;
    //[SerializeField] private BlockUV bottom = default;
    [SerializeField] public Int3 drawDistance = default;


    private void OnValidate() => PlayerPrefs.SetInt(nameof(chunkSize), (int)chunkSize);

    private void Start()
    {
        if (seed == -1)
            seed = UnityEngine.Random.Range(-100_000, 100_000);

        simplexNoiseSettings = new SimplexNoiseSettings(smoothness, steepness, seed);

        if (drawDistance.X % chunkSize != 0 || drawDistance.Y % chunkSize != 0 || drawDistance.Z % chunkSize != 0)
        {
            throw new Exception("Diggah, WeltSize nicht teilbar durch ChunkSize");
        }
    }

    public void SetNoiseSettings(SimplexNoiseSettings settings)
    {
        this.simplexNoiseSettings = settings;
    }

    private void Update()
    {
        dictionarySize = ChunkDictionary.GetActiveChunks().Count;
    }
}