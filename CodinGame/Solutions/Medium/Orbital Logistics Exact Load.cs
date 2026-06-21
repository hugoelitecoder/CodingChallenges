using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    public static void Main()
    {
        var total = Stopwatch.StartNew();
        var read = Stopwatch.StartNew();
        var input = ReadPair();
        var capacity = input.Item1;
        var itemCount = input.Item2;
        var items = new Item[itemCount];
        for (var i = 0; i < itemCount; i++)
        {
            var item = ReadPair();
            items[i] = new(item.Item1, item.Item2);
        }
        read.Stop();
        var solve = Stopwatch.StartNew();
        var answer = CargoSolver.Solve(capacity, items, out var stats);
        solve.Stop();
        var output = Stopwatch.StartNew();
        Console.WriteLine(answer);
        output.Stop();
        total.Stop();
        PrintDebug(capacity, itemCount, answer, stats, read.Elapsed, solve.Elapsed, output.Elapsed, total.Elapsed);
    }
    private static (int, int) ReadPair()
    {
        var text = Console.ReadLine()!.AsSpan().Trim();
        var split = text.IndexOf(' ');
        return (int.Parse(text[..split]), int.Parse(text[(split + 1)..]));
    }
    private static void PrintDebug(int capacity, int itemCount, long answer, SearchStatistics stats, TimeSpan read, TimeSpan solve, TimeSpan output, TimeSpan total)
    {
        Console.Error.WriteLine($"[DEBUG] capacity={capacity} items={itemCount} answer={answer}\n[DEBUG] left={stats.LeftCombinations} right={stats.RightCombinations} uniqueRightWeights={stats.UniqueRightWeights} matches={stats.MatchCount}\n[DEBUG] read={read.TotalMilliseconds:F3}ms solve={solve.TotalMilliseconds:F3}ms output={output.TotalMilliseconds:F3}ms total={total.TotalMilliseconds:F3}ms");
    }
}

public readonly record struct Item(int Weight, int Value);
public readonly record struct SearchStatistics(int LeftCombinations, int RightCombinations, int UniqueRightWeights, int MatchCount);
public static class CargoSolver
{
    public static long Solve(int capacity, Item[] items, out SearchStatistics stats)
    {
        var split = items.Length / 2;
        var right = new Dictionary<int, long>();
        var rightCount = 0;
        SearchRight(items, split, capacity, 0, 0, right, ref rightCount);
        var leftCount = 0;
        var matches = 0;
        var answer = -1L;
        SearchLeft(items, 0, split, capacity, 0, 0, right, ref leftCount, ref matches, ref answer);
        stats = new(leftCount, rightCount, right.Count, matches);
        return answer;
    }
    private static void SearchRight(Item[] items, int index, int capacity, int weight, long value, Dictionary<int, long> best, ref int count)
    {
        if (weight > capacity) return;
        if (index == items.Length)
        {
            count++;
            if (!best.TryGetValue(weight, out var current) || value > current) best[weight] = value;
            return;
        }
        SearchRight(items, index + 1, capacity, weight, value, best, ref count);
        SearchRight(items, index + 1, capacity, weight + items[index].Weight, value + items[index].Value, best, ref count);
    }
    private static void SearchLeft(Item[] items, int index, int end, int capacity, int weight, long value, Dictionary<int, long> right, ref int count, ref int matches, ref long answer)
    {
        if (weight > capacity) return;
        if (index == end)
        {
            count++;
            if (right.TryGetValue(capacity - weight, out var rightValue))
            {
                matches++;
                var total = value + rightValue;
                if (total > answer) answer = total;
            }
            return;
        }
        SearchLeft(items, index + 1, end, capacity, weight, value, right, ref count, ref matches, ref answer);
        SearchLeft(items, index + 1, end, capacity, weight + items[index].Weight, value + items[index].Value, right, ref count, ref matches, ref answer);
    }
}