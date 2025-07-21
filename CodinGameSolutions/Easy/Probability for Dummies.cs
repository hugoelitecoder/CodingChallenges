using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        int m = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());
        const int S = 38;

        var C = new BigInteger[S + 1, S + 1];
        for (int i = 0; i <= S; i++)
        {
            C[i, 0] = C[i, i] = 1;
            for (int j = 1; j < i; j++)
                C[i, j] = C[i - 1, j - 1] + C[i - 1, j];
        }

        var pow = new BigInteger[S + 1];
        for (int k = 0; k <= S; k++)
            pow[k] = BigInteger.Pow(new BigInteger(k), n);

        var denom = pow[S];
        var numerator = BigInteger.Zero;
        int kmax = Math.Min(n, S);
        for (int k = m; k <= kmax; k++)
        {
            BigInteger onto = BigInteger.Zero;
            for (int j = 0; j <= k; j++)
            {
                var term = C[k, j] * pow[k - j];
                if ((j & 1) == 1) onto -= term;
                else onto += term;
            }
            numerator += C[S, k] * onto;
        }

        var p = numerator * 100;
        var quotient = p / denom;
        var rem = p % denom;
        if (rem * 2 >= denom) quotient++;

        Console.WriteLine(quotient + "%");
    }
}
