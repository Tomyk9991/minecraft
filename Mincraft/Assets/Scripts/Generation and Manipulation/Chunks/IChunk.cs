using System.Collections.Generic;
using UnityEngine;

public interface IChunk
{
    GameObject CurrentGO { get; set; }
    Int3 Position { get; set; }
    
    void AddBlock(Block block);
    void AddBlocks(Block[] blocks);
    IChunk TryAddBlock(Block block, Vector3 normal);
    void RemoveBlock(Int3 block);
    void GenerateChunk();

    bool[] BoolNeigbours(Int3 blockPos);
    bool GetNeigbourAt(int index, Int3 blockPos);
    
    int BlockCount();
    Block GetBlock(Int3 position);
    Block[] GetBlocks();
    
    (Int3 lowerBound, Int3 higherBound) GetChunkBounds();
    void CalculateNeigbours();
}
