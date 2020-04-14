using System.Collections.Generic;
using Core.Builder.Generation;
using Core.Chunking;
using Core.Math;

namespace Core.StructureGeneration
{
    public interface IStructureBuilder
    {
        Queue<(Int3 Origin, Biom Biom)> StructureOrigin { get; set; }
        void Build(Biom biom, Chunk callingChunk, in Int3 origin);
    }
}