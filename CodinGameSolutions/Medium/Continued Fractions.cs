using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var input = Console.ReadLine().Trim();
        Console.WriteLine(
            input.StartsWith("[")
                ? ConvertCfToFrac(input)
                : ConvertFracToCf(input)
        );
    }

    static string ConvertCfToFrac(string cf)
    {
        var parts = cf.Trim('[', ']').Split(new[] {';', ','}, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim()).Select(int.Parse).ToArray();
        BigInteger num = parts.Last(), den = 1;
        for (int i = parts.Length - 2; i >= 0; i--)
        {
            var a = parts[i];
            var tmp = num;
            num = a * num + den;
            den = tmp;
        }
        if (den < 0) { num = -num; den = -den; }
        return $"{num}/{den}";
    }

    static string ConvertFracToCf(string frac)
    {
        var v = frac.Split('/').Select(BigInteger.Parse).ToArray();
        BigInteger p = v[0], q = v[1];
        var coeffs = new List<BigInteger>();

        while (q != 0)
        {
            BigInteger a = p / q;
            BigInteger r = p % q;
            if (r != 0 && ((p.Sign < 0) ^ (q.Sign < 0)))
            {
                a -= 1;
                r = p - a * q;
            }
            coeffs.Add(a);
            p = q;
            q = r;
        }

        if (coeffs.Count > 1 && coeffs.Last() == 1)
        {
            coeffs[^2]++;
            coeffs.RemoveAt(coeffs.Count - 1);
        }

        return coeffs.Count > 1
            ? $"[{coeffs[0]}; {string.Join(", ", coeffs.Skip(1))}]"
            : $"[{coeffs[0]}]";
    }
}