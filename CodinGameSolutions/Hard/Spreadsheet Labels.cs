using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        string[] vals = Console.ReadLine().Split();
        var output = new List<string>();

        foreach (var val in vals)
        {
            if (BigInteger.TryParse(val, out var num))
                output.Add(NumberToAlpha(num));
            else
                output.Add(AlphaToNumber(val).ToString());
        }

        Console.WriteLine(string.Join(" ", output));
    }

    static string NumberToAlpha(BigInteger num)
    {
        var sb = new StringBuilder();
        while (num > 0)
        {
            num -= 1;
            var rem = (int)(num % 26);
            sb.Insert(0, (char)('A' + rem));
            num /= 26;
        }
        return sb.ToString();
    }

    static BigInteger AlphaToNumber(string s)
    {
        BigInteger n = 0;
        foreach (char c in s)
        {
            n = n * 26 + (c - 'A' + 1);
        }
        return n;
    }
}
