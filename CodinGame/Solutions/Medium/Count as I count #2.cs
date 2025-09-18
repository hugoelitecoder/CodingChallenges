using System;
using System.Numerics;

class Solution
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        int target = 50 - n;
        if (target < 0)
        {
            Console.WriteLine(0);
            return;
        }
        BigInteger[] dp = new BigInteger[target + 1];
        dp[0] = 1;
        for (int s = 1; s <= target; s++)
        {
            BigInteger count = 0;
            if (s >= 1) count += dp[s - 1];
            for (int k = 2; k <= 12; k++)
            {
                if (s >= k)
                    count += dp[s - k] * 2;
            }
            dp[s] = count;
        }

        Console.WriteLine(dp[target]);
    }
}
