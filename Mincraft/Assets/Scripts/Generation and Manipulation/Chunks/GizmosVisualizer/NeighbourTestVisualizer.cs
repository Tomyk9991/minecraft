using System.Linq;
using Core.Chunking.Threading;
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
        
        [SerializeField] private GameObject player = null;
        

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !ChunkSettings.Instance.drawGizmosChunks) return;

            Chunk chunk = ChunkClusterDictionary.GetChunkAt(this.transform.position.ToInt3());

            if (chunk == null) return;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(chunk.Cluster.Position.ToVector3() + Vector3.one * 128, Vector3.one * 16 * 16);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(chunk.GlobalPosition.ToVector3() + Vector3.one * 8, Vector3.one * 16);
            selectedClusterPosition = chunk.Cluster.Position;
        }

        [ContextMenu("Redraw chunk")]
        private void Redraw()
        {
            Chunk c = ChunkClusterDictionary.GetChunkAt(this.transform.position.ToInt3());

            ChunkJob job = new ChunkJob();
            job.CreateChunkFromExisting(c);

            ChunkJobManager.ChunkJobManagerUpdaterInstance.Add(job);
        }

        [ContextMenu("Chunkcleanup test")]
        private void Test()
        {
            Int3 temp = transform.position.ToInt3();
            Int2 position = new Int2(temp.X, temp.Z);
            
            Int3 latestPlayerPosition = player.transform.position.ToInt3();
            
            int xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, ChunkSettings.ChunkSize);
            int zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, ChunkSettings.ChunkSize);

            ChunkCleanup cleanup = new ChunkCleanup(ChunkSettings.DrawDistance, ChunkSettings.ChunkSize);

            Debug.Log("Inside draw-Distance: " + cleanup.InsideDrawDistance(position, xPlayerPos, zPlayerPos));
            Debug.Log("In Hashset: " + HashSetPositionChecker.Contains(position));

            Debug.Log("In Dictionary: " + ChunkClusterDictionary.Contains(temp));
        }
    }
}

