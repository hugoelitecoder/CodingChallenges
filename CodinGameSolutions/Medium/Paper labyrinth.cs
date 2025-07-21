using System;
using System.Collections.Generic;

class Solution
{
    static int W, H;
    static int[,] grid;
    static readonly (int dx,int dy, int bit)[] dirs = {
        ( 0, 1, 1),  // down  → bit 1
        (-1, 0, 2),  // left  → bit 2
        ( 0,-1, 4),  // up    → bit 4
        ( 1, 0, 8),  // right → bit 8
    };

    static int BFS((int x,int y) src, (int x,int y) dst)
    {
        var dist = new int[H, W];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                dist[y, x] = -1;

        var q = new Queue<(int x,int y)>();
        dist[src.y, src.x] = 0;
        q.Enqueue(src);

        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            if (x == dst.x && y == dst.y)
                return dist[y, x];

            int cell = grid[y, x];
            foreach (var (dx, dy, bit) in dirs)
            {
                if ((cell & bit) != 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || nx >= W || ny < 0 || ny >= H) continue;
                if (dist[ny, nx] != -1) continue;
                dist[ny, nx] = dist[y, x] + 1;
                q.Enqueue((nx, ny));
            }
        }

        return -1;
    }

    static void Main()
    {
        var parts = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
        var start = (x: parts[0], y: parts[1]);
        parts = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
        var rabbit = (x: parts[0], y: parts[1]);
        parts = Array.ConvertAll(Console.ReadLine().Split(), int.Parse);
        W = parts[0]; H = parts[1];

        grid = new int[H, W];
        for (int y = 0; y < H; y++)
        {
            var line = Console.ReadLine().Trim();
            for (int x = 0; x < W; x++)
                grid[y, x] = Convert.ToInt32(line[x].ToString(), 16);
        }

        int toRabbit   = BFS(start,  rabbit);
        int backToExit = BFS(rabbit, start);
        Console.WriteLine($"{toRabbit} {backToExit}");
    }
}
