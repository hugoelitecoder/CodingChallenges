using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var m = int.Parse(Console.ReadLine());
        var n = int.Parse(Console.ReadLine());
        var bars = Console.ReadLine()
            .Split(' ')
            .Select(int.Parse)
            .ToArray();

        var bestSum = -1;
        var bestCount = int.MaxValue;
        var bestSeq = new int[0];

        var totalMasks = 1 << n;
        for (var mask = 0; mask < totalMasks; mask++)
        {
            var sum = 0;
            var seq = new List<int>();
            for (var i = 0; i < n; i++)
            {
                if (((mask >> i) & 1) == 0) continue;
                sum += bars[i];
                if (sum > m) break;
                seq.Add(bars[i]);
            }
            if (sum > m) continue;
            var count = seq.Count;
            if (sum > bestSum
                || (sum == bestSum && count < bestCount)
                || (sum == bestSum && count == bestCount && IsLexSmaller(seq, bestSeq)))
            {
                bestSum = sum;
                bestCount = count;
                bestSeq = seq.ToArray();
            }
        }

        Console.WriteLine(string.Join(" ", bestSeq));
    }

    static bool IsLexSmaller(IList<int> a, IList<int> b)
    {
        for (var i = 0; i < a.Count; i++)
        {
            if (a[i] < b[i]) return true;
            if (a[i] > b[i]) return false;
        }
        return false;
    }
}
