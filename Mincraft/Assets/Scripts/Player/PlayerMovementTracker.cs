using System;
using Core.Math;
using Extensions;
using UnityEngine;

namespace Core.Player
{
    public class PlayerMovementTracker : SingletonBehaviour<PlayerMovementTracker>
    {
        public static event Action<Direction> OnDirectionModified;

        public int xPlayerPos = 0;
        public int zPlayerPos = 0;
        private int privateXPlayerPos = 0;
        private int privateZPlayerPos = 0;
        [Space]
        public int prevXPlayerPos = 0;
        public int prevZPlayerPos = 0;
        
        private const int chunkSize = 0x10;

        private Int3 latestPlayerPosition;

        private void Start()
        {
            UpdateLatestPlayerPosition();

            if (latestPlayerPosition.X > -chunkSize && latestPlayerPosition.X < chunkSize ||
                latestPlayerPosition.Z > -chunkSize && latestPlayerPosition.Z < chunkSize)
            {
                privateXPlayerPos = (latestPlayerPosition.X + 48) / chunkSize;
                privateZPlayerPos = (latestPlayerPosition.Z + 48) / chunkSize;
            }
            else
            {
                privateXPlayerPos = latestPlayerPosition.X / chunkSize;
                privateZPlayerPos = latestPlayerPosition.Z / chunkSize;
            }

            xPlayerPos = latestPlayerPosition.X / chunkSize;
            zPlayerPos = latestPlayerPosition.Z / chunkSize;

            prevXPlayerPos = privateXPlayerPos;
            prevZPlayerPos = privateZPlayerPos;
        }
        
        private void Update()
        {
            UpdateLatestPlayerPosition();

            if (latestPlayerPosition.X > -chunkSize && latestPlayerPosition.X < chunkSize ||
                latestPlayerPosition.Z > -chunkSize && latestPlayerPosition.Z < chunkSize)
            {
                privateXPlayerPos = (latestPlayerPosition.X + 48) / chunkSize;
                privateZPlayerPos = (latestPlayerPosition.Z + 48) / chunkSize;
            }
            else
            {
                privateXPlayerPos = latestPlayerPosition.X / chunkSize;
                privateZPlayerPos = latestPlayerPosition.Z / chunkSize;
            }
            

            if (privateXPlayerPos != prevXPlayerPos || privateZPlayerPos != prevZPlayerPos)
            {
                CalcDirection();
            }
            
            xPlayerPos = latestPlayerPosition.X / chunkSize;
            zPlayerPos = latestPlayerPosition.Z / chunkSize;
        }

        private void CalcDirection()
        {
            var xDirection = System.Math.Sign(privateXPlayerPos - prevXPlayerPos);
            var zDirection = System.Math.Sign(privateZPlayerPos - prevZPlayerPos);

            if (xDirection != 0)
            {
                var hor = xDirection == -1 ? Direction.Left : Direction.Right;
                OnDirectionModified?.Invoke(hor);
            }

            if (zDirection != 0)
            {
                var ver = zDirection == -1 ? Direction.Back : Direction.Forward;
                OnDirectionModified?.Invoke(ver);
            }
            
            prevXPlayerPos = privateXPlayerPos;
            prevZPlayerPos = privateZPlayerPos;
        }

        private void UpdateLatestPlayerPosition()
        {
            latestPlayerPosition.X = Mathf.RoundToInt(transform.position.x);
            latestPlayerPosition.Y = Mathf.RoundToInt(transform.position.y);
            latestPlayerPosition.Z = Mathf.RoundToInt(transform.position.z);
        }

        public string PlayerPos()
        {
            return latestPlayerPosition.ToString();
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
