using System;
using System.Linq;
using System.Collections.Generic;

class Player
{
    static readonly Dictionary<(int dx, int dy), string> Dirs = new() {
        [(0, -1)] = "UP",    [(0,  1)] = "DOWN",
        [(-1, 0)] = "LEFT",  [(1,  0)] = "RIGHT"
    };

    static void Main()
    {
        var wh   = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var dd   = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int w = wh[0], h = wh[1];
        var rows = Enumerable.Range(0, h).Select(_ => Console.ReadLine()).ToArray();
        var st   = Console.ReadLine().Split().Select(int.Parse).ToArray();
        var start = (x: st[0] + 1, y: st[1] + 1);
        var goal  = (x: dd[0] + 1, y: dd[1] + 1);

        var grid = new char[h + 2, w + 2];
        for (int y = 0; y < h + 2; y++)
            for (int x = 0; x < w + 2; x++)
                grid[y, x] = '1';
        for (int y = 1; y <= h; y++)
            for (int x = 1; x <= w; x++)
                grid[y, x] = rows[y - 1][x - 1];

        var path = BFS(grid,
                       new List<(int x, int y)> { start },
                       new HashSet<(int x, int y)> { start },
                       new Dictionary<(int x, int y), (int x, int y)>(),
                       goal);
        if (path == null) return;

        var last = path[0];
        foreach (var cur in path.Skip(1))
        {
            Console.WriteLine(Dirs[(cur.x - last.x, cur.y - last.y)]);
            Console.ReadLine();
            last = cur;
        }
    }

    static List<(int x, int y)> BFS(
        char[,] grid,
        List<(int x, int y)> frontier,
        HashSet<(int x, int y)> seen,
        Dictionary<(int x, int y), (int x, int y)> cameFrom,
        (int x, int y) goal)
    {
        if (!frontier.Any()) return null;
        if (frontier.Contains(goal)) {
            var path = new List<(int, int)> { goal };
            while (cameFrom.TryGetValue(goal, out var prev))
            {
                goal = prev;
                path.Insert(0, goal);
            }
            return path;
        }

        var next = new List<(int, int)>();
        int H = grid.GetLength(0), W = grid.GetLength(1);
        foreach (var v in frontier)
            foreach (var d in Dirs.Keys)
            {
                var w = (x: v.x + d.dx, y: v.y + d.dy);
                if (w.x >= 0 && w.x < W && w.y >= 0 && w.y < H &&
                    grid[w.y, w.x] == '0' && seen.Add(w))
                {
                    cameFrom[w] = v;
                    next.Add(w);
                }
            }

        return BFS(grid, next, seen, cameFrom, goal);
    }
}
