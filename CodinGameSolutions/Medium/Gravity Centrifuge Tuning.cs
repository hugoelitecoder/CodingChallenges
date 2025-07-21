using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var s = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(s) || s == "0")
        {
            Console.WriteLine("0");
            return;
        }

        var N = BigInteger.Parse(s);

        // Fibonacci generator
        IEnumerable<BigInteger> fibs()
        {
            BigInteger a = 1, b = 2;
            while (a <= N)
            {
                yield return a;
                var t = a + b;
                a = b;
                b = t;
            }
        }

        // Zeckendorf mask
        var rem = N;
        var mask = fibs()
            .Select((f, i) => (f, i))
            .Reverse()
            .Where(p => p.f <= rem && (rem -= p.f) >= 0)
            .Select(p => BigInteger.One << p.i)
            .Aggregate(BigInteger.Zero, (m, bit) => m | bit);

        // Octal conversion
        var oct = string.Empty;
        while (mask > 0)
        {
            var d = mask % 8;
            oct = (char)('0' + (int)d) + oct;
            mask /= 8;
        }

        Console.WriteLine(string.IsNullOrEmpty(oct) ? "0" : oct);
    }
}
