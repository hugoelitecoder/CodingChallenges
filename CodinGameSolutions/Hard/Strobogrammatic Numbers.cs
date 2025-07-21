using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var low = Console.ReadLine();
        var high = Console.ReadLine();
        var finder = new StrobogrammaticFinder();
        var answer = finder.CountInRange(low, high);
        Console.WriteLine(answer);
    }
}

public class StrobogrammaticFinder
{
    private static readonly char[][] Pairs =
    {
        new[] { '0', '0' },
        new[] { '1', '1' },
        new[] { '6', '9' },
        new[] { '8', '8' },
        new[] { '9', '6' }
    };
    private static readonly string[] Singles = { "0", "1", "8" };

    public int CountInRange(string low, string high)
    {
        var count = 0;
        var lowLen = low.Length;
        var highLen = high.Length;
        for (var len = lowLen; len <= highLen; len++)
        {
            if (len == 0)
            {
                continue;
            }
            var numbers = Generate(len);
            foreach (var num in numbers)
            {
                if (IsInRange(num, low, high))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private List<string> Generate(int n)
    {
        return GenerateRecursive(n, n);
    }

    private List<string> GenerateRecursive(int n, int totalN)
    {
        if (n == 0)
        {
            return new List<string> { "" };
        }
        if (n == 1)
        {
            return new List<string>(Singles);
        }
        var middles = GenerateRecursive(n - 2, totalN);
        var results = new List<string>();
        foreach (var mid in middles)
        {
            foreach (var p in Pairs)
            {
                if (n == totalN && p[0] == '0')
                {
                    continue;
                }
                results.Add(p[0] + mid + p[1]);
            }
        }
        return results;
    }

    private bool IsInRange(string s, string low, string high)
    {
        var len = s.Length;
        if (len < low.Length || (len == low.Length && s.CompareTo(low) < 0))
        {
            return false;
        }
        if (len > high.Length || (len == high.Length && s.CompareTo(high) > 0))
        {
            return false;
        }
        return true;
    }
}

