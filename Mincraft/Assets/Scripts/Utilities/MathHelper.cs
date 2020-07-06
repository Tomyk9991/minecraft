using System;
using UnityEngine;

namespace Core.Math
{
    public static class MathHelper
    {
        /// <summary>
        /// Multiple of x closest to n
        /// </summary>
        public static int ClosestMultiple(int n, int x)
        {
            bool isNegative = false;
            if (n == 0) return 0;
            if (n < 0)
            {
                n = System.Math.Abs(n);
                isNegative = true;
            }

            if (x > n)
                return isNegative ? -x : x;
            n = n + x / 2;
            n = n - (n % x);
            return isNegative ? -n : n;
        }

        public static bool InChunkSpace(in Int3 pos)
            => pos.X >= 0 && pos.X < 16 &&
               pos.Y >= 0 && pos.Y < 16 &&
               pos.Z >= 0 && pos.Z < 16;

        public static int MultipleFloor(int n, int x)
            => Mathf.FloorToInt(n / (float) x) * x;

        /// <summary>
        /// Maps n from a range of [istart, istop] to [ostart, ostop]
        /// </summary>
        public static int MapToInt(float value, float istart, float istop, float ostart, float ostop)
            => (int) (ostart + (ostop - ostart) * ((value - istart) / (istop - istart)));

        /// <summary>
        /// Maps n from a range of [istart, istop] to [ostart, ostop]
        /// </summary>
        public static float Map(float value, float istart, float istop, float ostart, float ostop)
            => ostart + (ostop - ostart) * ((value - istart) / (istop - istart));

        public static bool BorderBlockPlusOne(in Int3 pos)
            => pos.X == -1 || pos.X == 16 ||
               pos.Y == -1 || pos.Y == 16 ||
               pos.Z == -1 || pos.Z == 16;

        public static bool BorderBlock(in Int3 pos)
            => pos.X == 0 || pos.X == 15 ||
               pos.Y == 0 || pos.Y == 15 ||
               pos.Z == 0 || pos.Z == 15;
    }
}