using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        int S = int.Parse(Console.ReadLine());
        int[] coins = Console.ReadLine()
                             .Split(' ')
                             .Select(int.Parse)
                             .ToArray();

        long[] dp = new long[N + 1];
        dp[0] = 1;  
        foreach (int c in coins)
        {
            for (int amt = c; amt <= N; amt++)
            {
                dp[amt] += dp[amt - c];
            }
        }

        Console.WriteLine(dp[N]);
    }
}
