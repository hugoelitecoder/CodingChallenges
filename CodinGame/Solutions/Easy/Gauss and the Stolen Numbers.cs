using System;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        var n = long.Parse(Console.ReadLine()!);
        var sRemaining = long.Parse(Console.ReadLine()!);
        var qRemaining = long.Parse(Console.ReadLine()!);
        var sw = Stopwatch.StartNew();
        var solver = new GaussMissingNumbers();
        var result = solver.Solve(n, sRemaining, qRemaining);
        Console.WriteLine($"{result.X} {result.Y}");
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] --- GAUSS SOLVER STATS ---");
        Console.Error.WriteLine($"[DEBUG] N: {n}");
        Console.Error.WriteLine($"[DEBUG] Missing: {result.X}, {result.Y}");
        Console.Error.WriteLine($"[DEBUG] Execution Time: {sw.ElapsedMilliseconds}ms");
    }
}

public record MissingPair(long X, long Y);
public class GaussMissingNumbers
{
    public MissingPair Solve(long n, long sRemaining, long qRemaining)
    {
        long theoreticalS = n * (n + 1) / 2;
        long theoreticalQ = n * (n + 1) * (2 * n + 1) / 6;
        long sumXY = theoreticalS - sRemaining;
        long sumSqXY = theoreticalQ - qRemaining;
        long prodXY = (sumXY * sumXY - sumSqXY) / 2;
        long discriminant = (long)Math.Round(Math.Sqrt(sumXY * sumXY - 4 * prodXY));
        long x = (sumXY - discriminant) / 2;
        long y = (sumXY + discriminant) / 2;
        return new MissingPair(x, y);
    }
}