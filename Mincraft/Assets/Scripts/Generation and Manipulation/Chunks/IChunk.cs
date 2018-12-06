using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChunk
{
    GameObject CurrentGO { get; set; }
    void GenerateChunk();
    (Vector3Int, Vector3Int) GetChunkBounds();
}
