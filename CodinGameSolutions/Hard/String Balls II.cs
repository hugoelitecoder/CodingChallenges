using System;

class Solution
{
    private const long M = 1_000_000_007;

    public static void Main(string[] args)
    {
        var radius = int.Parse(Console.ReadLine());
        var center = Console.ReadLine();
        var answer = Solve(center, radius);
        Console.WriteLine(answer);
    }

    public static long Solve(string center, int radius)
    {
        var len = center.Length;
        var maxRadius = ComputeMaxRadius(center);
        if (radius >= maxRadius) return Power(26, len);
        var useComplement = radius > maxRadius / 2;
        var effRadius = useComplement ? (int)(maxRadius - radius - 1) : radius;
        if (effRadius < 0) return Power(26, len);
        var dp = new long[effRadius + 1];
        dp[0] = 1;
        for (var i = 0; i < len; i++)
            dp = UpdateDP(dp, center[i], effRadius, useComplement);
        var total = Sum(dp);
        if (useComplement)
        {
            var all = Power(26, len);
            return (all - total + M) % M;
        }
        return total;
    }

    private static long ComputeMaxRadius(string s)
    {
        var sum = 0L;
        for (var i = 0; i < s.Length; i++)
        {
            var idx = s[i] - 'a';
            sum += Math.Max(idx, 25 - idx);
        }
        return sum;
    }

    private static long[] UpdateDP(long[] dp, char c, int effRadius, bool useComplement)
    {
        var next = new long[effRadius + 1];
        var idx = c - 'a';
        if (!useComplement)
        {
            var da = idx;
            var dz = 25 - idx;
            for (var k = 0; k <= effRadius; k++)
            {
                var val = Get(next, k - 1);
                val = (val + Get(dp, k)) % M;
                val = (val + Get(dp, k - 1)) % M;
                val = (val - Get(dp, k - da - 1) + M) % M;
                val = (val - Get(dp, k - dz - 1) + M) % M;
                next[k] = val;
            }
        }
        else
        {
            var d1 = Math.Min(idx, 25 - idx);
            var d2 = Math.Max(idx, 25 - idx);
            for (var k = 0; k <= effRadius; k++)
            {
                var val = Get(next, k - 1);
                val = (val + Get(dp, k)) % M;
                val = (val + Get(dp, k - (d2 - d1))) % M;
                val = (val - Get(dp, k - d2) + M) % M;
                val = (val - Get(dp, k - (d2 + 1)) + M) % M;
                next[k] = val;
            }
        }
        return next;
    }

    private static long Power(long bas, int exp)
    {
        var res = 1L;
        bas %= M;
        while (exp > 0)
        {
            if ((exp & 1) == 1) res = (res * bas) % M;
            bas = (bas * bas) % M;
            exp >>= 1;
        }
        return res;
    }

    private static long Get(long[] arr, int i)
    {
        if (i < 0 || i >= arr.Length) return 0;
        return arr[i];
    }

    private static long Sum(long[] arr)
    {
        var sum = 0L;
        for (var i = 0; i < arr.Length; i++)
            sum = (sum + arr[i]) % M;
        return sum;
    }
}
