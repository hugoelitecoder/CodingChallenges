using System;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        int W = int.Parse(Console.ReadLine());
        int s = int.Parse(Console.ReadLine());
        int k = int.Parse(Console.ReadLine());

        long totalWays = Combination(N, s);
        long successWays = Combination(W, k) * Combination(N - W, s - k);
        long failWays = totalWays - successWays;

        long gcd = GCD(successWays, failWays);
        long a = successWays / gcd;
        long b = failWays / gcd;

        Console.WriteLine($"{a}:{b}");
    }

    static long Combination(int n, int r)
    {
        if (r < 0 || r > n) return 0;
        r = Math.Min(r, n - r);
        long result = 1;
        for (int i = 1; i <= r; i++)
        {
            result = result * (n - r + i) / i;
        }
        return result;
    }

    static long GCD(long x, long y)
    {
        while (y != 0)
        {
            long t = x % y;
            x = y;
            y = t;
        }
        return x;
    }
}
