using System;
using System.Text;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var n = long.Parse(Console.ReadLine());
        var output = GetFractionDecimal(n);
        Console.WriteLine(output);
    }

    public static string GetFractionDecimal(long n)
    {
        if (n == 0) return "NaN";
        if (n == 1) return "1.0";
        var result = new StringBuilder("0.");
        var remainder = 1L;
        var remPos = new Dictionary<long, int>();
        var dec = new StringBuilder();
        while (remainder != 0 && !remPos.ContainsKey(remainder))
        {
            remPos.Add(remainder, dec.Length);
            remainder *= 10;
            var digit = remainder / n;
            dec.Append(digit);
            remainder %= n;
        }
        if (remainder == 0)
        {
            result.Append(dec);
        }
        else
        {
            var idx = remPos[remainder];
            result.Append(dec, 0, idx);
            result.Append('(');
            result.Append(dec, idx, dec.Length - idx);
            result.Append(')');
        }
        return result.ToString();
    }
}
