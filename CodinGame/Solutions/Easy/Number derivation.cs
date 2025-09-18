using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var original = n;
        var factors = new Dictionary<int,int>();
        for (var p = 2; p * p <= n; p++)
        {
            while (n % p == 0)
            {
                if (!factors.ContainsKey(p)) factors[p] = 0;
                factors[p]++;
                n /= p;
            }
        }
        if (n > 1) factors[n] = 1;
        var deriv = 0L;
        foreach (var kv in factors)
            deriv += (long)kv.Value * original / kv.Key;
        Console.WriteLine(deriv);
    }
}
