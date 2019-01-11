using System.Collections.Generic;
using UnityEngine;

public interface IChunk
{
    GameObject CurrentGO { get; set; }
    Vector3Int Position { get; set; }
    
    void AddBlock(Block block);
    IChunk TryAddBlock(Block block, Vector3 normal);
    void RemoveBlock(Vector3Int block);
    void GenerateChunk();

    bool[] BoolNeigbours(Vector3Int blockPos);
    bool GetNeigbourAt(int index, Vector3Int blockPos);
    
    int BlockCount();
    Block GetBlock(Vector3Int position);
    Block[] GetBlocks();
    
    (Vector3Int lowerBound, Vector3Int higherBound) GetChunkBounds();
    void CalculateNeigbours();
}
