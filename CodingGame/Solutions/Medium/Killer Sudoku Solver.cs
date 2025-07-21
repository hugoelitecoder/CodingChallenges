using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var grid = new int[9, 9];
        var cageId = new char[9, 9];
        for (var r = 0; r < 9; r++)
        {
            var parts = Console.ReadLine().Split();
            var g = parts[0];
            var ids = parts[1];
            for (var c = 0; c < 9; c++)
            {
                grid[r, c]   = g[c] == '.' ? 0 : g[c] - '0';
                cageId[r, c] = ids[c];
            }
        }

        var cageTarget = Console.ReadLine()
            .Split()
            .Select(tok => tok.Split('='))
            .ToDictionary(a => a[0][0], a => int.Parse(a[1]));

        var solver = new KillerSudoku(grid, cageId, cageTarget);
        if (solver.Solve())
        {
            solver.PrintSolution();
        }
        else
        {
            Console.WriteLine("No solution found.");
        }
    }
}

class KillerSudoku
{
    private readonly int[,] _grid;
    private readonly char[,] _cageId;
    private readonly Dictionary<char, int> _cageTarget;
    private Dictionary<char, List<(int r, int c)>> _cageCells;
    private Dictionary<char, int> _cageSum, _cageCount, _cageSize;
    private Dictionary<char, bool[]> _cageUsed;
    private bool[,] _rowUsed, _colUsed, _boxUsed;
    private List<(int r, int c)> _empties;

    public KillerSudoku(int[,] grid, char[,] cageId, Dictionary<char, int> cageTarget)
    {
        _grid       = grid;
        _cageId     = cageId;
        _cageTarget = cageTarget;
        Initialize();
    }

    public bool Solve()
    {
        return Solve(0);
    }

    public void PrintSolution()
    {
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
                Console.Write(_grid[r, c]);
            Console.WriteLine();
        }
    }

    private void Initialize()
    {
        _cageCells = _cageTarget.Keys
            .ToDictionary(k => k, k => new List<(int r, int c)>());
        for (var r = 0; r < 9; r++)
            for (var c = 0; c < 9; c++)
                _cageCells[_cageId[r, c]].Add((r, c));

        _cageSum   = _cageCells.Keys.ToDictionary(k => k, k => 0);
        _cageCount = _cageCells.Keys.ToDictionary(k => k, k => 0);
        _cageSize  = _cageCells.ToDictionary(kv => kv.Key, kv => kv.Value.Count);
        _cageUsed  = _cageCells.Keys.ToDictionary(k => k, k => new bool[10]);

        _rowUsed = new bool[9, 10];
        _colUsed = new bool[9, 10];
        _boxUsed = new bool[9, 10];
        _empties = new List<(int r, int c)>();

        for (var r = 0; r < 9; r++)
            for (var c = 0; c < 9; c++)
            {
                var v = _grid[r, c];
                if (v != 0)
                {
                    _rowUsed[r, v] = _colUsed[c, v] = _boxUsed[(r / 3) * 3 + (c / 3), v] = true;
                    var id = _cageId[r, c];
                    _cageUsed[id][v] = true;
                    _cageSum[id]   += v;
                    _cageCount[id] += 1;
                }
                else
                {
                    _empties.Add((r, c));
                }
            }
    }

    private bool Solve(int idx)
    {
        if (idx == _empties.Count)
            return true;

        var (r, c)    = _empties[idx];
        var b         = (r / 3) * 3 + (c / 3);
        var id        = _cageId[r, c];
        var usedCount = _cageCount[id];
        var size      = _cageSize[id];
        var sumSoFar  = _cageSum[id];
        var rem       = size - usedCount - 1;
        var target    = _cageTarget[id];

        for (var d = 1; d <= 9; d++)
        {
            if (_rowUsed[r, d] || _colUsed[c, d] || _boxUsed[b, d] || _cageUsed[id][d])
                continue;

            var newSum = sumSoFar + d;
            if (rem == 0
                ? newSum != target
                : newSum + rem * 1 > target || newSum + rem * 9 < target)
                continue;

            PlaceDigit(r, c, b, id, d);
            if (Solve(idx + 1))
                return true;
            RemoveDigit(r, c, b, id, d);
        }

        return false;
    }

    private void PlaceDigit(int r, int c, int b, char id, int d)
    {
        _grid[r, c]       = d;
        _rowUsed[r, d]    = true;
        _colUsed[c, d]    = true;
        _boxUsed[b, d]    = true;
        _cageUsed[id][d]  = true;
        _cageSum[id]     += d;
        _cageCount[id]   += 1;
    }

    private void RemoveDigit(int r, int c, int b, char id, int d)
    {
        _grid[r, c]       = 0;
        _rowUsed[r, d]    = false;
        _colUsed[c, d]    = false;
        _boxUsed[b, d]    = false;
        _cageUsed[id][d]  = false;
        _cageSum[id]     -= d;
        _cageCount[id]   -= 1;
    }
}
