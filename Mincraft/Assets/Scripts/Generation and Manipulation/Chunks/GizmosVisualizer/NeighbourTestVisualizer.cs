using UnityEngine;

using UnityInspector;
using Core.Math;
using Extensions;

namespace Core.Chunking.Debugging
{
    public class NeighbourTestVisualizer : MonoBehaviour
    {
        [SerializeField, ShowOnly] private int selectedIndex = 0;
        [SerializeField] private Int3 selectedClusterPosition;

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;

            Chunk chunk = ChunkClusterDictionary.GetChunkAt(this.transform.position.ToInt3());

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(chunk.Cluster.Position.ToVector3() + Vector3.one * 128, Vector3.one * 16 * 16);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(chunk.GlobalPosition.ToVector3() + Vector3.one * 8, Vector3.one * 16);
            selectedClusterPosition = chunk.Cluster.Position;
        }
    }
}

