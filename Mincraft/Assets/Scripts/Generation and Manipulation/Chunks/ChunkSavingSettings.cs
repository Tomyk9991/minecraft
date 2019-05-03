using System.Collections.Generic;
using UnityEngine;

public class ChunkSavingSettings : MonoBehaviour
{
    [SerializeField] private string path = @"C:/Users/thoma/Documents/MinecraftCloneWorlds";
    private ContextIO<Chunk> chunkIOManager;
    private ContextIO<SimplexNoiseSettings> simplexSettingsIOManager;

    private ChunkJobManager manager;
    private MeshModifier modifier;

    [ConsoleMethod(nameof(SaveWorld))]
    public void SaveWorld(int worldIndex)
    {
        InitializeIOManagers(worldIndex);

        SaveChunks(); // Saving chunks mean to save all active chunks
        SaveNoiseSettings(); // Save Simplex Noise Settings
        // Player settings later?
    }

    public void SaveChunks()
    {
        List<Chunk> chunks = ChunkDictionary.GetActiveChunks();

        foreach (var chunk in chunks)
            chunkIOManager.SaveContext(chunk, chunk.Position.ToString());
    }

    public void SaveNoiseSettings()
    {
        simplexSettingsIOManager.SaveContext(ChunkSettings.SimplexNoiseSettings, "WorldNoiseSettings");
    }

    [ConsoleMethod(nameof(LoadWorld))]
    public void LoadWorld(int worldIndex)
    {
        InitializeIOManagers(worldIndex);
        LoadNoiseSettings();
        LoadChunks();
    }

    public void LoadChunks()
    {
        List<Chunk> chunks = chunkIOManager.LoadContexts();

        if (manager == null)
            manager = new ChunkJobManager();

        if (modifier == null)
            modifier = new MeshModifier();

        if (!manager.Running)
            manager.Start();

        if (chunks != null)
        {
            foreach (var c in chunks)
            {
                c.CurrentGO = ChunkGameObjectPool.Instance.GetNextUnusedChunk();
                c.CurrentGO.transform.position = c.Position.ToVector3();
                c.CurrentGO.SetActive(true);
                c.CurrentGO.name = "Chunk " + c.Position.ToString();

                manager.Add(new ChunkJob(c));
            }
        }
    }

    public void LoadNoiseSettings()
    {
        ChunkSettings.Instance.SetNoiseSettings(simplexSettingsIOManager.LoadContext());
    }

    private void InitializeIOManagers(int worldIndex)
    {
        if (chunkIOManager == null || chunkIOManager.WorldIndex != worldIndex)
            chunkIOManager = new ContextIO<Chunk>(path, worldIndex);

        if (simplexSettingsIOManager == null || simplexSettingsIOManager.WorldIndex != worldIndex)
            simplexSettingsIOManager = new ContextIO<SimplexNoiseSettings>(path, worldIndex);
    }

    private void Update()
    {
        if (manager == null)
            return;


        for (int i = 0; i < manager.FinishedJobs.Count && i < 5; i++)
        {
            ChunkJob job = manager.DequeueFinishedJobs();

            modifier.SetMesh(job.Chunk.CurrentGO, job.MeshData, job.ColliderData);
        }
    }
}

