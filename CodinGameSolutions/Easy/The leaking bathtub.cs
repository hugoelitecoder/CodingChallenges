using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        var S = long.Parse(Console.ReadLine());       // cm²
        var h = long.Parse(Console.ReadLine());       // cm
        var flow = double.Parse(Console.ReadLine());  // L/min
        var n = int.Parse(Console.ReadLine());

        var leaks = new List<(long height, double flow)>();
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split();
            var lh = long.Parse(parts[0]);
            var lf = double.Parse(parts[1]);
            leaks.Add((lh, lf));
        }

        leaks = leaks.OrderBy(l => l.height).ToList();
        leaks.Add((h, 0.0));

        double totalSeconds = 0.0;
        long currentHeight = 0;
        double leakingFlow = 0.0;

        foreach (var leak in leaks)
        {
            var nextHeight = leak.height;
            var deltaH = nextHeight - currentHeight;
            var netFlow = flow - leakingFlow;
            if (netFlow <= 0.0)
            {
                Console.WriteLine($"Impossible, {currentHeight} cm.");
                return;
            }
            var volumeCm3 = (double)S * deltaH;
            totalSeconds += (volumeCm3 / 1000.0 / netFlow) * 60.0;

            currentHeight = nextHeight;
            leakingFlow += leak.flow;
        }

        var secs = (long)Math.Floor(totalSeconds);
        var hh = secs / 3600;
        var mm = (secs % 3600) / 60;
        var ss = secs % 60;
        Console.WriteLine($"{hh:D2}:{mm:D2}:{ss:D2}");
    }
}
