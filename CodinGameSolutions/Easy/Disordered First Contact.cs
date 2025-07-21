using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        string MESSAGE = Console.ReadLine();
        List<int> indexMap = new List<int>();
        for (int i = 1; indexMap.Count < MESSAGE.Length; i++)
            indexMap.InsertRange(i % 2 * indexMap.Count,
                                 Enumerable.Range(indexMap.Count,
                                                  Math.Min(i,
                                                           MESSAGE.Length - indexMap.Count)));
        if (N > 0)
            indexMap = indexMap.Select((n, i) => new { n, i })
                               .OrderBy(p => p.n)
                               .Select(p => p.i)
                               .ToList();
        indexMap = indexMap.Select(n =>
        {
            for (int i = 1; i < Math.Abs(N); i++)
                n = indexMap[n];
            return n;
        }).ToList();
        Console.WriteLine(string.Concat(indexMap.Select(i => MESSAGE[i])));
    }
}
