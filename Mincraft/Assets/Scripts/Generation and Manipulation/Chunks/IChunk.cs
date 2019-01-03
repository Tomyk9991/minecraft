using System.Collections.Generic;
using UnityEngine;

public interface IChunk
{
    GameObject CurrentGO { get; set; }
    Vector3Int ChunkOffset { get; set; }
    
    void AddBlock(Block block);
    void RemoveBlock(Block block);
    void GenerateChunk();
    
    int BlockCount();
    Block GetBlock(Vector3Int position);
    List<Block> GetBlocks();
    
    (Vector3Int lowerBound, Vector3Int higherBound) GetChunkBounds();
}
