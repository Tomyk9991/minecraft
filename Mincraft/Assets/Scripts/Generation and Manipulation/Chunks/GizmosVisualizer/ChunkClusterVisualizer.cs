using System.Collections.Generic;
using UnityEngine;

using Core.Chunking;
using Core.Math;
using UnityInspector;

namespace Testing.Gizmos
{
    public class ChunkClusterVisualizer : MonoBehaviour
    {
        [SerializeField, ShowOnly] private int selectedIndex = 0;
        [SerializeField] private Int3 selectedClusterPosition = Int3.Zero;
        
        private int length = 0;

        [ContextMenu("Next Index")]
        public void NextIndex()
        {
            selectedIndex = (selectedIndex + 1) % length;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
            
            List<ChunkCluster> clusters = ChunkClusterDictionary.GetActiveChunkClusters();
            length = clusters.Count;
            
            UnityEngine.Gizmos.DrawWireCube(clusters[selectedIndex].Position.ToVector3(), Vector3.one * 16 * 16);
            selectedClusterPosition = clusters[selectedIndex].Position;
        }
    }
}
