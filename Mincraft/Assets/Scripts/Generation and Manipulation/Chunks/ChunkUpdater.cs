using System;
using System.Collections.Concurrent;
using UnityEngine;
using System.Collections.Generic;

public class ChunkUpdater : MonoBehaviour, ICreateChunk
{
    public ChunkGameObjectPool GoPool { get; set; }

    [Header("References")]
    [SerializeField] private ChunkGenerator generator = null;
    [SerializeField] private GameObject player = null;

    [Header("Chunk-redraw condition")]
    [SerializeField] private float maxDistance = 10;
    [Tooltip("Float, der bestimmt, nach wie viel Sekunden die Entfernung berechnet werden soll")]
    [SerializeField] private float periodNewRedraw = 1;
    [SerializeField] private int amountDrawChunksPerFrame = 5;
    [Header("Debug")]
    [SerializeField] private bool drawLatestPlayerPosition = false;

    private Int3 size;
    private int chunkSize;

    //Simplex noise
    private float smoothness = 0;
    private float steepness = 0;

    private Int3 latestPlayerPosition;

    private float currentDistance = 0;
    private float ellapsedTime = 0;

    private List<IChunk> newChunks;
    private MeshModifier modifier;
    private ConcurrentQueue<MeshData> meshDatasQueue;

    private void Start()
    {
        newChunks = new List<IChunk>();
        modifier = new MeshModifier();
        meshDatasQueue = new ConcurrentQueue<MeshData>();

        smoothness = generator.smoothness;
        steepness = generator.steepness;

        modifier.MeshAvailable += (sender, data) =>
        {
            meshDatasQueue.Enqueue(data);
        };

        latestPlayerPosition = player.transform.position.ToInt3();
        size = generator.drawDistance;
        chunkSize = ChunkGenerator.GetMaxSize;
        GoPool = ChunkGameObjectPool.Instance;
    }

    private void Update()
    {
        ellapsedTime += Time.deltaTime;
        if (ellapsedTime > periodNewRedraw)
        {
            Debug.Log("Check");
            ellapsedTime = 0f;
            currentDistance = Vector3.Distance(player.transform.position, latestPlayerPosition.ToVector3());

            if (currentDistance > maxDistance)
            {
                //Divide and conquer
                RecalculateChunks();

                currentDistance = 0;
                latestPlayerPosition = player.transform.position.ToInt3();
            }
        }

        CheckForNewChunksToDraw();
    }

    private void CheckForNewChunksToDraw()
    {
        if (meshDatasQueue.Count != 0)
        {
            for (int i = 0, counter = 0; i < meshDatasQueue.Count; i++, counter++)
            {
                if (counter == amountDrawChunksPerFrame - 1) break; //Exiting-condition, if you want to draw less than all available meshes

                if (meshDatasQueue.TryDequeue(out var data))
                {
                    modifier.RedrawMeshFilter(data.GameObject, data);
                }
            }
        }
    }

    private void RecalculateChunks()
    {
        // 1) Iteriere durch alle Chunkpositionen in der Umgebung des Spielers
        // 2) Prüfe, ob die Chunkpositionen bereits gezeichnet sind
        // 2.a.1) Erstelle neuen Chunk
        // 2.a.2) Wenn ein Chunk noch nicht gezeichnet wurde, zeichne diesen Chunk
        // 2.a.3) Füge den neu gezeichneten Chunk zu den aktuell gezeichneten Chunks hinzu
        // 2.b.1) Wenn ein Chunk bereits gezeichnet wurde, aber nicht mehr im Umgebung, entferne den Chunk
        // 2.b.2) Entferne alle Referenzen zu diesem Chunk und gib das GameObject wieder frei
        // 3) Berechne Chunks neu. (Nicht alle, nur die Am Rand und die neu hinzugefügten


        // 1)

        int xStart = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        int yStart = MathHelper.ClosestMultiple(0, chunkSize); // Fix height
        int zStart = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);



        int counter = 0;

        for (int x = xStart - size.X * 2; x < xStart + size.X * 2; x += chunkSize)
        {
            for (int y = yStart; y > -size.Y; y -= chunkSize) // Minus to calculate chunks downwards, not upwards
            {
                for (int z = zStart - size.Z * 2; z < zStart + size.Z * 2; z += chunkSize)
                {
                    // 2)
                    Int3 chunkPos = new Int3(x, y, z);
                    if (ChunkDictionary.GetValue(chunkPos) == null)
                    {
                        // 2.a.1)
                        IChunk chunk = GenerateChunk(chunkPos);
                        newChunks.Add(chunk);
                        chunk.GenerateBlocks();
                    }

                    counter++;
                }
            }
        }

        Debug.Log($"Checked {counter} chunks");

        foreach (IChunk activeChunk in ChunkDictionary.GetActiveChunks())
            activeChunk.CalculateNeigbours();

        for (int i = 0; i < newChunks.Count; i++)
        {
            newChunks[i].CurrentGO.SetActive(true);
            newChunks[i].CurrentGO.name = "New " + newChunks[i].Position.ToString();
            modifier.Combine(newChunks[i]);
        }

        Debug.Log($"{newChunks.Count} new Chunks generated");

        newChunks.Clear();
    }


    public IChunk GenerateChunk(Int3 pos)
    {
        // Verschiebe das auf den Thread, 
        //if (System.IO.FileExists())
        //{

        //}
        IChunk chunk = new Chunk
        {
            Position = pos,
            CurrentGO = GoPool.GetNextUnusedChunk()
        };

        ChunkDictionary.Add(pos, chunk);
        ChunkGameObjectDictionary.Add(chunk.CurrentGO, chunk);

        return chunk;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && drawLatestPlayerPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(latestPlayerPosition.ToVector3(), 2f);
        }
    }
}