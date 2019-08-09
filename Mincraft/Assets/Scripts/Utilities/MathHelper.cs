using System;
using UnityEngine;

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
            n = Math.Abs(n);
            isNegative = true;
        }

        if (x > n)
            return isNegative ? -x : x;
        n = n + x / 2;
        n = n - (n % x);
        return isNegative ? -n : n;
    }
    
    public static float Map(float n, float start1, float stop1, float start2, float stop2)
        => ((n - start1) / (stop1 - start1)) * (stop2 - start2) + start2;
}