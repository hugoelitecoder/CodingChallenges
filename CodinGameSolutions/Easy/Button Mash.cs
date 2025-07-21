using System;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        if (n <= 1)
        {
             Console.WriteLine(n);
             return;
        }

        var dp = new int[n + 1];
        dp[1] = 1;
        for (var i = 2; i <= n; i++)
        {
            if (i % 2 == 0)
            {
                var costFromPrev = dp[i - 1] + 1;
                var costFromHalf = dp[i / 2] + 1;
                dp[i] = Math.Min(costFromPrev, costFromHalf);
            }
            else
            {
                var costFromPrev = dp[i - 1] + 1;
                var costFromNext = dp[(i + 1) / 2] + 2;
                dp[i] = Math.Min(costFromPrev, costFromNext);
            }
        }
        Console.WriteLine(dp[n]);
    }
}