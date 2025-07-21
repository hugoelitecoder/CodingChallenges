using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var mask = Convert.ToUInt64(Console.ReadLine().Trim(), 16);
        var weights = new List<ulong>();
        for (int b = 0; b < 64; b++)
            if (((mask >> b) & 1UL) != 0)
                weights.Add(1UL << b);
        weights.Sort();

        int pop = weights.Count;
        if (pop <= 4)
        {
            var all = new List<ulong>();
            for (ulong sub = mask; sub != 0; sub = (sub - 1) & mask)
                all.Add(sub);
            all.Sort();
            Console.WriteLine(string.Join(",", all));
            return;
        }

        const int M = 13, L = M + 1;
        var best = new List<ulong> { 0UL };
        foreach (var w in weights)
        {
            var list2 = best.Select(x => x + w).ToList();
            var merged = new List<ulong>(L);
            int i = 0, j = 0;
            while (merged.Count < L && (i < best.Count || j < list2.Count))
            {
                if (j >= list2.Count || (i < best.Count && best[i] < list2[j]))
                    merged.Add(best[i++]);
                else
                    merged.Add(list2[j++]);
            }
            best = merged;
        }
        var first13 = best.Skip(1).Take(M).ToList();

        var lowbit = mask & (~(mask - 1UL));
        var secondLargest = mask - lowbit;
        var largest = mask;

        var parts = first13.Select(u => u.ToString()).ToList();
        parts.Add("...");
        parts.Add(secondLargest.ToString());
        parts.Add(largest.ToString());
        Console.WriteLine(string.Join(",", parts));
    }
}
