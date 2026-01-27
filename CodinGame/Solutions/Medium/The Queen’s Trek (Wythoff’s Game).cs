using System;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var line = Console.ReadLine();
        var parts = line.Split(' ');
        var n = int.Parse(parts[0]);
        var m = int.Parse(parts[1]);
        Console.Error.WriteLine("[DEBUG] Input n=" + n + " m=" + m);
        var firstWins = WythoffNim.IsFirstPlayerWinning(n, m);
        var result = firstWins ? "FIRST" : "SECOND";
        Console.WriteLine(result);
        sw.Stop();
        Console.Error.WriteLine("[DEBUG] Result=" + result);
        Console.Error.WriteLine("[DEBUG] ElapsedMs=" + sw.ElapsedMilliseconds);
    }
}

static class WythoffNim
{
    private const double Phi = 1.6180339887498948482;
    public static bool IsFirstPlayerWinning(int n, int m)
    {
        var a = n;
        var b = m;
        if (a > b)
        {
            (a, b) = (b, a);
        }
        var k = b - a;
        var t = (int)Math.Floor(k * Phi);
        var losing = t == a;
        return !losing;
    }
}
