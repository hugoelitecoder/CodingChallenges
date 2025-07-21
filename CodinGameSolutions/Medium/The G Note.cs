using System;
using System.Numerics;
using System.Runtime.CompilerServices;

static class Solution
{
    const int BITS = 53;
    static readonly BigInteger MOD = BigInteger.One << BITS;
    static BigInteger[] bn, bd;
    static int lastP = -1;

    public static ulong Calc(int p)
    {
        if (p != lastP)
        {
            FillB(p, out bn, out bd);
            lastP = p;
        }

        int n = p + 1;
        BigInteger L = 1;
        for (int j = 0; j <= p; j++)
            L = lcm(L, bd[j]);

        BigInteger M = MOD * n * L;
        BigInteger N = BigInteger.Pow(p, p);

        BigInteger s = 0;
        unchecked
        {
            for (int j = 0; j <= p; j++)
            {
                BigInteger c  = binom(n, j);
                BigInteger pw = BigInteger.ModPow(N, n - j, M);
                BigInteger t  = ((c * bn[j]) % M) * pw % M;
                t = (t * (L / bd[j])) % M;
                s = (s + t) % M;
            }
        }

        BigInteger S2 = s / (n * L);
        BigInteger r  = S2 % MOD;
        if (r < 0) r += MOD;
        return (ulong)r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static BigInteger gcd(BigInteger a, BigInteger b)
        => BigInteger.GreatestCommonDivisor(a < 0 ? -a : a, b < 0 ? -b : b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static BigInteger lcm(BigInteger a, BigInteger b)
        => BigInteger.Abs(a / gcd(a, b) * b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static BigInteger binom(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (k > n - k) k = n - k;
        BigInteger c = 1;
        for (int i = 1; i <= k; i++)
            c = c * (n + 1 - i) / i;
        return c;
    }

    static void FillB(int p, out BigInteger[] Bn, out BigInteger[] Bd)
    {
        Bn = new BigInteger[p + 1];
        Bd = new BigInteger[p + 1];
        var aN = new BigInteger[p + 1];
        var aD = new BigInteger[p + 1];

        for (int m = 0; m <= p; m++)
        {
            aN[m] = 1;
            aD[m] = m + 1;
            for (int j = m; j >= 1; j--)
            {
                var d = aN[j - 1] * aD[j] - aN[j] * aD[j - 1];
                var e = aD[j - 1] * aD[j];
                var g = gcd(d, e);
                d /= g;
                e /= g;
                aN[j - 1] = d * j;
                aD[j - 1] = e;
            }
            Bn[m] = aN[0];
            Bd[m] = aD[0];
        }
    }

    static void Main()
    {
        if (int.TryParse(Console.ReadLine(), out int p))
            Console.WriteLine(Calc(p));
        else
            Console.WriteLine("Invalid");
    }
}
