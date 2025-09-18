using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var s = Console.ReadLine().Split();
        int sy = int.Parse(s[0]), sx = int.Parse(s[1]);
        s = Console.ReadLine().Split();
        int ey = int.Parse(s[0]), ex = int.Parse(s[1]);
        s = Console.ReadLine().Split();
        int h = int.Parse(s[0]), w = int.Parse(s[1]);
        char[,] grid = new char[h, w];
        for (int i = 0; i < h; i++)
        {
            var line = Console.ReadLine();
            for (int j = 0; j < w; j++) grid[i, j] = line[j];
        }
        var solver = new MazeSolver(grid);
        Console.WriteLine(solver.FindShortestPath(sy, sx, ey, ex));
    }
}
record State(char Tile, int Y, int X, int Dist);

class MazeSolver
{
    private readonly char[,] grid;
    private readonly bool[,,] visited;
    private readonly int height, width;
    private static readonly (int y, int x)[] Vertical = { (1, 0), (-1, 0) };
    private static readonly (int y, int x)[] Horizontal = { (0, 1), (0, -1) };
    private static readonly (int y, int x)[] AllDirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public MazeSolver(char[,] grid)
    {
        this.grid = grid;
        height = grid.GetLength(0);
        width = grid.GetLength(1);
        visited = new bool[5, height, width];
    }

    public int FindShortestPath(int sy, int sx, int ey, int ex)
    {
        var queue = new Queue<State>();
        queue.Enqueue(new State('.', sy, sx, 0));
        MarkVisited('.', sy, sx);

        while (queue.Count > 0)
        {
            var (tile, y, x, dist) = queue.Dequeue();
            if (y == ey && x == ex && tile == '.') return dist;

            foreach (var (dy, dx) in GetDirections(tile))
            {
                int ny = y + dy, nx = x + dx;
                if (!InBounds(ny, nx)) continue;
                char nextTile = DetermineNextTile(tile, grid[ny, nx], dy, dx);
                if (nextTile == '\0') continue;
                int idx = TileIndex(nextTile);
                if (visited[idx, ny, nx]) continue;
                MarkVisited(nextTile, ny, nx);
                queue.Enqueue(new State(nextTile, ny, nx, dist + 1));
            }
        }
        return -1;
    }

    private static IEnumerable<(int y, int x)> GetDirections(char tile)
        => tile switch
        {
            '|' => Vertical,
            '-' => Horizontal,
            _   => AllDirs
        };

    private static char DetermineNextTile(char cur, char target, int dy, int dx)
    {
        if (target == cur) return cur;
        if (target == '-' && dy == 0) return '-';
        if (target == '|' && dx == 0) return '|';
        if ((cur == '-' || cur == '|') && (target == '.' || target == '+')) return target;
        if (target == 'X') return cur;
        return '\0';
    }

    private int TileIndex(char tile)
        => tile switch
        {
            '.' => 0,
            '+' => 1,
            '|' => 2,
            '-' => 3,
            'X' => 4,
            _   => 0
        };

    private void MarkVisited(char tile, int y, int x)
        => visited[TileIndex(tile), y, x] = true;

    private bool InBounds(int y, int x)
        => y >= 0 && y < height && x >= 0 && x < width;
}
