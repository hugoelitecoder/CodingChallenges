using System;
using System.Collections.Generic;

class Solution
{
    static readonly (int dx, int dy)[] EvenOffsets = {
        ( 1,  0), (-1,  0),
        ( 0,  1), ( 0, -1),
        (-1,  1), (-1, -1)
    };
    static readonly (int dx, int dy)[] OddOffsets = {
        ( 1,  0), (-1,  0),
        ( 0,  1), ( 0, -1),
        ( 1,  1), ( 1, -1)
    };

    static void Main()
    {
        var parts = Console.ReadLine().Split();
        int w = int.Parse(parts[0]), h = int.Parse(parts[1]);

        var grid = new char[h][];
        (int sx, int sy) = (-1, -1);
        (int ex, int ey) = (-1, -1);

        for (int y = 0; y < h; y++)
        {
            grid[y] = Console.ReadLine().ToCharArray();
            for (int x = 0; x < w; x++)
            {
                if (grid[y][x] == 'S') { sx = x; sy = y; }
                else if (grid[y][x] == 'E') { ex = x; ey = y; }
            }
        }

        var q = new Queue<(int x, int y)>();
        bool[,] seen = new bool[h, w];
        var prev = new (int px, int py)[h, w];
        q.Enqueue((sx, sy));
        seen[sy, sx] = true;
        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            if (x == ex && y == ey) break;
            var offsets = (y % 2 == 0) ? EvenOffsets : OddOffsets;

            foreach (var (dx, dy) in offsets)
            {
                int nx = (x + dx + w) % w;
                int ny = (y + dy + h) % h;

                if (!seen[ny, nx] && grid[ny][nx] != '#')
                {
                    seen[ny, nx] = true;
                    prev[ny, nx] = (x, y);
                    q.Enqueue((nx, ny));
                }
            }
        }

        int cx = ex, cy = ey;
        while (!(cx == sx && cy == sy))
        {
            var (px, py) = prev[cy, cx];
            if (grid[cy][cx] == '_')
                grid[cy][cx] = '.';
            (cx, cy) = (px, py);
        }

        for (int y = 0; y < h; y++)
            Console.WriteLine(new string(grid[y]));
    }
}
