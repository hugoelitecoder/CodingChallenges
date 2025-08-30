using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    static void Main(string[] args)
    {
        var n = long.Parse(Console.ReadLine());
        var sw = Stopwatch.StartNew();
        (var count, var sum) = Solve(n);
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Input n: {n}");
        Console.Error.WriteLine($"[DEBUG] Found {count} numbers with beautiful bases.");
        Console.Error.WriteLine($"[DEBUG] Sum of sums of bases: {sum}");
        Console.Error.WriteLine($"[DEBUG] Calculation time: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"{count} {sum}");
    }

    public static (long, long) Solve(long n)
    {
        var numbersWithBase = new HashSet<long>();
        var sumOfBaseSums = 0L;
        var limit = (long)Math.Sqrt(n);
        for (var b = 2L; b <= limit; b++)
        {
            var powerOfB = b * b;
            while (true)
            {
                numbersWithBase.Add(powerOfB);
                sumOfBaseSums += b;
                if (powerOfB > n / b)
                {
                    break;
                }
                powerOfB *= b;
            }
        }
        return (numbersWithBase.Count, sumOfBaseSums);
    }
}