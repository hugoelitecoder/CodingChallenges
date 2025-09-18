using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var a = Console.ReadLine().Split().Select(int.Parse).ToArray();

        var cost = new int[n, n + 1];
        for (int l = 0; l < n; l++)
        {
            for (int r = l + 1; r <= n; r++)
            {
                cost[l, r] = cost[l, r - 1] + ((a[r - 1] == (r - l - 1)) ? 0 : 1);
            }
        }

        var dp = new int[n + 1, n + 1];
        for (int i = 0; i <= n; i++)
            for (int k = 0; k <= n; k++)
                dp[i, k] = int.MaxValue / 2;
        dp[0, 0] = 0;
        dp[0, 1] = 0;

        for (int i = 1; i <= n; i++)
        {
            for (int k = 1; k <= i; k++)
            {
                for (int j = k - 1; j < i; j++)
                {
                    if (dp[j, k - 1] < int.MaxValue / 2)
                        dp[i, k] = Math.Min(dp[i, k], dp[j, k - 1] + cost[j, i]);
                }
            }
        }

        for (int k = 1; k <= n; k++)
            Console.WriteLine(dp[n, k]);
    }
}
