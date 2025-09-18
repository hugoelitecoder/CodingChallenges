using System;

class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        Console.WriteLine(CountStaircases(N));
    }

    static long CountStaircases(int n)
    {
        var dp = new long[n + 2, n + 2];
        for (int prev = 0; prev <= n; prev++)
            dp[0, prev] = 1;

        for (int rem = 1; rem <= n; rem++)
        {
            for (int prev = n - 1; prev >= 0; prev--)
            {
                long res = 0;
                for (int nxt = prev + 1; nxt <= rem; nxt++)
                {
                    res += dp[rem - nxt, nxt];
                }
                dp[rem, prev] = res;
            }
        }
        return dp[n, 0] - 1;
    }
}
