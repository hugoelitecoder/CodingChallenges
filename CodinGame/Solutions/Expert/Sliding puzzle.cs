using System;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var firstLine = Console.ReadLine().Split(' ');
        var h = int.Parse(firstLine[0]);
        var w = int.Parse(firstLine[1]);
        var puzzle = new SlidingPuzzle(w, h);
        for (var i = 0; i < h; i++)
        {
            var pieces = Console.ReadLine().Split(' ');
            for (var j = 0; j < w; j++)
            {
                var piece = pieces[j];
                if (piece == ".")
                {
                    puzzle[i, j] = 0;
                }
                else
                {
                    puzzle[i, j] = int.Parse(piece);
                }
            }
        }
        var result = puzzle.Solve();
        Console.WriteLine(result);
    }
}

class SlidingPuzzle
{
    private readonly int _width;
    private readonly int _height;
    private readonly int[] _grid;
    private readonly int[] _goal;
    private int _maxDepth = 11;
    private static readonly (int dx, int dy)[] Moves = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public SlidingPuzzle(int w, int h)
    {
        _width = w;
        _height = h;
        var size = w * h;
        _grid = new int[size];
        _goal = new int[size];
        for (var i = 0; i < size - 1; i++)
        {
            _goal[i] = i + 1;
        }
        _goal[size - 1] = 0;
    }

    public int this[int row, int col]
    {
        get => _grid[row * _width + col];
        set => _grid[row * _width + col] = value;
    }

    public int Solve()
    {
        var emptyX = -1;
        var emptyY = -1;
        for (var i = 0; i < _height; i++)
        {
            for (var j = 0; j < _width; j++)
            {
                if (this[i, j] == 0)
                {
                    emptyY = i;
                    emptyX = j;
                    break;
                }
            }
            if (emptyX != -1)
            {
                break;
            }
        }
        if (emptyX == -1)
        {
            return -1;
        }
        return DFS(emptyX, emptyY, 0);
    }

    private int DFS(int emptyX, int emptyY, int depth)
    {
        if (depth >= _maxDepth)
        {
            return 1000;
        }
        if (_grid.SequenceEqual(_goal))
        {
            _maxDepth = depth;
            return 0;
        }
        var lowest = 1000;
        foreach (var move in Moves)
        {
            var newX = emptyX + move.dx;
            var newY = emptyY + move.dy;
            if (newX >= 0 && newX < _width && newY >= 0 && newY < _height)
            {
                var temp = this[newY, newX];
                this[emptyY, emptyX] = temp;
                this[newY, newX] = 0;
                var newScore = DFS(newX, newY, depth + 1) + 1;
                this[newY, newX] = temp;
                this[emptyY, emptyX] = 0;
                if (newScore < lowest)
                {
                    lowest = newScore;
                }
            }
        }
        return lowest;
    }
}