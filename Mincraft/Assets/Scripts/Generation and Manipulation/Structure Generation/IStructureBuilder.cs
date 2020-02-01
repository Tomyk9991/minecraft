using System.Collections.Generic;
using Core.Builder;

namespace Core.StructureGeneration
{
    public interface IStructureBuilder
    {
        IEnumerable<Block> NextBlock();
    }
}