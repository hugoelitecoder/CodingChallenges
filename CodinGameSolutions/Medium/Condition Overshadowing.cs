using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var lines = Enumerable.Range(0, n)
                              .Select(_ => Console.ReadLine().Split())
                              .ToArray();

        var analyzer = new ConditionChainAnalyzer(lines);
        var overshadowed = analyzer.GetOvershadowed();
        Console.WriteLine(overshadowed.Any()
            ? string.Join(" ", overshadowed)
            : "ok");
    }
}

class ConditionChainAnalyzer
{
    private readonly string[][] _conds;
    public ConditionChainAnalyzer(string[][] conds) => _conds = conds;

    public List<int> GetOvershadowed()
    {
        var covered = new List<(long Min, long Max)>();
        var result = new List<int>();

        for (int i = 0; i < _conds.Length; i++)
        {
            var ranges = GetRanges(_conds[i][1], long.Parse(_conds[i][2]));
            if (ranges.All(r => IsCovered(r, covered)))
                result.Add(i);
            else
                foreach (var r in ranges)
                    Merge(r, covered);
        }

        return result;
    }

    private static (long Min, long Max)[] GetRanges(string op, long v) => op switch {
        ">"  => new[] { (v + 1, long.MaxValue) },
        "<"  => new[] { (long.MinValue, v - 1) },
        "==" => new[] { (v, v) },
        "!=" => new[] { (long.MinValue, v - 1), (v + 1, long.MaxValue) },
        _    => throw new ArgumentException()
    };

    private static bool IsCovered((long Min, long Max) r, List<(long Min, long Max)> cv)
        => cv.Any(c => r.Min >= c.Min && r.Max <= c.Max);

    private static void Merge((long Min, long Max) r, List<(long Min, long Max)> cv)
    {
        cv.Add(r);
        cv.Sort((a, b) => a.Min.CompareTo(b.Min));

        var merged = new List<(long Min, long Max)>();
        var (currMin, currMax) = cv[0];

        foreach (var (nextMin, nextMax) in cv.Skip(1))
        {
            bool overlapOrAdj = currMax == long.MaxValue
                                || nextMin <= currMax + 1;
            if (overlapOrAdj)
            {
                currMax = Math.Max(currMax, nextMax);
            }
            else
            {
                merged.Add((currMin, currMax));
                currMin = nextMin;
                currMax = nextMax;
            }
        }

        merged.Add((currMin, currMax));
        cv.Clear();
        cv.AddRange(merged);
    }
}
