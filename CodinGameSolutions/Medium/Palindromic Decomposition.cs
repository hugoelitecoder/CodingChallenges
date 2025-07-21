using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var s = Console.ReadLine();
        var isPal = BuildPalindromeTable(s);
        var (prefixPal, suffixPal) = BuildEdgePalindromes(s, isPal);
        var result = CountDecompositions(s, isPal, prefixPal, suffixPal);
        Console.WriteLine(result);
    }

    private static bool[,] BuildPalindromeTable(string s)
    {
        var n = s.Length;
        var isPal = new bool[n, n];
        for (var len = 1; len <= n; len++)
            for (var i = 0; i + len <= n; i++)
            {
                var j = i + len - 1;
                if (s[i] == s[j] && (len <= 2 || isPal[i + 1, j - 1]))
                    isPal[i, j] = true;
            }
        return isPal;
    }

    private static (bool[] prefixPal, bool[] suffixPal) BuildEdgePalindromes(string s, bool[,] isPal)
    {
        var n = s.Length;
        var prefixPal = new bool[n + 1];
        var suffixPal = new bool[n + 1];
        prefixPal[0] = true;
        suffixPal[n] = true;
        for (var i = 1; i <= n; i++)
            prefixPal[i] = isPal[0, i - 1];
        for (var i = 0; i < n; i++)
            suffixPal[i] = isPal[i, n - 1];
        return (prefixPal, suffixPal);
    }

    private static long CountDecompositions(string s, bool[,] isPal, bool[] prefixPal, bool[] suffixPal)
    {
        var n = s.Length;
        long result = 0;
        for (var j = 0; j <= n; j++)
        {
            if (!suffixPal[j]) continue;
            var count = 0;
            for (var i = 0; i <= j; i++)
                if (prefixPal[i] && (j == i || isPal[i, j - 1]))
                    count++;
            result += count;
        }
        return result;
    }
}
