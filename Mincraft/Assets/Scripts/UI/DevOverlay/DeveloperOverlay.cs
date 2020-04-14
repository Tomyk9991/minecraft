﻿using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

using Core.Chunking;
using Core.Chunking.Threading;
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
        [SerializeField] private TextMeshProUGUI amountNoiseJobsOutput = null;
        [SerializeField] private TextMeshProUGUI amountChunkJobsOutput = null;

        private Transform[] transforms = null;
        private Timer timer;
        private JobManager jobmanager;

        private StringBuilder stringbuilder;

        private void Start()
        {
            jobmanager = JobManager.JobManagerUpdaterInstance;
            stringbuilder = new StringBuilder();
            
            List<Transform> t = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                t.Add(transform.GetChild(i));
            }

            timer = new Timer(WorldSettings.WorldTick);
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
                chunksLoadedOutput.SetText(GetLoadedChunksAmount());
                amountNoiseJobsOutput.SetText(GetNoiseJobCount());
                amountChunkJobsOutput.SetText(GetChunkJobCount());
            }
        }

        private StringBuilder GetNoiseJobCount()
        {
            stringbuilder.Clear();
            return stringbuilder.Append(jobmanager.NoiseJobsCount * ChunkBuffer.YBound);
        }

        private StringBuilder GetChunkJobCount()
        {
            stringbuilder.Clear();
            return stringbuilder.Append(jobmanager.MeshJobsCount).Append(" finished jobs: ").Append(jobmanager.FinishedJobsCount);
        }
        

        private StringBuilder GetLoadedChunksAmount()
        {
            stringbuilder.Clear();
            return stringbuilder.Append(ChunkBuffer.DataLength * ChunkBuffer.YBound);
        }
    }
}