using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var compA = ParseCompressed(Console.ReadLine());
        var compB = ParseCompressed(Console.ReadLine());
        var i = 0;
        var j = 0;
        var dotProduct = 0L;
        var remainA = compA[0].Item1;
        var valA = compA[0].Item2;
        var remainB = compB[0].Item1;
        var valB = compB[0].Item2;

        while (i < compA.Count && j < compB.Count)
        {
            var overlap = remainA < remainB ? remainA : remainB;
            dotProduct += overlap * valA * valB;
            remainA -= overlap;
            remainB -= overlap;

            if (remainA == 0)
            {
                i++;
                if (i < compA.Count)
                {
                    remainA = compA[i].Item1;
                    valA = compA[i].Item2;
                }
            }

            if (remainB == 0)
            {
                j++;
                if (j < compB.Count)
                {
                    remainB = compB[j].Item1;
                    valB = compB[j].Item2;
                }
            }
        }

        Console.WriteLine(dotProduct);
    }

    static List<(long, long)> ParseCompressed(string line)
    {
        var parts = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
        var result = new List<(long, long)>();
        for (var i = 0; i < parts.Length; i += 2)
        {
            var count = long.Parse(parts[i]);
            var value = long.Parse(parts[i + 1]);
            result.Add((count, value));
        }
        return result;
    }
}
