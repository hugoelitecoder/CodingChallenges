using System;

class Solution
{
    private static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            var t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    private static long ExtendedGcd(long a, long b, out long x, out long y)
    {
        if (b == 0)
        {
            x = 1; y = 0;
            return a;
        }
        var g = ExtendedGcd(b, a % b, out var x1, out var y1);
        x = y1;
        y = x1 - (a / b) * y1;
        return g;
    }

    private static long ModInverse(long a, long m)
    {
        a %= m;
        if (a < 0) a += m;
        ExtendedGcd(a, m, out var x, out _);
        return (x % m + m) % m;
    }

    public static void Main()
    {
        var parts = Console.ReadLine().Split();
        var x = long.Parse(parts[0]);
        var y = long.Parse(parts[1]);
        var m = long.Parse(parts[2]);
        var n = long.Parse(parts[3]);
        var l = long.Parse(parts[4]);

        if (m == n && x != y)
        {
            Console.WriteLine("Impossible");
            return;
        }

        var c = m > n ? m - n : n - m;
        var s = m > n 
            ? (y - x + l) % l 
            : (x - y + l) % l;

        var g = Gcd(c, l);
        c /= g;
        s /= g;
        l /= g;

        var mod = l;
        var inv = mod == 1 ? 0 : ModInverse(c, mod);
        var t = (s * inv) % mod;

        Console.WriteLine(t);
    }
}
