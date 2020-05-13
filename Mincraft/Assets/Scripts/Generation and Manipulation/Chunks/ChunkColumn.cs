using System;
using System.Runtime.Remoting.Messaging;
using Core.Math;
using UnityEngine;

namespace Core.Chunks
{
    public class ChunkColumn
    {
        public Int2 GlobalPosition { get; set; }
        public Chunk[] chunks;
        // private DrawingState state;

        private bool dirty = false;
        private object _dirtyStateMutex = new object();
        
        
        /// <summary>
        /// Discribes the local position, which depends on the global player position 
        /// </summary>
        public Int2 LocalPosition { get; set; }
        // public DrawingState State
        // {
        //     get => state;
        //     set => this.state = value;
        // }

        public bool Dirty
        {
            get { lock (_dirtyStateMutex) { return dirty; } }
            set { lock (_dirtyStateMutex) { this.dirty = value; } }
        }
        
        public Chunk this[int index]
        {
            get => chunks[index];  
            set => chunks[index] = value;
        }
        
        public ChunkColumn(Int2 globalPosition, Int2 localPosition, int minYHeight, int maxYHeight)
        {
            this.GlobalPosition = globalPosition;
            this.LocalPosition = localPosition;
            //TODO State
            //this.State = DrawingState.None;
            chunks = new Chunk[System.Math.Abs(minYHeight / 16) + System.Math.Abs(maxYHeight / 16)];
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