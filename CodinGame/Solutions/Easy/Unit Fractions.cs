using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var n = long.Parse(Console.ReadLine());
        var pf = Factor(n);
        var divisors = new List<long>();
        BuildDivisors(new List<KeyValuePair<long, int>>(pf), 0, 1, divisors);

        var n2 = n * n;
        var results = new List<(long x, long y)>();

        foreach (var d in divisors)
        {
            if (d < n) continue;
            var x = d + n;
            var y = n2 / d + n;
            if (x >= y) results.Add((x, y));
        }

        results.Sort((a, b) => b.x.CompareTo(a.x));
        foreach (var (x, y) in results)
            Console.WriteLine($"1/{n} = 1/{x} + 1/{y}");
    }

    static Dictionary<long, int> Factor(long v)
    {
        var res = new Dictionary<long, int>();
        for (long p = 2; p * p <= v; p++)
        {
            while (v % p == 0)
            {
                if (!res.ContainsKey(p)) res[p] = 0;
                res[p]++;
                v /= p;
            }
        }
        if (v > 1)
        {
            if (!res.ContainsKey(v)) res[v] = 0;
            res[v]++;
        }
        return res;
    }

    static void BuildDivisors(List<KeyValuePair<long, int>> pf, int i, long cur, List<long> outDivs)
    {
        if (i == pf.Count)
        {
            outDivs.Add(cur);
            return;
        }

        var (p, exp) = pf[i];
        var maxE = exp * 2;
        long val = 1;
        for (var e = 0; e <= maxE; e++)
        {
            BuildDivisors(pf, i + 1, cur * val, outDivs);
            val *= p;
        }
    }
}
