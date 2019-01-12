using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

public class Chunk : IChunk
{
    public GameObject CurrentGO { get; set; }
    public Vector3Int Position { get; set; }

    public Vector3Int lowerBound, higherBound;
    
    private bool boundsCalculated = false;
    private Block[] blocks;
    private static int chunkSize;

    private IChunk[] chunkNeigbours;
    
    private static Vector3Int[] directions = 
    {
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1),
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };
    
    #region Interface implementation

    public Chunk()
    {
        chunkSize = ChunkGenerator.GetMaxSize;
        blocks = new Block[chunkSize * chunkSize * chunkSize];
        chunkNeigbours = new IChunk[6];
    }

    public void AddBlock(Block block)
    {
        Vector3Int local = GetLocalPosition(block.Position);
        int index = GetFlattenIndex(local);
        blocks[index] = block;
    }

    public IChunk TryAddBlock(Block block, Vector3 normal)
    {
        Vector3Int local = GetLocalPosition(block.Position);
        int index = GetFlattenIndex(local);

        if (index >= chunkSize * chunkSize * chunkSize || index < 0)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                if (directions[i] == normal)
                    return chunkNeigbours[i];
            }

            return null;
        }

        AddBlock(block);
        return this;
    }

    public void RemoveBlock(Vector3Int blockPos)
    {
        int index = GetFlattenIndex(GetLocalPosition(blockPos));
        blocks[index] = null;
    }
    
    public int BlockCount() => blocks.Length;

    public void GenerateChunk()
    {
    }
    
    public Block GetBlock(Vector3Int position)
    {
        int index = GetFlattenIndex(GetLocalPosition(position));
        return blocks[index];
    }
    
    public Block[] GetBlocks() => blocks;
    
    public (Vector3Int lowerBound, Vector3Int higherBound) GetChunkBounds()
    {
        if (!boundsCalculated)
            GetChunkBoundsCalc();

        return (lowerBound, higherBound);
    }
    
    public void CalculateNeigbours()
    {
        for (int i = 0; i < chunkNeigbours.Length; i++)
        {
            chunkNeigbours[i] = ChunkDictionary.GetValue(this.Position + (directions[i] * chunkSize));
        }
    }

    /// <summary>
    /// Returns Block's neigbours
    /// </summary>
    /// <param name="index"></param>
    /// <param name="blockPos">Rufe Block mit globaler Position auf</param>
    /// <returns></returns>
    public bool GetNeigbourAt(int index, Vector3Int blockPos)
    {
        Vector3Int local = GetLocalPosition(blockPos);
        switch (index)
        {
            case 0:
            {
                if ((local + directions[0]).z < chunkSize)
                {
                    return blocks[GetFlattenIndex(local + directions[0])] != null;
                }
                else
                {
                    if (chunkNeigbours[0] != null)
                    {
                        return chunkNeigbours[0].GetBlock(new Vector3Int(blockPos.x, blockPos.y, chunkNeigbours[0].Position.z)) != null;
                    }
                }
                break;
            }
            case 1:
                if ((local + directions[1]).z >= 0)
                {
                    return blocks[GetFlattenIndex(local + directions[1])] != null;
                }
                else
                {
                    if (chunkNeigbours[1] != null)
                    {
                        return chunkNeigbours[1].GetBlock(new Vector3Int(blockPos.x, blockPos.y, chunkNeigbours[1].Position.z + chunkSize - 1)) != null;
                    }
                }
                break;
            case 2:
                if ((local + directions[2]).y < chunkSize)
                {
                    return blocks[GetFlattenIndex(local + directions[2])] != null;
                }
                else
                {
                    if (chunkNeigbours[2] != null)
                    {
                        return chunkNeigbours[2].GetBlock(new Vector3Int(blockPos.x, chunkNeigbours[2].Position.y, blockPos.z)) != null;
                    }
                }
                break;
            case 3:
                if ((local + directions[3]).y >= 0)
                {
                    return blocks[GetFlattenIndex(local + directions[3])] != null;
                }
                else
                {
                    if (chunkNeigbours[3] != null)
                    {
                        return chunkNeigbours[3].GetBlock(new Vector3Int(blockPos.x, chunkNeigbours[3].Position.y + chunkSize - 1, blockPos.z)) != null;
                    }
                }
                break;
            case 4:
                if ((local + directions[4]).x >= 0)
                {
                    return blocks[GetFlattenIndex(local + directions[4])] != null;
                }
                else
                {
                    if (chunkNeigbours[4] != null)
                    {
                        return chunkNeigbours[4].GetBlock(new Vector3Int(chunkNeigbours[4].Position.x + chunkSize - 1, blockPos.y, blockPos.z)) != null;
                    }
                }
                break;
            case 5:
                if ((local + directions[5]).x < chunkSize)
                {
                    return blocks[GetFlattenIndex(local + directions[5])] != null;
                }
                else
                {
                    if (chunkNeigbours[5] != null)
                    {
                        return chunkNeigbours[5].GetBlock(new Vector3Int(chunkNeigbours[5].Position.x, blockPos.y, blockPos.z)) != null;
                    }
                }
                break;
        }
        
        return false;
    }
    
    #endregion
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="blockPos">Rufe Block mit globaler Position auf</param>
    /// <returns></returns>
    public bool[] BoolNeigbours(Vector3Int blockPos)
    {
        return new []
        {
            GetNeigbourAt(0, blockPos),
            GetNeigbourAt(1, blockPos),
            GetNeigbourAt(2, blockPos),
            GetNeigbourAt(3, blockPos),
            GetNeigbourAt(4, blockPos),
            GetNeigbourAt(5, blockPos)
        };
    }
    
    private void GetChunkBoundsCalc()
    {
        boundsCalculated = true;
        int maxSize = ChunkGenerator.GetMaxSize;
        int half = maxSize / 2;

        lowerBound = new Vector3Int(Mathf.FloorToInt(Position.x),
            Mathf.FloorToInt(Position.y),
            Mathf.FloorToInt(Position.z));

        higherBound = new Vector3Int(Mathf.FloorToInt(Position.x + maxSize),
            Mathf.FloorToInt(Position.y + maxSize),
            Mathf.FloorToInt(Position.z + maxSize));
    }

    private int GetFlattenIndex(Vector3Int localPosition)
        => localPosition.x + chunkSize * (localPosition.y + chunkSize * localPosition.z);

    //TODO Kann sein, dass die Origin des Chunks nicht unten Links, sondern
    //in der Mitte ist
    private Vector3Int GetLocalPosition(Vector3Int globalPosition)
        => globalPosition - this.Position;
}
