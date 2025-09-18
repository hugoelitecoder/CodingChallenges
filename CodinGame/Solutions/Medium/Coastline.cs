using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        for (int tc = 0; tc < n; tc++)
        {
            var parts = Console.ReadLine()
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Select(int.Parse)
                              .ToArray();
            int p = parts[0], r = parts[1];
            var pts = new (double x, double y)[p];
            for (int i = 0; i < p; i++)
                pts[i] = (parts[2 + 2*i], parts[3 + 2*i]);

            var intervals = ComputeIntervals(pts, r);
            if (intervals == null)
            {
                Console.WriteLine(-1);
                continue;
            }

            int answer = GreedyCover(intervals);
            Console.WriteLine(answer);
        }
    }

    static List<(double L, double R)> ComputeIntervals((double x, double y)[] pts, double r)
    {
        var list = new List<(double, double)>();
        double r2 = r * r;
        foreach (var (x, y) in pts)
        {
            if (y > r) return null;
            double dx = Math.Sqrt(r2 - y*y);
            list.Add((x - dx, x + dx));
        }
        return list;
    }

    static int GreedyCover(List<(double L, double R)> intervals)
    {
        intervals.Sort((a, b) => a.R.CompareTo(b.R));
        int count = 0;
        double last = double.NegativeInfinity;
        foreach (var (L, R) in intervals)
        {
            if (L > last)
            {
                count++;
                last = R;
            }
        }
        return count;
    }
}
