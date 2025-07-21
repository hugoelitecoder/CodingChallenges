using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static int L, H, paddedL, paddedH;
    static char[][] map;
    static int[,] area;
    static readonly (int dx, int dy)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    static void Main()
    {
        L = int.Parse(Console.ReadLine());
        H = int.Parse(Console.ReadLine());
        paddedL = L + 2;
        paddedH = H + 2;

        var rows = new string[H];
        for (int i = 0; i < H; i++)
            rows[i] = Console.ReadLine();

        InitializeMap(rows);
        area = new int[paddedH, paddedL];

        int queries = int.Parse(Console.ReadLine());
        while (queries-- > 0)
        {
            var parts = Console.ReadLine().Split();
            int x = int.Parse(parts[0]) + 1;
            int y = int.Parse(parts[1]) + 1;
            Console.WriteLine(GetLakeArea(x, y));
        }
    }

    static void InitializeMap(string[] rows)
    {
        map = new char[paddedH][];
        map[0] = Enumerable.Repeat('#', paddedL).ToArray();
        for (int y = 1; y <= H; y++)
            map[y] = new[] { '#' }
                .Concat(rows[y - 1].ToCharArray())
                .Concat(new[] { '#' })
                .ToArray();
        map[paddedH - 1] = Enumerable.Repeat('#', paddedL).ToArray();
    }

    static int GetLakeArea(int x, int y)
    {
        if (map[y][x] == '#')
            return 0;
        if (area[y, x] == 0)
            BFS(x, y);
        return area[y, x];
    }

    static void BFS(int sx, int sy)
    {
        var queue = new Queue<(int X, int Y)>();
        var cells = new List<(int X, int Y)>();
        EnqueueIfWater(sx, sy, queue);
        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            cells.Add((x, y));
            foreach (var d in dirs)
                EnqueueIfWater(x + d.dx, y + d.dy, queue);
        }
        int count = cells.Count;
        foreach (var (x, y) in cells)
            area[y, x] = count;
    }

    static void EnqueueIfWater(int x, int y, Queue<(int X, int Y)> q)
    {
        if (map[y][x] == 'O' && area[y, x] == 0)
        {
            area[y, x] = -1;
            q.Enqueue((x, y));
        }
    }
}
