using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;

namespace Core.Builder.Generation
{
    public abstract class TreeGenerator
    {
        public virtual List<ChunkJob> Generate(Chunk chunk, int x, int y, int z)
        {
            return null;
        }
    }
}
