using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var n = long.Parse(Console.ReadLine());
        var visibleBuildings = SightCalculator.CountVisibleBuildings(n);
        Console.WriteLine(visibleBuildings);
    }
}

public static class SightCalculator
{
    public static long CountVisibleBuildings(long n)
    {
        var k = (int)((n - 1) / 2);
        var phi = SievePhi(k);
        long sumOfPhi = 0;
        for (var i = 1; i <= k; i++)
        {
            sumOfPhi += phi[i];
        }
        return 8 * sumOfPhi;
    }

    private static long[] SievePhi(int limit)
    {
        var phi = new long[limit + 1];
        for (var i = 0; i <= limit; i++)
        {
            phi[i] = i;
        }
        for (var p = 2; p <= limit; p++)
        {
            if (phi[p] == p)
            {
                for (var j = p; j <= limit; j += p)
                {
                    phi[j] -= phi[j] / p;
                }
            }
        }
        return phi;
    }
}