using System.Collections.Generic;
using Core.Builder.Generation;
using UnityEngine;

namespace Core.Chunks.Threading
{
    [CreateAssetMenu(fileName = "BiomScriptable", menuName = "Scriptable Objects/Biom")]
    public class BiomScriptable : ScriptableObject
    {
        public List<Biom> bioms = null;
    }
}