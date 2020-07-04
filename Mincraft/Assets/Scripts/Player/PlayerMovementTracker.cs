using System;
using Core.Math;
using Extensions;
using UnityEngine;

namespace Core.Player
{
    public class PlayerMovementTracker : SingletonBehaviour<PlayerMovementTracker>
    {
        public static event Action<Direction> OnDirectionModified;
        public static event Action<int, int> OnChunkPositionChanged;

        public int xPlayerPos = 0;
        public int zPlayerPos = 0;
        [Space]
        public int prevXPlayerPos = 0;
        public int prevZPlayerPos = 0;
        
        private const int chunkSize = 0x10;

        private Int3 latestPlayerPosition;

        private void Start()
        {
            latestPlayerPosition = transform.position.ToInt3();
            
            xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
            zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);
            prevXPlayerPos = xPlayerPos;
            prevZPlayerPos = zPlayerPos;
            OnChunkPositionChanged?.Invoke(xPlayerPos, zPlayerPos);
        }
        
        private void Update()
        {
            latestPlayerPosition = transform.position.ToInt3();
            xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
            zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);

            if (prevXPlayerPos != xPlayerPos || prevZPlayerPos != zPlayerPos)
            {
                Direction d = Direction.None;
                if (xPlayerPos == 0 || zPlayerPos == 0)
                    return;
                
                int resultX = xPlayerPos - prevXPlayerPos;
                int resultY = zPlayerPos - prevZPlayerPos;

                if (resultX > 0)
                    d = Direction.Right;
                else if (resultX < 0)
                    d = Direction.Left;

                if (resultY > 0)
                    d = Direction.Forward;
                else if (resultY < 0)
                    d = Direction.Back;
                
                
                OnDirectionModified?.Invoke(d);
                prevXPlayerPos = xPlayerPos;
                prevZPlayerPos = zPlayerPos;
                OnChunkPositionChanged?.Invoke(xPlayerPos, zPlayerPos);
            }
        }

        public string PlayerPos()
        {
            return latestPlayerPosition.ToString();
        }
    }
    
    public enum Direction
    {
        None = 0b0000,
        Left = 0b0001,
        Right = 0b0010,
        Forward = 0b0100,
        Back = 0b1000,
    }
}
