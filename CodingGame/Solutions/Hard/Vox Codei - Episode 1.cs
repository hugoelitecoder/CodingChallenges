using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var dims  = Console.ReadLine().Split();
        int width  = int.Parse(dims[0]);
        int height = int.Parse(dims[1]);

        var grid = new char[height, width];
        for (int row = 0; row < height; row++)
        {
            var line = Console.ReadLine();
            for (int col = 0; col < width; col++)
                grid[row, col] = line[col];
        }

        var game = new BombGame(width, height, grid);

        string input;
        while ((input = Console.ReadLine()) != null)
        {
            var parts = input.Split();
            int rounds = int.Parse(parts[0]);
            int bombs  = int.Parse(parts[1]);

            Console.WriteLine(game.ProcessTurn(rounds, bombs));
        }
    }
}

class BombGame
{
    private const int DefaultFuse       = 3;
    private const int ExplosionRadius   = 3;
    private static readonly (int dx, int dy)[] Directions = {(1,0),(-1,0),(0,1),(0,-1)};
    private readonly int _width;
    private readonly int _height;
    private readonly char[,] _currentGrid;
    private readonly char[,] _predictedGrid;
    private readonly List<Bomb> _scheduledBombs = new List<Bomb>();

    private struct Bomb { public int X, Y, Fuse; }

    public BombGame(int width, int height, char[,] initialGrid)
    {
        _width         = width;
        _height        = height;
        _currentGrid   = (char[,])initialGrid.Clone();
        _predictedGrid = (char[,])initialGrid.Clone();
    }

    public string ProcessTurn(int round, int bombCount)
    {
        for (int i = _scheduledBombs.Count - 1; i >= 0; i--)
        {
            var b = _scheduledBombs[i];
            if (--b.Fuse == 0)
            {
                Detonate(_currentGrid, b.X, b.Y);
                _scheduledBombs.RemoveAt(i);
            }
            else
            {
                _scheduledBombs[i] = b;
            }
        }

        int bestScore = 0, bestX = 0, bestY = 0;
        for (int y = 0; y < _height; y++)
        for (int x = 0; x < _width;  x++)
        {
            if (_predictedGrid[y, x] != '.') continue;

            int score = CalculateTargets(_predictedGrid, x, y);
            if (score <= bestScore || bombCount == 0) continue;
            if (!IsWinningMove(_predictedGrid, x, y, bombCount - 1)) continue;

            bestScore = score;
            bestX     = x;
            bestY     = y;
        }

        if (bestScore == 0)
            return "WAIT";

        bool occupied = _scheduledBombs.Exists(b => b.X == bestX && b.Y == bestY);
        if (_currentGrid[bestY, bestX] == '.' && !occupied)
        {
            Detonate(_predictedGrid, bestX, bestY);
            _scheduledBombs.Add(new Bomb { X = bestX, Y = bestY, Fuse = DefaultFuse });
            return $"{bestX} {bestY}";
        }

        return "WAIT";
    }

    private int CalculateTargets(char[,] grid, int x, int y)
    {
        int count = 0;
        foreach (var (dx, dy) in Directions)
        {
            for (int r = 1; r <= ExplosionRadius; r++)
            {
                int nx = x + dx * r, ny = y + dy * r;
                if (!InBounds(nx, ny) || grid[ny, nx] == '#') break;
                if (grid[ny, nx] == '@') count++;
            }
        }
        return count;
    }

    private void Detonate(char[,] grid, int x, int y)
    {
        foreach (var (dx, dy) in Directions)
        {
            for (int r = 1; r <= ExplosionRadius; r++)
            {
                int nx = x + dx * r, ny = y + dy * r;
                if (!InBounds(nx, ny) || grid[ny, nx] == '#') break;
                if (grid[ny, nx] == '@') grid[ny, nx] = '.';
            }
        }
    }

    private int CountTargets(char[,] grid)
    {
        int count = 0;
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (grid[y, x] == '@') count++;
        return count;
    }

    private bool IsWinningMove(char[,] grid, int x, int y, int remainingBombs)
    {
        var clone = (char[,])grid.Clone();
        Detonate(clone, x, y);

        int maxHits = 0;
        for (int yy = 0; yy < _height; yy++)
        for (int xx = 0; xx < _width;  xx++)
        {
            if (clone[yy, xx] != '.') continue;
            int hits = CalculateTargets(clone, xx, yy);
            if (hits > maxHits) maxHits = hits;
        }

        int remTargets = CountTargets(clone);
        return (long)maxHits * remainingBombs >= remTargets;
    }

    private bool InBounds(int x, int y)
        => x >= 0 && x < _width && y >= 0 && y < _height;
}
