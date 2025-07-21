using System;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        var N = int.Parse(Console.ReadLine().Trim());
        for (var i = 0; i < N; i++)
        {
            var s = Console.ReadLine().Trim();
            var initial = SumOfSquares(s);
            var happy = IsHappy(initial);
            Console.WriteLine($"{s} {(happy?":)":":(")}");
        }
    }

    private static int SumOfSquares(string s)
    {
        var sum = 0;
        foreach (var ch in s)
        {
            var d = ch - '0';
            sum += d * d;
        }
        return sum;
    }

    private static int SumOfSquares(int n)
    {
        var sum = 0;
        while (n > 0)
        {
            var d = n % 10;
            sum += d * d;
            n /= 10;
        }
        return sum;
    }

    private static bool IsHappy(int n)
    {
        var seen = new HashSet<int>();
        var cur = n;
        while (true)
        {
            if (cur == 1) return true;
            if (!seen.Add(cur)) return false;
            cur = SumOfSquares(cur);
        }
    }
}
