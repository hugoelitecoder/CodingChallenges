using System;

class Solution
{
    public static void Main()
    {
        var parts = Console.ReadLine().Split();
        var w = int.Parse(parts[0]);
        var h = int.Parse(parts[1]);
        var n = int.Parse(parts[2]);
        var sx = int.Parse(parts[3]);
        var sy = int.Parse(parts[4]);
        var seed = uint.Parse(parts[5]);
        var grid = Generate(w, h, n, sx, sy, seed);
        Print(grid);
    }
    private static Cell[,] Generate(int w, int h, int mines, int sx, int sy, uint seed)
    {
        var grid = new Cell[h, w];
        var rng = new LCG(seed);
        for (var dy = -1; dy <= 1; dy++)
            for (var dx = -1; dx <= 1; dx++)
                if (IsValid(sx + dx, sy + dy, w, h))
                    grid[sy + dy, sx + dx].IsForbidden = true;
        for (var placed = 0; placed < mines; )
        {
            var x = (int)(rng.Next() % (uint)w);
            var y = (int)(rng.Next() % (uint)h);
            ref var cell = ref grid[y, x];
            if (!cell.IsMine && !cell.IsForbidden)
            {
                cell.IsMine = true;
                placed++;
            }
        }
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
                if (!grid[y, x].IsMine)
                {
                    var cnt = 0;
                    foreach (var (dx, dy) in _dirs)
                        if (IsValid(x + dx, y + dy, w, h) && grid[y + dy, x + dx].IsMine)
                            cnt++;
                    grid[y, x].NeighborMines = cnt;
                }
        return grid;
    }
    private static void Print(Cell[,] grid)
    {
        var h = grid.GetLength(0);
        var w = grid.GetLength(1);
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
                Console.Write(grid[y, x]);
            Console.WriteLine();
        }
    }
    private static bool IsValid(int x, int y, int w, int h) =>
        x >= 0 && x < w && y >= 0 && y < h;
    private struct Cell
    {
        public int NeighborMines;
        public bool IsMine;
        public bool IsForbidden;
        public override string ToString() =>
            IsMine ? "#" : (NeighborMines > 0 ? NeighborMines.ToString() : ".");
    }
    private class LCG
    {
        private uint _state;
        public LCG(uint seed) => _state = seed;
        public uint Next() => _state = (214013u * _state + 2531011u) >> 16;
    }
    private static readonly (int dx, int dy)[] _dirs = {
        (-1, -1), (0, -1), (1, -1),
        (-1,  0),          (1,  0),
        (-1,  1), (0,  1), (1,  1)
    };
}
