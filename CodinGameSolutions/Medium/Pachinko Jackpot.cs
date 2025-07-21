using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int height = int.Parse(Console.ReadLine());
        long[] best = new long[0];
        for (int row = 0; row < height; row++)
        {
            var inc = Console.ReadLine()
                             .Select(c => c - '0')
                             .ToArray();

            var next = new long[row + 1];
            for (int j = 0; j <= row; j++)
            {
                long left  = (j - 1 >= 0)   ? best[j - 1] : 0;
                long right = (j < best.Length) ? best[j]     : 0;
                next[j] = Math.Max(left, right) + inc[j];
            }
            best = next;
        }
        var prizes = Enumerable.Range(0, height + 1)
                               .Select(_ => long.Parse(Console.ReadLine()))
                               .ToArray();

        long jackpot = 0;
        for (int k = 0; k <= height; k++)
        {
            long pathSum = Math.Max(
                k - 1 >= 0   ? best[k - 1] : 0,
                k < best.Length ? best[k]     : 0
            );
            jackpot = Math.Max(jackpot, pathSum * prizes[k]);
        }

        Console.WriteLine(jackpot);
    }
}
