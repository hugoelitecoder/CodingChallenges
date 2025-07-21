using System;

class Program
{
    static void Main()
    {
        int rows = int.Parse(Console.ReadLine());
        int cols = int.Parse(Console.ReadLine());
        var map = new string[rows];
        for (int r = 0; r < rows; r++)
            map[r] = Console.ReadLine().Trim();

        var dp = new int[rows + 1, cols + 1];
        dp[0, 1] = 1;

        for (int r = 1; r <= rows; r++)
            for (int c = 1; c <= cols; c++)
                if (map[r - 1][c - 1] == '0')
                    dp[r, c] = dp[r - 1, c] + dp[r, c - 1];

        Console.WriteLine(dp[rows, cols]);
    }
}