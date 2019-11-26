using System;
using Core.Math;
using UnityEngine;

namespace Core.Chunking
{
    public class ChunkColumn
    {
        public bool DesiredForVisualization { get; set; }
        public Int2 GlobalPosition { get; set; }
        public Chunk[] chunks;
        private DrawingState state;

        private int minHeight = 0;
        private int maxHeight = 0;
        /// <summary>
        /// Discribes the local position, which depends on the global player position 
        /// </summary>
        public Int2 LocalPosition { get; set; }
        public DrawingState State
        {
            get
            {
                lock (_drawingstateMutex)
                {
                    return state;
                }
            }
            set
            {
                lock (_drawingstateMutex)
                {
                    this.state = value;
                }
            }
        }
        public Chunk this[int index]
        {
            get
            {
                lock (_mutex)
                {
                    return chunks[index];
                }
            }
            set
            {
                lock (_mutex)
                {
                    chunks[index] = value;
                }
            }
        }

        private object _mutex = new object();
        private object _drawingstateMutex = new object();


        public ChunkColumn(Int2 globalPosition, Int2 localPosition, int minYHeight, int maxYHeight)
        {
            this.GlobalPosition = globalPosition;
            this.LocalPosition = localPosition;
            chunks = new Chunk[System.Math.Abs(minYHeight / 16) + System.Math.Abs(maxYHeight / 16)];
            this.minHeight = minYHeight;
            this.maxHeight = maxYHeight;
        }

        public ChunkColumn[] Neighbours()
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

    public enum DrawingState
    {
        None = 0,
        Drawn = 1,
        NoiseReady = 2,
        InNoiseQueue = 3
    }
}