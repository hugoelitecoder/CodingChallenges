using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split();
        var a = int.Parse(parts[0]);
        var b = int.Parse(parts[1]);
        Console.WriteLine(CountPolygons(a, b));
    }

    private static int CountPolygons(int a, int b)
    {
        var fermat = new[] { 3, 5, 17, 257, 65537 };
        var bases  = new List<long>();
        DFS(0, 1, fermat, b, bases);

        var count = 0;
        foreach (var odd in bases)
        {
            var val = odd;
            while (val <= b)
            {
                if (val >= a) count++;
                val <<= 1;
            }
        }
        return count;
    }

    private static void DFS(int idx, long current, int[] fermat, int max, List<long> outList)
    {
        if (idx == fermat.Length)
        {
            outList.Add(current);
            return;
        }
        DFS(idx + 1, current, fermat, max, outList);
        var next = current * fermat[idx];
        if (next <= max)
            DFS(idx + 1, next, fermat, max, outList);
    }
}
