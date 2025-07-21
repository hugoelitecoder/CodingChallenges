using System;
using System.Linq;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine()!);
        var grid = Enumerable.Range(0, n)
                             .Select(_ => Console.ReadLine()!.ToCharArray())
                             .ToArray();

        int half = n / 2;
        int[] sums = new int[4];
        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++)
            {
                if (!char.IsDigit(grid[r][c])) continue;
                int val = grid[r][c] - '0';
                int quad = (r < half ? 0 : 2) + (c < half ? 0 : 1);
                sums[quad] += val;
            }
        }

        // Find the odd and standard sums
        var freq = sums.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        int oddValue = freq.First(kv => kv.Value == 1).Key;
        int standardValue = freq.First(kv => kv.Value > 1).Key;

        // Which quadrant had oddValue?
        int oddQuad = Array.FindIndex(sums, x => x == oddValue) + 1;

        Console.WriteLine($"Quad-{oddQuad} is Odd-Quad-Out");
        Console.WriteLine($"It has value of {oddValue}");
        Console.WriteLine($"Others have value of {standardValue}");
    }
}
