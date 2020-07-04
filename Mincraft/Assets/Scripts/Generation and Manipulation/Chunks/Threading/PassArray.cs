using System.Collections.Generic;

namespace Core.Chunks.Threading
{
    public class PassArray
    {
        private List<Pass> passes;
        public PassArray(int length)
        {
            this.passes = new List<Pass>(length);
        }

        public Pass CurrentPass
        {
            get
            {
                lock (passes)
                {
                    if (this.passes.Count != 0)
                    {
                        return this.passes[0];
                    }

                    return null;
                }
            }
        } 

        public void Add(Pass pass)
        {
            lock (passes)
            {
                this.passes.Add(pass);
            }
        }

        public void RemoveCurrent()
        {
            lock (passes)
            {
                if (this.passes.Count != 0)
                {
                    this.passes.RemoveAt(0);
                }
            }
        }
    }
}