using UnityEngine;

public class Chunk : MonoBehaviour, IChunk
{
    [SerializeField] private bool drawChunkGizmos = true;
    public GameObject CurrentGO { get; set; }

    public Vector3Int lowerBound, higherBound;
    private bool boundsCalculated = false;
    
    private void Start()
    {
        CurrentGO = gameObject;
        GetChunkBoundsCalc();
    }

    public void GenerateChunk()
    {
        ModifyMesh.CombineForAll(transform.gameObject);
    }

    public (Vector3Int, Vector3Int) GetChunkBounds()
    {
        if (!boundsCalculated)
            GetChunkBoundsCalc();

        return (lowerBound, higherBound);
    }

    private void GetChunkBoundsCalc()
    {
        boundsCalculated = true;
        Vector3Int maxSize = ChunkManager.GetMaxSize;
        int xHalf = maxSize.x / 2;
        int yHalf = maxSize.y / 2;
        int zHalf = maxSize.z / 2;

        lowerBound = new Vector3Int(Mathf.FloorToInt(-xHalf + transform.position.x),
                                    Mathf.FloorToInt(-yHalf + transform.position.y),
                                    Mathf.FloorToInt(-zHalf + transform.position.z));
        
        higherBound = new Vector3Int(Mathf.FloorToInt(xHalf + transform.position.x),
                                     Mathf.FloorToInt(yHalf + transform.position.y),
                                     Mathf.FloorToInt(zHalf + transform.position.z));
    }

    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying && drawChunkGizmos)
            Gizmos.DrawWireCube(transform.position, ChunkManager.GetMaxSize);
        #endif
    }
}
