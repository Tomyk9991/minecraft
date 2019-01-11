using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Block
{
    public Vector3Int Position;
    public int ID { get; set; }

    public Block(Vector3Int position)
    {
        ID = 0;
        this.Position = position;
    }

    public void SetID(int id)
    {
        this.ID = id;
    }
}