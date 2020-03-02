using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;

using Core.Chunking;
using Core.Chunking.Threading;
using Core.Performance.Parallelisation;
using Core.Player;
using Utilities;

namespace Core.UI.DeveloperOverlay
{
    public class DeveloperOverlay : MonoBehaviour
    {
        [Header("Developer overlay")]
        [SerializeField] private bool showingOverlay = false;

        [Header("Outputs")]
        [SerializeField] private TextMeshProUGUI playerPositionOutput = null;
        [SerializeField] private TextMeshProUGUI chunksLoadedOutput = null;
        [SerializeField] private TextMeshProUGUI chunksInGameObjectOutput = null;
        [SerializeField] private TextMeshProUGUI cpuUsageOutput = null;
        [SerializeField] private TextMeshProUGUI amountNoiseJobsOutput = null;
        [SerializeField] private TextMeshProUGUI amountChunkJobsOutput = null;

        [Header("Calculations")]
        [SerializeField] private Transform worldParent = null;

        private Transform[] transforms = null;
        private PerformanceCounter theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private Timer timer;

        private void Start()
        {
            List<Transform> t = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                t.Add(transform.GetChild(i));
            }

            //timer = new Timer(WorldSettings.WorldTick);
            timer = new Timer(0.00001f);
            this.transforms = t.ToArray();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                showingOverlay = !showingOverlay;

                foreach (Transform rectTransform in transforms)
                {
                    rectTransform.gameObject.SetActive(showingOverlay);
                }
            }

            if (showingOverlay && timer.TimeElapsed(Time.deltaTime))
            {
                playerPositionOutput.text = PlayerMovementTracker.Instance.PlayerPos();
                chunksLoadedOutput.text = GetLoadedChunksAmount().ToString();
                chunksInGameObjectOutput.text = GetAmountChunksInGameObjects().ToString();
                cpuUsageOutput.text = GetCPUUsage();
                amountNoiseJobsOutput.text = GetNoiseJobCount();
                amountChunkJobsOutput.text = GetChunkJobCount();
            }
        }

        private string GetNoiseJobCount()
        {
            return (JobManager.JobManagerUpdaterInstance.NoiseJobsCount * ChunkBuffer.YBound).ToString();
            //return (NoiseJobManager.NoiseJobManagerUpdaterInstance.Count * ChunkBuffer.YBound).ToString();
        }

        private string GetChunkJobCount()
        {
            return JobManager.JobManagerUpdaterInstance.MeshJobsCount + " finished jobs: " +
                   JobManager.JobManagerUpdaterInstance.FinishedJobsCount;
            //return MeshJobManager.MeshJobManagerUpdaterInstance.JobsCount + " finished jobs: " + MeshJobManager.MeshJobManagerUpdaterInstance.FinishedJobsCount;
        }
        

        private int GetLoadedChunksAmount()
        {
            return ChunkBuffer.Data.Length * ChunkBuffer.YBound;
        }

        private int GetAmountChunksInGameObjects()
        {
            return worldParent.transform.Cast<Transform>().Count(t => t.name != "Unused chunk");
        }

        private string GetCPUUsage()
        {
            return theCPUCounter.NextValue() + "%";
        }
    }
}