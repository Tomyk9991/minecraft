using System.Text;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Math;
using Core.Player;
using UnityEditor;
using UnityEngine;

namespace Testing
{
    public class CreateTreeOnClickTest : MonoBehaviour
    {
        private Int2 playerPos = Int2.Zero;
        private Vector3 chunkPos = Vector3.zero;

        private int drawDistance;
        private const int CHUNKSIZE = 0x10;

        //private MeshJobManager _meshJobManager;
        private JobManager _jobManager;

        private void Start()
        {
            PlayerMovementTracker.OnChunkPositionChanged += (x, y) =>
            {
                playerPos.X = x;
                playerPos.Y = y;
            };

            _jobManager = JobManager.JobManagerUpdaterInstance;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                
            }
        }
    }

    [CustomEditor(typeof(CreateTreeOnClickTest))]
    public class CreateTreeOnClickTestEditor : Editor
    {
        private StringBuilder stringbuilder;
        private JobManager jobManager;
        public override void OnInspectorGUI()
        {
            if (stringbuilder == null)
                stringbuilder = new StringBuilder();
            
            if(jobManager == null)
                jobManager = JobManager.JobManagerUpdaterInstance;
            
            base.OnInspectorGUI();


            if (GUILayout.Button("Rerender"))
            {
                int len = ChunkBuffer.Dimension;

                for (int i = 0; i < len; i++)
                {
                    for (int j = 0; j < len; j++)
                    {
                        ChunkColumn column = ChunkBuffer.GetChunkColumn(i, j);
                        for (int k = 0; k < column.Chunks.Length; k++)
                        {
                            // jobManager.AddWithoutNoise(new MeshJob(column[k]));
                        }
                    }
                }
            }

            if (Application.isPlaying)
            {
                GUILayout.Label("Active noisejobs: " + GetNoiseJobCount().ToString() + "\n" + 
                                "Active meshjobs: " + GetChunkJobCount().ToString() + "\n" + 
                                "Active Chunks: " + GetLoadedChunksAmount().ToString());
                
                Repaint();
            }
            
        }
        
        private StringBuilder GetNoiseJobCount()
        {
            stringbuilder.Clear();
            return stringbuilder.Append(jobManager.NoiseJobsCount * ChunkBuffer.YBound);
        }

        private StringBuilder GetChunkJobCount()
        {
            stringbuilder.Clear();
            return stringbuilder.Append(jobManager.MeshJobsCount).Append(" finished jobs: ").Append(jobManager.FinishedJobsCount);
        }
        

        private StringBuilder GetLoadedChunksAmount()
        {
            stringbuilder.Clear();
            return stringbuilder.Append(ChunkBuffer.DataLength * ChunkBuffer.YBound);
        }
    }
}