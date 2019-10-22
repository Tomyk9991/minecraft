using UnityEngine;

using System.Collections.Generic;

namespace Core.Chunking.Debugging
{
    public class NeighbourTestVisualizer : MonoBehaviour
    {
        public Chunk Chunk;

        private List<Vector3> points;

        private void OnDrawGizmosSelected()
        {
            if(points != null)
            {
                Gizmos.color = Color.white;
                for (int i = 0; i < points.Count; i++)
                {
                    Gizmos.DrawWireCube(points[i], Vector3.one * 16);
                }

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(this.transform.position + Vector3.one * 8, Vector3.one * 16);
            }
        }

        [ContextMenu("Test")]
        private void Test()
        {
            Chunk.CalculateNeighbours();
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

