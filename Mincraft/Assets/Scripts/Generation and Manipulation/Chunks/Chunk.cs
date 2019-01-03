using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

public class Chunk : MonoBehaviour, IChunk
{
    [SerializeField] private bool drawChunkGizmos = true;
    public GameObject CurrentGO { get; set; }

    [HideInInspector] public Vector3Int ChunkOffset = Vector3Int.zero;
    public Vector3Int lowerBound, higherBound;
    
    private bool boundsCalculated = false;
    private List<Block> blocks;
    
    #region Interface implementation

    public void AddBlock(Block block)
    {
        if (blocks == null)
            blocks = new List<Block>();

        blocks.Add(block);
    }

    public void RemoveBlock(Block block)
    {
        blocks.Remove(block);
    }
    
    public int BlockCount() => blocks.Count;

    public void GenerateChunk()
    {
        ModifyMesh.Combine(this);
//        ModifyMesh.CombineForAll(transform.gameObject);
    }
    
    public Block GetBlock(Vector3Int position)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].Position == position)
                return blocks[i];
        }
        
        throw new Exception("Block can't be found");
    }
    
    public List<Block> GetBlocks() => blocks;
    public (Vector3Int, Vector3Int) GetChunkBounds()
    {
        if (!boundsCalculated)
            GetChunkBoundsCalc();

        return (lowerBound, higherBound);
    }
    
    #endregion
    
    private void GetChunkBoundsCalc()
    {
        boundsCalculated = true;
        Vector3Int maxSize = ChunkManager.GetMaxSize;
        int xHalf = maxSize.x / 2;
        int yHalf = maxSize.y / 2;
        int zHalf = maxSize.z / 2;

        lowerBound = new Vector3Int(Mathf.FloorToInt(-xHalf + ChunkOffset.x),
            Mathf.FloorToInt(-yHalf + ChunkOffset.y),
            Mathf.FloorToInt(-zHalf + ChunkOffset.z));
        
        higherBound = new Vector3Int(Mathf.FloorToInt(xHalf + ChunkOffset.x),
            Mathf.FloorToInt(yHalf + ChunkOffset.y),
            Mathf.FloorToInt(zHalf + ChunkOffset.z));
    }

    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying && drawChunkGizmos)
            Gizmos.DrawWireCube(ChunkOffset, ChunkManager.GetMaxSize);
        #endif
    }
}
