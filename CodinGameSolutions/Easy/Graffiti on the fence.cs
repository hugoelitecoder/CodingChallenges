using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int L = int.Parse(Console.ReadLine());
        int N = int.Parse(Console.ReadLine());
        List<(int st, int ed)> reports = new List<(int st, int ed)>();

        for (int i = 0; i < N; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int st = int.Parse(inputs[0]);
            int ed = int.Parse(inputs[1]);
            reports.Add((st, ed));
        }

        // Sort the reports by starting point
        reports = reports.OrderBy(r => r.st).ToList();

        // Merge intervals
        List<(int st, int ed)> merged = new List<(int st, int ed)>();
        int currentStart = reports[0].st;
        int currentEnd = reports[0].ed;

        foreach (var report in reports.Skip(1))
        {
            if (report.st <= currentEnd)
            {
                // Overlapping intervals, merge them
                currentEnd = Math.Max(currentEnd, report.ed);
            }
            else
            {
                // Non-overlapping interval, add the previous one and start a new one
                merged.Add((currentStart, currentEnd));
                currentStart = report.st;
                currentEnd = report.ed;
            }
        }
        // Add the last interval
        merged.Add((currentStart, currentEnd));

        // Find unpainted sections
        List<(int st, int ed)> unpainted = new List<(int st, int ed)>();
        int lastEnd = 0;

        foreach (var interval in merged)
        {
            if (lastEnd < interval.st)
            {
                unpainted.Add((lastEnd, interval.st));
            }
            lastEnd = interval.ed;
        }

        if (lastEnd < L)
        {
            unpainted.Add((lastEnd, L));
        }

        // Output the result
        if (unpainted.Count == 0)
        {
            Console.WriteLine("All painted");
        }
        else
        {
            foreach (var section in unpainted)
            {
                Console.WriteLine($"{section.st} {section.ed}");
            }
        }
    }
}