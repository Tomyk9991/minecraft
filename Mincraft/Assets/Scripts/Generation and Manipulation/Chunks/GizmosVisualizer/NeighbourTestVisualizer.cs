using UnityEngine;

using UnityInspector;
using Core.Math;
using Extensions;
using System.Collections.Generic;

namespace Core.Chunking.Debugging
{
    public class NeighbourTestVisualizer : MonoBehaviour
    {
        public Chunk Chunk;

        private List<Vector3> points;
        

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !ChunkSettings.Instance.drawGizmosChunks) return;

            Chunk chunk = ChunkClusterDictionary.GetChunkAt(this.transform.position.ToInt3());

            if (chunk == null) return;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(chunk.Cluster.Position.ToVector3() + Vector3.one * 128, Vector3.one * 16 * 16);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(chunk.GlobalPosition.ToVector3() + Vector3.one * 8, Vector3.one * 16);
        }

        private void OnDrawGizmos()
        {
            if(points != null)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    Gizmos.DrawCube(points[i], Vector3.one * 16);
                }
            }
        }

        [ContextMenu("Test")]
        private void Test()
        {
            Chunk[] neighbours = Chunk.GetNeigbours();

            points = new List<Vector3>();
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] == null)
                    continue;

                points.Add(neighbours[i].GlobalPosition.ToVector3() + Vector3.one * 8);
            }
        }
    }
}

