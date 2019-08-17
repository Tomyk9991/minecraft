﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

using Core.Chunking;
using Core.Math;
using Extensions;

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
        [SerializeField] private TextMeshProUGUI chunksInHashmapOutput = null;
        [SerializeField] private TextMeshProUGUI cpuUsageOutput = null;

        [Header("Calculations")]
        [SerializeField] private Transform playerTarget = null;
        [SerializeField] private Transform worldParent = null;
        
        private Transform[] transforms = null;
        private ChunkUpdater _chunkUpdater;

        private void Start()
        {
            List<Transform> t = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                t.Add(transform.GetChild(i));
            }

            this.transforms = t.ToArray();
            _chunkUpdater = ChunkUpdater.Instance;
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

            if (showingOverlay)
            {
                playerPositionOutput.text = GetPlayerPosition().ToString();
                chunksLoadedOutput.text = GetLoadedChunksAmount().ToString();
                chunksInGameObjectOutput.text = GetAmountChunksInGameObjects().ToString();
                chunksInHashmapOutput.text = GetAmountChunksInHashMap().ToString();
                cpuUsageOutput.text = GetCPUUsage();
            }
        }

        private Int3 GetPlayerPosition()
        {
            return playerTarget.position.ToInt3();
        }

        private int GetLoadedChunksAmount()
        {
            return ChunkClusterDictionary.Count;
        }

        //Änderbar mit GameobjectPool
        private int GetAmountChunksInGameObjects()
        {
            return _chunkUpdater.chunks.Count;
        }

        private int GetAmountChunksInHashMap()
        {
            return HashSetPositionChecker.Count;
        }

        private string GetCPUUsage()
        {
            return "0%";
        }
    }
}
