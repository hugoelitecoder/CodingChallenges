using System;
using System.Numerics;

class Solution
{
    static void Main()
    {
        var s = Console.ReadLine().Trim();
        var parts = s.Split('.');
        var intPart = parts[0];
        var fracPart = parts.Length > 1 ? parts[1] : string.Empty;
        int nonRep, period;
        var repeatUnit = FindCycle(fracPart, out nonRep, out period);

        var nonRepeating = fracPart.Substring(0, nonRep);
        var repeating = repeatUnit;

        var (numer, denom) = ToFraction(intPart, nonRepeating, repeating);
        var g = Gcd(BigInteger.Abs(numer), denom);
        numer /= g;
        denom /= g;

        Console.WriteLine(denom == 1 ? numer.ToString() : $"{numer} / {denom}");
    }

    static string FindCycle(string f, out int k, out int p)
    {
        int L = f.Length;
        for (k = 0; k < L; k++)
        {
            for (p = 1; p <= L - k; p++)
            {
                if (L - k < 2 * p) continue;
                var u = f.Substring(k, p);
                bool ok = true;
                for (int i = k; i < L; i++)
                    if (f[i] != u[(i - k) % p]) { ok = false; break; }
                if (ok) return u;
            }
        }
        k = 0; p = 0;
        return string.Empty;
    }

    static (BigInteger numer, BigInteger denom) ToFraction(string A, string B, string R)
    {
        int h = B.Length;
        int r = R.Length;
        var a = BigInteger.Parse(A);
        var b = h > 0 ? BigInteger.Parse(B) : BigInteger.Zero;
        var rep = BigInteger.Parse(R);

        var pow10h = BigInteger.Pow(10, h);
        var nines = BigInteger.Pow(10, r) - 1;

        var denom = pow10h * nines;
        var numer = b * nines + rep + a * denom;

        return (numer, denom);
    }

    static BigInteger Gcd(BigInteger a, BigInteger b)
    {
        while (b != 0)
        {
            var t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}
