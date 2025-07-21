using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static readonly int[] DR = { -1, 0, 1,  0 };
    static readonly int[] DC = {  0, 1, 0, -1 };
    static readonly int[] Mask = { 1|2, 2|4, 4|8, 8|1 };

    static void Main()
    {
        int rows = int.Parse(Console.ReadLine());
        int cols = int.Parse(Console.ReadLine());

        var shapes = GetShapeMap();
        var grid   = ReadGrid(rows, cols, shapes);

        var (bestR, bestC, bestScore) = FindBestStart(grid, rows, cols);

        Console.WriteLine($"{bestR} {bestC}");
        Console.WriteLine(bestScore);
    }

    static Dictionary<string,int> GetShapeMap() => new Dictionary<string,int>
    {
        ["  |  " + "  +--" + "     "] = 0,
        ["     " + "  +--" + "  |  "] = 1,
        ["     " + "--+  " + "  |  "] = 2,
        ["  |  " + "--+  " + "     "] = 3
    };

    static int[,] ReadGrid(int rows, int cols, Dictionary<string,int> shapes)
    {
        var grid = new int[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            string l0 = Console.ReadLine();
            string l1 = Console.ReadLine();
            string l2 = Console.ReadLine();

            for (int c = 0; c < cols; c++)
            {
                string key = l0.Substring(c * 5, 5)
                           + l1.Substring(c * 5, 5)
                           + l2.Substring(c * 5, 5);
                grid[r, c] = shapes[key];
            }
        }
        return grid;
    }

    static (int r, int c, int score) FindBestStart(int[,] baseGrid, int rows, int cols)
    {
        int bestScore = -1, bestR = 0, bestC = 0;

        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            int score = ComputeScore(baseGrid, r, c, rows, cols);
            if (score > bestScore)
            {
                bestScore = score;
                bestR = r;
                bestC = c;
            }
        }

        return (bestR, bestC, bestScore);
    }

    static int ComputeScore(int[,] baseGrid, int sr, int sc, int rows, int cols)
    {
        var grid     = (int[,])baseGrid.Clone();
        var frontier = new List<(int r, int c)> { (sr, sc) };
        var next     = new List<(int r, int c)>();
        int sens     = 1, score = 0;

        while (frontier.Any())
        {
            foreach (var (r, c) in frontier.Distinct())
            {
                grid[r, c] = (grid[r, c] + sens + 4) % 4;
                score++;

                int m = Mask[grid[r, c]];
                for (int d = 0; d < 4; d++)
                {
                    if ((m & (1 << d)) == 0) continue;

                    int nr = r + DR[d], nc = c + DC[d];
                    if (nr < 0 || nr >= rows || nc < 0 || nc >= cols) continue;

                    if ((Mask[grid[nr, nc]] & (1 << ((d + 2) % 4))) != 0)
                        next.Add((nr, nc));
                }
            }

            frontier.Clear();
            frontier.AddRange(next);
            next.Clear();
            sens = -sens;
        }

        return score;
    }
}
