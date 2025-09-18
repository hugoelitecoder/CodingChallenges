using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int D = int.Parse(Console.ReadLine());
        int N = int.Parse(Console.ReadLine());
        var names = new string[N];
        var coords = new int[N][];
        for (int i = 0; i < N; i++)
        {
            var line = Console.ReadLine().Trim();
            var paren = line.IndexOf('(');
            names[i] = line.Substring(0, paren);
            var inside = line.Substring(paren + 1, line.Length - paren - 2);
            coords[i] = inside.Split(',')
                              .Select(s => int.Parse(s))
                              .ToArray();
        }

        long minDist2 = long.MaxValue;
        long maxDist2 = long.MinValue;
        int minI = 0, minJ = 1;
        int maxI = 0, maxJ = 1;

        for (int i = 0; i < N; i++)
        {
            for (int j = i + 1; j < N; j++)
            {
                long dist2 = 0;
                for (int k = 0; k < D; k++)
                {
                    var d = coords[j][k] - coords[i][k];
                    dist2 += (long)d * d;
                }
                if (dist2 > 0 && dist2 < minDist2)
                {
                    minDist2 = dist2;
                    minI = i;
                    minJ = j;
                }
                if (dist2 > maxDist2)
                {
                    maxDist2 = dist2;
                    maxI = i;
                    maxJ = j;
                }
            }
        }

        var diffsMin = new List<int>();
        for (int k = 0; k < D; k++)
            diffsMin.Add(coords[minJ][k] - coords[minI][k]);
        Console.WriteLine(
            $"{names[minI]}{names[minJ]}({string.Join(",", diffsMin)})"
        );

        var diffsMax = new List<int>();
        for (int k = 0; k < D; k++)
            diffsMax.Add(coords[maxJ][k] - coords[maxI][k]);
        Console.WriteLine(
            $"{names[maxI]}{names[maxJ]}({string.Join(",", diffsMax)})"
        );
    }
}
