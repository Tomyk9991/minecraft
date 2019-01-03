using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ChunkManager : SingletonBehaviour<ChunkManager>
{
    public static Vector3Int GetMaxSize => ChunkManager.Instance.maxSize;
    
    [SerializeField] private GameObject chunkPrefab = null;
    [SerializeField] private Vector3Int maxSize;
    
    public List<IChunk> chunks = new List<IChunk>();
    
    private void Start()
    {
        foreach (Transform children in transform)
            chunks.Add(children.GetComponent<IChunk>());

        foreach (IChunk chunk in chunks)
        {
            chunk.CurrentGO.name += " " + chunk.CurrentGO.transform.position.ToString();
            chunk.GenerateChunk();
        }
    }

    public (IChunk, GameObject) AddBlock(Block block)
    {
        ChunkDictionary.Add(block.Position, block.Position);
        
        (IChunk chunk, GameObject parent, bool hasCreatedNewChunk) = GenerateOrGetChunkGameObject(block.Position);
        chunk.AddBlock(block);

        return (chunk, parent);
    }

    public void RemoveBlock(GameObject currentChunk, Block block)
    {
        ChunkDictionary.Remove(block.Position);
        //Ich entferne erst den Block auf dem Chunk
        IChunk chunk = currentChunk.GetComponent<IChunk>();
        chunk.RemoveBlock(block);
        // Und erstelle anschließend mit den restlichen Blöcken den Chunk
        MeshData data = ModifyMesh.Combine(chunk);

        var refMesh = currentChunk.GetComponent<MeshFilter>();
        
        refMesh.mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = data.Vertices.ToArray(),
            triangles = data.Triangles.ToArray()
        };
        
        refMesh.mesh.RecalculateNormals();
        
        Destroy(currentChunk.GetComponent<MeshCollider>());
        currentChunk.AddComponent<MeshCollider>();
        
        DeleteChunkIfNotNeeded(chunk);
    }

    private void DeleteChunkIfNotNeeded(IChunk currentChunk)
    {
        if (currentChunk.BlockCount() == 0)
        {
            chunks.Remove(currentChunk);
            Destroy(currentChunk.CurrentGO);
        }
    }

    private (IChunk, GameObject, bool) GenerateOrGetChunkGameObject(Vector3 target)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            (Vector3Int lowerBound, Vector3Int higherBound) = chunks[i].GetChunkBounds();
            
            if ((target.x >= lowerBound.x && target.x <= higherBound.x) &&
                (target.y >= lowerBound.y && target.y <= higherBound.y) &&// Dann liegt es dazwischen und muss 
                (target.z >= lowerBound.z && target.z <= higherBound.z))// dementsprechend in diesen Chunk
            {
                return (chunks[i], chunks[i].CurrentGO, false);
            }
        }

        int x = Mathf.RoundToInt(target.x / maxSize.x) * maxSize.x;
        int y = Mathf.RoundToInt(target.y / maxSize.y) * maxSize.y;
        int z = Mathf.RoundToInt(target.z / maxSize.z) * maxSize.z;
        
        Vector3Int chunkPos = new Vector3Int(x, y, z);
        
        GameObject go = Instantiate(chunkPrefab, transform);
        Chunk chunk = (Chunk) go.GetComponent<IChunk>();
        
        chunk.CurrentGO = go;
        chunk.ChunkOffset = chunkPos;
        
        go.name = "Chunk " + chunk.ChunkOffset.ToString();
        
        chunks.Add(go.GetComponent<IChunk>());

        return (go.GetComponent<IChunk>(), go, true);
    }
}