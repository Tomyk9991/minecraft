using System.Collections.Generic;
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

    public void AddBlock(GameObject block)
    {
        Vector3 centeredCubePosition = block.transform.position;
        ChunkDictionary.Add(centeredCubePosition, block.transform);

        (GameObject parent, bool hasCreatedNewChunk) = GenerateOrGetChunkGameObject(centeredCubePosition);
        
        block.transform.parent = parent.transform;

        if (!hasCreatedNewChunk)
            ModifyMesh.Combine(block, parent);
        else
            ModifyMesh.CombineForAll(parent);
        
        
    }

    public void RemoveBlock(GameObject currentChunk, Transform blockToRemove)
    {
        ChunkDictionary.Remove(blockToRemove.position);
        ModifyMesh.RemoveBlockFromMesh(currentChunk.transform, blockToRemove);
        DeleteChunkIfNotNeeded(currentChunk.transform);
    }

    private void DeleteChunkIfNotNeeded(Transform currentChunk)
    {
        if (currentChunk.childCount == 0)
        {
            chunks.Remove(currentChunk.GetComponent<IChunk>());
            Destroy(currentChunk.gameObject);
        }
    }

    private (GameObject, bool) GenerateOrGetChunkGameObject(Vector3 target)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            (Vector3Int lowerBound, Vector3Int higherBound) = chunks[i].GetChunkBounds();
            
            if ((target.x >= lowerBound.x && target.x <= higherBound.x) &&
                (target.y >= lowerBound.y && target.y <= higherBound.y) &&
                (target.z >= lowerBound.z && target.z <= higherBound.z))   // Dann liegt es dazwischen und muss dementsprechend in diesen Chunk
            {
                return (chunks[i].CurrentGO, false);
            }
        }

        int x = Mathf.RoundToInt(target.x / maxSize.x) * maxSize.x;
        int y = Mathf.RoundToInt(target.y / maxSize.y) * maxSize.y;
        int z = Mathf.RoundToInt(target.z / maxSize.z) * maxSize.z;
        
        Vector3Int chunkPos = new Vector3Int(x, y, z);
        
        GameObject go = Instantiate(chunkPrefab, transform);
        go.transform.position = chunkPos;
        go.name = "Chunk " + go.transform.position.ToString();
        go.GetComponent<IChunk>().CurrentGO = go;
        chunks.Add(go.GetComponent<IChunk>());

        
        return (go, true);
    }
}
