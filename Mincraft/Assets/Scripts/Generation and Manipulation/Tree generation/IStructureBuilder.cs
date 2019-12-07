using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Core.Chunking;
using Core.Chunking.Threading;
using Core.Math;

namespace Core.Builder.Generation
{
    public interface IStructureBuilder
    {
        /// <summary>
        /// Generates to the root-chunk a tree automatically to the neighbouring chunks if needed
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        void Generate(Chunk rootChunk, Biom biom, int x, int y, int z);
    }
}
