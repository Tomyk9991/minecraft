using System;
using System.Collections.Generic;
using Core.Math;
using Core.UI.Console;
using Extensions;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Core.Player
{
    public class PlayerMovementTracker : SingletonBehaviour<PlayerMovementTracker>
    {
        [SerializeField] private float DistanceThreshold = 16.0f;
        public static event Action<Direction> OnDirectionModified;

        //Variablen für Initialisierung in ChunkUpdater
        public int xPlayerPos = 0;
        public int zPlayerPos = 0;
        [Space] private Int3 latestPlayerPosition;
        private Int3 prevLatestPlayerPosition;

        private static Int3 latestStandingBlock;
        public static Int3 CurrentStandingBlock => latestStandingBlock;

        private Int3 deltaResult;

        private void Start()
        {
            UpdateLatestPlayerPosition();
            
            xPlayerPos = latestPlayerPosition.X;
            zPlayerPos = latestPlayerPosition.Z;


            prevLatestPlayerPosition.X = latestPlayerPosition.X;
            prevLatestPlayerPosition.Z = latestPlayerPosition.Z;
        }

        private void Update()
        {
            UpdateLatestPlayerPosition();
            
            deltaResult = latestPlayerPosition - prevLatestPlayerPosition;
            
            if (System.Math.Abs(deltaResult.X) >= DistanceThreshold)
            {
                OnDirectionModified?.Invoke(deltaResult.X > 0 ? Direction.Right : Direction.Left);
                prevLatestPlayerPosition.X = latestPlayerPosition.X;
            }

            if (System.Math.Abs(deltaResult.Z) >= DistanceThreshold)
            {
                OnDirectionModified?.Invoke(deltaResult.Z > 0 ? Direction.Forward : Direction.Back);
                prevLatestPlayerPosition.Z = latestPlayerPosition.Z;
            }
        }

        private void UpdateLatestPlayerPosition()
        {
            latestStandingBlock.X = (int) transform.position.x;
            latestStandingBlock.Y = (int) transform.position.y;
            latestStandingBlock.Z = (int) transform.position.z;


            latestPlayerPosition.X = Mathf.RoundToInt(transform.position.x);
            latestPlayerPosition.Y = Mathf.RoundToInt(transform.position.y);
            latestPlayerPosition.Z = Mathf.RoundToInt(transform.position.z);
        }

        public string PlayerPos()
        {
            return latestPlayerPosition.ToString();
        }

        [ConsoleMethod(nameof(MovePlayer))]
        private void MovePlayer(int x, int y, int z)
        {
            transform.position = new Vector3(x, y, z);
        }
    }

    public enum Direction : sbyte
    {
        None = 0,
        Left = 1,
        Right = 2,
        Forward = 4,
        Back = 8,
    }
}