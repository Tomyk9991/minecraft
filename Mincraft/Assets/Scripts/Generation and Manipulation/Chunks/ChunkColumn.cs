using System;
using System.Runtime.Remoting.Messaging;
using Core.Math;
using UnityEngine;

namespace Core.Chunks
{
    public class ChunkColumn
    {
        public Int2 GlobalPosition { get; set; }
        public Chunk[] Chunks { get; set; }
        private bool dirty = false;

        /// <summary>
        /// Discribes the local position, which depends on the global player position 
        /// </summary>
        public Int2 LocalPosition { get; set; }

        public Chunk this[int index]
        {
            get => Chunks[index];  
            set => Chunks[index] = value;
        }
        
        public ChunkColumn(Int2 globalPosition, Int2 localPosition, int minYHeight, int maxYHeight)
        {
            this.GlobalPosition = globalPosition;
            this.LocalPosition = localPosition;
            Chunks = new Chunk[System.Math.Abs(minYHeight / 16) + System.Math.Abs(maxYHeight / 16)];
        }

        public ChunkColumn[] ChunkColumnNeighbours()
        {
            return new[] 
            {
                //Left
                ChunkBuffer.GetChunkColumn(this.LocalPosition.X - 1, this.LocalPosition.Y),
                //Right
                ChunkBuffer.GetChunkColumn(this.LocalPosition.X + 1, this.LocalPosition.Y),
                //Forward
                ChunkBuffer.GetChunkColumn(this.LocalPosition.X, this.LocalPosition.Y + 1),
                //Back
                ChunkBuffer.GetChunkColumn(this.LocalPosition.X, this.LocalPosition.Y - 1)
            };
        }
    }

    [Flags]
    public enum DrawingState
    {
        None = 0,
        Drawn = 1,
        NoiseReady = 2,
        InDrawingQueue = 3
    }
}