using System;
using System.Diagnostics;
class Solution
{
    static void Main(string[] args)
    {
        var nInput = Console.ReadLine();
        if (string.IsNullOrEmpty(nInput)) return;
        int n = int.Parse(nInput);
        var calculator = new ChocolateCutter();
        for (int i = 0; i < n; i++)
        {
            var sw = Stopwatch.StartNew();
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;
            var parts = line.Split(' ');
            long x = long.Parse(parts[0]);
            long y = long.Parse(parts[1]);
            long result = calculator.CalculateCutSquares(x, y);
            Console.WriteLine(result);
            sw.Stop();
            Console.Error.WriteLine($"[DEBUG] Dim: {x}x{y} | Result: {result} | Time: {sw.ElapsedTicks / 10.0}μs");
        }
    }
}
public class ChocolateCutter
{
    public long CalculateCutSquares(long x, long y)
    {
        return x + y - GetGCD(x, y);
    }
    private long GetGCD(long a, long b)
    {
        while (b != 0)
        {
            a %= b;
            var temp = a;
            a = b;
            b = temp;
        }
        return a;
    }
}