using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var jobs = new List<(int Start, int End)>(N);
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            int s = int.Parse(parts[0]);
            int d = int.Parse(parts[1]);
            jobs.Add((s, s + d));
        }

        jobs.Sort((a, b) => a.End.CompareTo(b.End));
        int count = 0, lastEnd = 0;
        foreach (var (start, end) in jobs)
        {
            if (start >= lastEnd)
            {
                count++;
                lastEnd = end;
            }
        }

        Console.WriteLine(count);
    }
}
