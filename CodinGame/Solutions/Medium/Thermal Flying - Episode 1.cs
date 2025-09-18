using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        int width = int.Parse(Console.ReadLine());
        int height = int.Parse(Console.ReadLine());
        int t = int.Parse(Console.ReadLine());

        var (grid, startX, startY) = ParseGrid(width, height);
        var (finalX, finalY) = SimulateFlight(width, height, t, grid, startX, startY);

        Console.WriteLine($"{finalX} {finalY}");
    }

    static (int[][] grid, int startX, int startY) ParseGrid(int width, int height)
    {
        var grid = new int[height][];
        int startX = 0, startY = 0;

        for (int i = 0; i < height; i++)
        {
            string rowLine = Console.ReadLine().Trim();
            var row = new List<int>(width);
            bool negative = false;

            foreach (char c in rowLine)
            {
                if (row.Count >= width) break;
                if (c == '.' || c == '#') row.Add(0);
                else if (c == 'V')
                {
                    startX = row.Count;
                    startY = i;
                    row.Add(0);
                }
                else if (c == '-') negative = true;
                else if (char.IsDigit(c))
                {
                    row.Add(negative ? -(c - '0') : (c - '0'));
                    negative = false;
                }
            }

            if (row.Count != width)
            {
                row.Clear();
                var tokens = rowLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tok in tokens)
                {
                    if (tok == "." || tok == "#") row.Add(0);
                    else if (tok == "V")
                    {
                        startX = row.Count;
                        startY = i;
                        row.Add(0);
                    }
                    else row.Add(int.Parse(tok));
                }
            }

            grid[i] = row.ToArray();
        }

        return (grid, startX, startY);
    }

    static (int x, int y) SimulateFlight(int width, int height, int t, int[][] grid, int startX, int startY)
    {
        int x = startX;
        int y = height - 1 - startY;

        for (int step = 0; step < t; step++)
        {
            int newX = x + 1;
            int newY = y - 1;

            if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                break;

            int rowIdx = height - 1 - newY;
            newY += grid[rowIdx][newX];

            if (newY < 0 || newY >= height)
                break;

            x = newX;
            y = newY;
        }

        return (x, y);
    }
}
