using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        int l = int.Parse(Console.ReadLine());
        var grid = new char[n, n];
        var light = new int[n, n];
        var candles = new List<(int x, int y)>();

        for (int i = 0; i < n; i++)
        {
            var row = Console.ReadLine().Split(' ');
            for (int j = 0; j < n; j++)
            {
                grid[i, j] = row[j][0];
                if (grid[i, j] == 'C')
                    candles.Add((i, j));
            }
        }

        foreach (var (x, y) in candles)
        {
            for (int dx = -l + 1; dx < l; dx++)
            {
                for (int dy = -l + 1; dy < l; dy++)
                {
                    int dist = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    if (dist >= l) continue;

                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= n || ny >= n) continue;

                    int intensity = l - dist;
                    if (intensity > 0)
                        light[nx, ny] = Math.Max(light[nx, ny], intensity);
                }
            }
        }

        int darkSpots = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (grid[i, j] == 'X' && light[i, j] == 0)
                    darkSpots++;

        // Debug
        Console.Error.WriteLine("Light map:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                Console.Error.Write(light[i, j] + " ");
            Console.Error.WriteLine();
        }

        Console.WriteLine(darkSpots);
    }
}
