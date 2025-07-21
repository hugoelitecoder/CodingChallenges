using System;
using System.Collections.Generic;

class Solution
{
    const int MaxDigits = 2;
    static readonly char[] Separators = new char[] { ' ', ',', ';', '\t', '\n', '\r' };

    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var digits = new List<int>();
        foreach (var s in Console.ReadLine().Split(Separators, StringSplitOptions.RemoveEmptyEntries))
        {
            digits.Add(int.Parse(s));
        }
        var b = GenerateKolakoski(n, digits);
        for (int i = 0; i < n; i++)
        {
            Console.Write(b[i]);
        }
        Console.WriteLine();
    }

    static List<int> GenerateKolakoski(int n, List<int> digits)
    {
        var b = new List<int>();
        int ai = 0, bi = 0;
        while (b.Count < n)
        {
            int L = bi < b.Count ? b[ai] : digits[ai];
            ai++;
            int val = digits[bi % MaxDigits];
            for (int i = 0; i < L && b.Count < n; i++)
                b.Add(val);
            bi++;
        }
        return b;
    }
}
