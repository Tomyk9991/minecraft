using System.Collections.Generic;
using Core.Managers;
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

            foreach (Biom b in bioms)
            {
                if (b.minValue <= value && value < b.maxValue)
                    return b;
            }

            Debug.Log("Biom 0. Fehler?!");
            return bioms[0];

            // return bioms.Find(biom => biom.minValue <= value && value < biom.maxValue);
        }
    }
}
