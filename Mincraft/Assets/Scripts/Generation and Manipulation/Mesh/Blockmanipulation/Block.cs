using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct Block
{
    public Vector3Int Position;
    public int ID { get; set; }

    private bool[] neighbours;

    public bool[] Neighbours => neighbours;


    public Block(Vector3Int position)
    {
        ID = 0;
        this.Position = position;
        neighbours = null;
    }

    //Recalculates this index
    public bool GetNeigbourAt(int index)
    {
        return BlockDictionary.GetValue(this.Position + directions[index]).result;
    }

    public void RecalculateNeighbours()
    {
        neighbours = new[]
        {
            //Forward
            BlockDictionary.GetValue(this.Position + directions[0]).result,
            //Back
            BlockDictionary.GetValue(this.Position + directions[1]).result,
            //Up
            BlockDictionary.GetValue(this.Position + directions[2]).result,
            //Down
            BlockDictionary.GetValue(this.Position + directions[3]).result,
            //Left
            BlockDictionary.GetValue(this.Position + directions[4]).result,
            //Right
            BlockDictionary.GetValue(this.Position + directions[5]).result,
        };
    }

    private static Vector3Int[] directions = new[]
    {
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1),
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };
}