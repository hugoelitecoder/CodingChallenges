using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var input = Console.ReadLine().Split();
        int H = int.Parse(input[0]), W = int.Parse(input[1]);

        var arrows = new List<(int x, int y, char d)>();
        for (int y = 0; y < H; y++)
        {
            var row = Console.ReadLine();
            for (int x = 0; x < W; x++)
            {
                char c = row[x];
                if ("^v<>".Contains(c)) arrows.Add((x, y, c));
            }
        }

        var dirMap = new Dictionary<char, (int dx, int dy)>
        {
            ['^'] = (0, -1),
            ['v'] = (0, 1),
            ['<'] = (-1, 0),
            ['>'] = (1, 0),
        };

        int steps = 0;
        while (arrows.Count > 0)
        {
            var moved = new Dictionary<(int, int), List<(int x, int y, char d)>>();

            foreach (var (x, y, d) in arrows)
            {
                var (dx, dy) = dirMap[d];
                int nx = (x + dx + W) % W;
                int ny = (y + dy + H) % H;
                var pos = (nx, ny);
                if (!moved.ContainsKey(pos)) moved[pos] = new List<(int, int, char)>();
                moved[pos].Add((nx, ny, d));
            }
            arrows = moved.Values.Where(g => g.Count == 1).SelectMany(g => g).ToList();

            steps++;
        }
        Console.WriteLine(steps);
    }
}
