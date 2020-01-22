using System.Collections.Concurrent;
using UnityEngine;
using System.Threading;
using System;
using Core.Performance.Parallelisation;
using UnityEditor;

namespace Core.Chunking.Threading
{
    public class NoiseJobManager : AutoThreadCollection<NoiseJob>, IDisposable
    {
        public static NoiseJobManager NoiseJobManagerUpdaterInstance { get; private set; }
        public int Count => this.jobs.Count; 

        public NoiseJobManager(int amountThreads, bool noiseUpdaterInstance = false) : base(amountThreads)
        {
            if (noiseUpdaterInstance)
                NoiseJobManagerUpdaterInstance = this;
        }

        public override void JobExecute(NoiseJob job)
        {
            for (int i = 0; i < job.Column.chunks.Length; i++)
            {
                job.Column.chunks[i].GenerateBlocks();
            }

            job.Column.State = DrawingState.NoiseReady;
        }
        
        public void Dispose()
        {
            base.Stop();
        }
    }
}
