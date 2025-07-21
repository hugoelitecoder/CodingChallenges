using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine().Trim());
        var seq = Enumerable.Range(0, N)
                            .Select(_ => int.Parse(Console.ReadLine().Trim()))
                            .ToArray();

        var tails = new List<int>();
        foreach (var x in seq)
        {
            int idx = tails.BinarySearch(x);
            if (idx < 0) idx = ~idx;
            if (idx == tails.Count)
                tails.Add(x);
            else
                tails[idx] = x;
        }

        Console.WriteLine(tails.Count);
    }
}