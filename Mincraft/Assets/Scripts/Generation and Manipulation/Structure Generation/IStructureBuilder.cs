using System.Collections.Generic;
using Core.Builder.Generation;
using Core.Chunks;
using Core.Math;

namespace Core.StructureGeneration
{
    public interface IStructureBuilder
    {
        Queue<(Int3 Origin, Biom Biom)> StructureOrigin { get; set; }
        HashSet<Chunk> Build(Biom biom, Chunk callingChunk, in Int3 origin);
    }
}