using System;
using System.Collections.Generic;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        BigInteger G = BigInteger.Parse(inputs[0]);
        BigInteger H = BigInteger.Parse(inputs[1]);
        BigInteger Q = BigInteger.Parse(inputs[2]);

        Console.WriteLine(BabyStepGiantStep(G, H, Q));
    }

    static BigInteger BabyStepGiantStep(BigInteger G, BigInteger H, BigInteger Q)
    {
        BigInteger m = SqrtCeil(Q);
        var table = new Dictionary<BigInteger, BigInteger>();

        BigInteger e = 1;
        for (BigInteger j = 0; j < m; j++)
        {
            if (!table.ContainsKey(e))
                table[e] = j;
            e = (e * G) % Q;
        }

        BigInteger factor = ModPow(ModInv(G, Q), m, Q);
        BigInteger gamma = H;

        for (BigInteger i = 0; i <= m; i++)
        {
            if (table.TryGetValue(gamma, out BigInteger j))
                return i * m + j;
            gamma = (gamma * factor) % Q;
        }
        return -1;
    }

    static BigInteger ModPow(BigInteger a, BigInteger exp, BigInteger mod)
    {
        return BigInteger.ModPow(a, exp, mod);
    }

    static BigInteger ModInv(BigInteger a, BigInteger mod)
    {
        BigInteger m0 = mod, t, q;
        BigInteger x0 = 0, x1 = 1;
        if (mod == 1) return 0;
        while (a > 1)
        {
            q = a / mod;
            t = mod;

            mod = a % mod; a = t;
            t = x0;

            x0 = x1 - q * x0;
            x1 = t;
        }
        if (x1 < 0)
            x1 += m0;
        return x1;
    }

    static BigInteger SqrtCeil(BigInteger n)
    {
        if (n < 0) throw new ArgumentException();
        if (n == 0) return 0;
        BigInteger x = n / 2 + 1;
        BigInteger y = (x + n / x) / 2;
        while (y < x)
        {
            x = y;
            y = (x + n / x) / 2;
        }
        if (x * x < n) return x + 1;
        return x;
    }
}
