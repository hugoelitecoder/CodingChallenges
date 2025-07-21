using System;
using System.Linq;

class Solution
{
    const int MOD = 1_000_000_007;

    static void Main()
    {
        int T = int.Parse(Console.ReadLine());
        while (T-- > 0)
        {
            var inpt = Console.ReadLine().Split().Select(int.Parse).ToArray();
            int K = inpt[0], N = inpt[1];
            Console.WriteLine(K switch
            {
                1 => Solve1(N),
                2 => Solve2(N),
                3 => Solve3(N),
                _ => 0
            });
        }
    }

    static long Solve1(int N) => N % 3 == 0 ? 1 : 0;

    static long Solve2(int N)
    {
        var dp = new long[] { 0, 0, 1, 1 }.Concat(new long[Math.Max(0, N - 3)]).ToArray();
        for (int i = 4; i <= N; i++)
            dp[i] = (dp[i - 2] + dp[i - 3]) % MOD;
        return dp[N];
    }

    static long Solve3(int N)
    {
        var dp = new[]
        {
            new long[] { 1, 1, 1 }.Concat(new long[Math.Max(0, N - 2)]).ToArray(),
            new long[] { 0, 0, 0 }.Concat(new long[Math.Max(0, N - 2)]).ToArray(),
            new long[] { 0, 0, 1 }.Concat(new long[Math.Max(0, N - 2)]).ToArray()
        };
        for (int i = 3; i <= N; i++)
        {
            dp[0][i] = (dp[0][i - 1] + dp[0][i - 3] + 2 * dp[1][i - 2]) % MOD;
            dp[1][i] = (dp[2][i - 2] + dp[1][i - 3]) % MOD;
            dp[2][i] = (dp[2][i - 3] + dp[0][i - 2]) % MOD;
        }
        return dp[0][N];
    }
}
