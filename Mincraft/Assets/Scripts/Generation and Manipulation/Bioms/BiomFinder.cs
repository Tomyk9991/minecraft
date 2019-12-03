using Core.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Builder.Generation
{
    public static class BiomFinder
    {
        private static List<Biom> bioms;

        /// <summary>
        /// Finds the correct biom based an the noise
        /// </summary>
        /// <param name="value">Value between -1 and 1</param>
        /// <returns></returns>
        public static Biom Find(float value)
        {
            if (bioms == null)
                bioms = WorldSettings.Instance.Bioms;

            return bioms.Find(biom => biom.minValue <= value && value < biom.maxValue);
            return null;
        }
    }
}
