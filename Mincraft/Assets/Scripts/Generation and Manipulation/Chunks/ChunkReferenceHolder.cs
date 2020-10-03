using System;
using UnityEngine;

namespace Core.Chunks
{
    public class ChunkReferenceHolder : MonoBehaviour
    {
        public Chunk Chunk { get; set; }

        public void OnDrawGizmosSelected()
        {
            Vector3 sixteen = Vector3.one * 16;
            Vector3 eight = Vector3.one * 8;
            Gizmos.DrawWireCube(transform.position + eight, sixteen);
        }
    }
}