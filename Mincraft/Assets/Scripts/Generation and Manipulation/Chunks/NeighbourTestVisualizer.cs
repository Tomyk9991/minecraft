using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeighbourTestVisualizer : MonoBehaviour
{
    private Int3 pos = Int3.One;
    private Chunk chunk;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(8, 8, 8), Vector3.one * 16f);

        //if (pos == Int3.One)
        //{
        //    pos = transform.position.ToInt3();
        //    Chunk c = ChunkDictionary.GetValue(pos);

        //    if (c != null)
        //        this.chunk = c;
        //}

        //if (chunk != null)
        //{
        //    Chunk[] neigbours = chunk.GetNeigbours();

        //    Gizmos.color = Color.white;
        //    for (int i = 0; i < neigbours.Length; i++)
        //    {
        //        if (neigbours[i] != null)
        //        {
        //            Gizmos.DrawWireCube(neigbours[i].Position.ToVector3() + new Vector3(8, 8, 8), Vector3.one * 16f);
        //        }
        //    }
        //}
    }
}
