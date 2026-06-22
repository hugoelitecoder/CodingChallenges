using System;
using System.Diagnostics;
using System.Globalization;

class Solution
{
    public static void Main(string[] args)
    {
        var totalWatch = Stopwatch.StartNew();
        var readWatch = Stopwatch.StartNew();

        var count = int.Parse(Console.ReadLine() ?? "0");
        var parts = (Console.ReadLine() ?? "").Split(
            new[] { ' ' },
            StringSplitOptions.RemoveEmptyEntries
        );

        var heights = new int[count];
        for (var i = 0; i < count; i++)
            heights[i] = int.Parse(parts[i]);

        readWatch.Stop();
        var solveWatch = Stopwatch.StartNew();
        LeapStats stats;
        var answer = LeapSolver.FindMax(heights, out stats);
        solveWatch.Stop();
        var outputWatch = Stopwatch.StartNew();
        Console.WriteLine(answer);
        outputWatch.Stop();
        totalWatch.Stop();
        PrintDebug(
            count,
            answer,
            stats,
            readWatch.Elapsed,
            solveWatch.Elapsed,
            outputWatch.Elapsed,
            totalWatch.Elapsed
        );
    }

    private static void PrintDebug(
        int count,
        long answer,
        LeapStats stats,
        TimeSpan readTime,
        TimeSpan solveTime,
        TimeSpan outputTime,
        TimeSpan totalTime)
    {
        Console.Error.WriteLine("[DEBUG] ========================================");
        Console.Error.WriteLine("[DEBUG] Leap of Sheep Report");
        Console.Error.WriteLine("[DEBUG] ========================================");

        Console.Error.WriteLine("[DEBUG] Input:");
        Console.Error.WriteLine("[DEBUG]   Read " + count + " dirt pile heights.");
        Console.Error.WriteLine("[DEBUG]   Reading input took " + FormatTime(readTime) + ".");

        Console.Error.WriteLine("[DEBUG] Search:");
        Console.Error.WriteLine("[DEBUG]   Checked " + stats.CentersChecked + " possible middle piles.");
        Console.Error.WriteLine("[DEBUG]   Found " + stats.ValidCenters + " valid middle piles.");
        Console.Error.WriteLine("[DEBUG]   Best middle pile index: " + stats.BestIndex + ".");
        Console.Error.WriteLine("[DEBUG]   Best middle pile height: " + stats.BestHeight + ".");
        Console.Error.WriteLine("[DEBUG]   Calculating the hardest leap took " + FormatTime(solveTime) + ".");

        Console.Error.WriteLine("[DEBUG] Result:");
        Console.Error.WriteLine("[DEBUG]   Maximum leap difficulty: " + answer + ".");
        Console.Error.WriteLine("[DEBUG]   Writing output took " + FormatTime(outputTime) + ".");
        Console.Error.WriteLine("[DEBUG] Total execution time: " + FormatTime(totalTime) + ".");
        Console.Error.WriteLine("[DEBUG] ========================================");
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture) + " ms";
    }
}

static class LeapSolver
{
    public static long FindMax(int[] heights, out LeapStats stats)
    {
        var count = heights.Length;
        var rightMin = new int[count];

        rightMin[count - 1] = heights[count - 1];

        for (var i = count - 2; i >= 0; i--)
            rightMin[i] = Math.Min(heights[i], rightMin[i + 1]);

        var leftMin = heights[0];
        var best = -1L;
        var validCenters = 0;
        var bestIndex = -1;
        var bestHeight = -1;

        for (var middle = 1; middle < count - 1; middle++)
        {
            var middleHeight = heights[middle];
            var rightHeight = rightMin[middle + 1];

            if (middleHeight > leftMin && middleHeight > rightHeight)
            {
                validCenters++;

                var difficulty =
                    2L * middleHeight -
                    leftMin -
                    rightHeight;

                if (difficulty > best)
                {
                    best = difficulty;
                    bestIndex = middle;
                    bestHeight = middleHeight;
                }
            }

            if (middleHeight < leftMin)
                leftMin = middleHeight;
        }

        stats = new LeapStats(
            count - 2,
            validCenters,
            bestIndex,
            bestHeight
        );

        return best;
    }
}

struct LeapStats
{
    public int CentersChecked;
    public int ValidCenters;
    public int BestIndex;
    public int BestHeight;

    public LeapStats(
        int centersChecked,
        int validCenters,
        int bestIndex,
        int bestHeight)
    {
        CentersChecked = centersChecked;
        ValidCenters = validCenters;
        BestIndex = bestIndex;
        BestHeight = bestHeight;
    }
}