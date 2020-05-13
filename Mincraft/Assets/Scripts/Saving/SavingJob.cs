using System;
using System.Collections.Concurrent;
using System.Threading;
using Core.Chunks;
using Core.Managers;

namespace Core.Saving
{
    public class SavingJob : IDisposable
    {
        private Thread thread;
        private bool running = false;
        private ContextIO<Chunk> chunkSaver;

        private ConcurrentQueue<Chunk> chunksToSave;

        public SavingJob()
        {
            thread = new Thread(Save);
            chunksToSave = new ConcurrentQueue<Chunk>();
            chunkSaver = new ContextIO<Chunk>(ContextIO.DefaultPath + "/" + GameManager.CurrentWorldName  + "/");
        }

        public void Start()
        {
            running = true;
            thread.Start();
            //thread.IsBackground = true;
        }

        public void AddToSavingQueue(Chunk chunk)
        {
            chunksToSave.Enqueue(chunk);
        }

        private void Save()
        {
            while (running)
            {
                if (chunksToSave.TryDequeue(out Chunk c))
                {
                    if (c != null)
                    {
                        chunkSaver.SaveContext(c, "temp");
                        chunkSaver.Swap(c.LocalPosition.ToString(), "temp");
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }


        public void Dispose()
        {
            running = false;
        }
    }
}
