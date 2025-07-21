using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        long n = long.Parse(Console.ReadLine());
        Console.WriteLine(NthSolid(n));
    }

    static string NthSolid(long n)
    {
        var digits = new List<char>();
        while (n > 0)
        {
            n--;
            int d = (int)(n % 9) + 1;
            digits.Add((char)('0' + d));
            n /= 9;
        }
        digits.Reverse();
        return new string(digits.ToArray());
    }
}
