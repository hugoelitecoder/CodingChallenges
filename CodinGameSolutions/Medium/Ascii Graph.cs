using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var points = new HashSet<(int x, int y)>();
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split();
            points.Add((int.Parse(parts[0]), int.Parse(parts[1])));
        }

        int minGivenX = points.Count > 0 ? points.Min(p => p.x) : 0;
        int maxGivenX = points.Count > 0 ? points.Max(p => p.x) : 0;
        int minGivenY = points.Count > 0 ? points.Min(p => p.y) : 0;
        int maxGivenY = points.Count > 0 ? points.Max(p => p.y) : 0;

        int minX = Math.Min(minGivenX, 0) - 1;
        int maxX = Math.Max(maxGivenX, 0) + 1;
        int minY = Math.Min(minGivenY, 0) - 1;
        int maxY = Math.Max(maxGivenY, 0) + 1;

        for (int y = maxY; y >= minY; y--)
        {
            var line = new char[maxX - minX + 1];
            for (int x = minX; x <= maxX; x++)
            {
                if (points.Contains((x, y)))
                    line[x - minX] = '*';
                else if (x == 0 && y == 0)
                    line[x - minX] = '+';
                else if (y == 0)
                    line[x - minX] = '-';
                else if (x == 0)
                    line[x - minX] = '|';
                else
                    line[x - minX] = '.';
            }
            Console.WriteLine(new string(line));
        }
    }
}