using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var conditions = Enumerable.Range(0, N)
            .Select(_ =>
            {
                var parts = Console.ReadLine().Split();
                return new Congruence(long.Parse(parts[0]), long.Parse(parts[1]));
            })
            .ToList();

        var state = conditions[0];
        foreach (var cond in conditions.Skip(1))
        {
            var next = state.Combine(cond);
            if (!next.HasValue)
            {
                Console.WriteLine("-1");
                return;
            }
            state = next.Value;
        }

        long result = state.R;
        long minM = conditions.Max(c => c.M);
        if (result < minM)
        {
            result += (minM - result).CeilDiv(state.M) * state.M;
        }

        Console.WriteLine(result);
    }
}

struct Congruence
{
    public long M { get; }
    public long R { get; }

    public Congruence(long modulus, long remainder)
    {
        M = modulus;
        R = remainder.Mod(modulus);
    }

    public Congruence? Combine(Congruence other)
    {
        long g = M.Gcd(other.M);
        long diff = other.R - R;
        if (diff % g != 0) return null;

        long Mg = M / g;
        long mg = other.M / g;
        long inv = Mg.ModInv(mg);
        long t = ((diff / g).Mod(mg) * inv).Mod(mg);

        long newR = R + M * t;
        long newM = Mg * other.M;
        return new Congruence(newM, newR);
    }
}

static class Extensions
{
    public static long Gcd(this long a, long b)
    {
        while (b != 0)
        {
            long t = a % b;
            a = b;
            b = t;
        }
        return a;
    }

    public static long ExtendedGcd(this long a, long b, out long x, out long y)
    {
        if (b == 0)
        {
            x = 1; y = 0;
            return a;
        }
        long g = b.ExtendedGcd(a % b, out long x1, out long y1);
        x = y1;
        y = x1 - (a / b) * y1;
        return g;
    }

    public static long ModInv(this long a, long m)
    {
        long g = a.ExtendedGcd(m, out long x, out _);
        if (g != 1) throw new InvalidOperationException();
        return x.Mod(m);
    }

    public static long Mod(this long a, long m) =>
        (a % m + m) % m;

    public static long CeilDiv(this long a, long b) =>
        (a + b - 1) / b;
}
