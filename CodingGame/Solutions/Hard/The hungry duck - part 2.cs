using System;

class Solution
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int W = int.Parse(inputs[0]);
        int H = int.Parse(inputs[1]);

        int[][] Amounts = new int[H][];
        for (int i = 0; i < H; i++)
        {
            Amounts[i] = new int[W];
            inputs = Console.ReadLine().Split(' ');
            for (int j = 0; j < W; j++)
            {
                Amounts[i][j] = int.Parse(inputs[j]);
            }
        }

        int[][] dp = new int[H][];
        for (int i = 0; i < H; i++)
        {
            dp[i] = new int[W];
        }
        dp[0][0] = Amounts[0][0];
        for (int j = 1; j < W; j++)
        {
            dp[0][j] = Amounts[0][j] + dp[0][j - 1];
        }
        for (int i = 1; i < H; i++)
        {
            dp[i][0] = Amounts[i][0] + dp[i - 1][0];
        }
        for (int i = 1; i < H; i++)
        {
            for (int j = 1; j < W; j++)
            {
                dp[i][j] = Amounts[i][j] + Math.Max(dp[i - 1][j], dp[i][j - 1]);
            }
        }

        Console.WriteLine(dp[H - 1][W - 1]);
    }
}