using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Chunks.Threading.Jobs
{
    public interface IJob<T> : IJob
    {
        T Target { get; set; }
    }

    public interface IJob
    {
        void Execute();
    }
}
