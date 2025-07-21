using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static int W, H, PW, PH;
    static int[,] Maze;
    static readonly (int dx,int dy)[] Dirs = { (1,0),(-1,0),(0,1),(0,-1) };

    static void Main()
    {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        W = dims[0]; H = dims[1];
        var st = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int sx = st[0]+1, sy = st[1]+1;

        PW = W + 2; PH = H + 2;
        Maze = new int[PH, PW];
        for (int y = 0; y < PH; y++)
            for (int x = 0; x < PW; x++)
                Maze[y,x] = -1;
        for (int y = 1; y <= H; y++)
        {
            var line = Console.ReadLine();
            for (int x = 1; x <= W; x++)
                Maze[y, x] = line[x-1] == '#' ? -1 : 0;
        }

        var exits = new List<(int x,int y)>();
        DFS(sx, sy, exits);

        exits = exits.Distinct()
                       .Select(p => (x: p.x-1, y: p.y-1))
                       .OrderBy(p => p.x)
                       .ThenBy(p => p.y)
                       .ToList();

        Console.WriteLine(exits.Count);
        exits.ForEach(p => Console.WriteLine($"{p.x} {p.y}"));
    }

    static void DFS(int x, int y, List<(int x,int y)> exits)
    {
        if (Maze[y,x] != 0) return;
        Maze[y,x] = 1;
        if (x == 1 || x == W || y == 1 || y == H)
            exits.Add((x,y));

        foreach (var (dx,dy) in Dirs)
        {
            int nx = x+dx, ny = y+dy;
            if (Maze[ny,nx] == 0)
                DFS(nx, ny, exits);
        }
    }
}