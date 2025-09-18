using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int V = int.Parse(Console.ReadLine());
        long total = GenerateSmooths()
                     .Take(V)
                     .Sum();
        Console.WriteLine(total);
    }

    static IEnumerable<long> GenerateSmooths()
    {
        var set = new SortedSet<long> { 1 };
        while (true)
        {
            long x = set.Min;
            set.Remove(x);
            yield return x;
            foreach (var f in new[] { 2L, 3L, 5L })
            {
                long y = x * f;
                if (!set.Contains(y))
                    set.Add(y);
            }
        }
    }
}
