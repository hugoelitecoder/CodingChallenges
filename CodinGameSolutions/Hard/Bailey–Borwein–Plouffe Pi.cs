using System;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        Console.WriteLine(PiDigit(n));
    }

    static double FractionMod1(double f)
    {
        var frac = f - Math.Floor(f);
        if (frac < 0) frac += 1.0;
        return frac;
    }

    static double PiSum(int n, int offset)
    {
        double s = 0.0;
        int k = offset;
        for (int i = 0; i <= n; ++i)
        {
            s = FractionMod1(s + (PowMod(16, n - i, k) / (double)k));
            k += 8;
        }

        double num = 1.0 / 16.0;
        while ((num / k) > 1e-17)
        {
            s += num / k;
            num /= 16.0;
            k += 8;
        }

        return FractionMod1(s);
    }

    static int PowMod(int a, int b, int m)
    {
        long res = 1;
        long aa = a;
        while (b > 0)
        {
            if ((b & 1) != 0) res = (res * aa) % m;
            aa = (aa * aa) % m;
            b >>= 1;
        }
        return (int)res;
    }

    static int PiDigit(int n)
    {
        double s = 0.0;
        s += 4.0 * PiSum(n - 1, 1);
        s += -2.0 * PiSum(n - 1, 4);
        s += -1.0 * PiSum(n - 1, 5);
        s += -1.0 * PiSum(n - 1, 6);
        return (int)(16.0 * FractionMod1(s));
    }
}
