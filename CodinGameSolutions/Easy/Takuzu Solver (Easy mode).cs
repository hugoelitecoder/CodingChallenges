using System;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var lines = new string[n];
        for (var i = 0; i < n; i++)
        {
            lines[i] = Console.ReadLine();
        }
        var solver = new TakuzuSolver(n, lines);
        var solvedGrid = solver.Solve();
        for (var r = 0; r < n; r++)
        {
            var rowBuilder = new StringBuilder(n);
            for (var c = 0; c < n; c++)
            {
                rowBuilder.Append(solvedGrid[r, c]);
            }
            Console.WriteLine(rowBuilder.ToString());
        }
    }
}

public class TakuzuSolver
{
    private readonly int _size;
    private readonly char[,] _grid;
    private static readonly (int dr1, int dc1, int dr2, int dc2)[] _offsets =
    {
        (0, -1, 0, 1), (-1, 0, 1, 0),
        (0, -2, 0, -1), (-2, 0, -1, 0),
        (0, 1, 0, 2), (1, 0, 2, 0)
    };

    public TakuzuSolver(int size, string[] lines)
    {
        _size = size;
        _grid = new char[size, size];
        for (var r = 0; r < size; r++)
        {
            for (var c = 0; c < size; c++)
            {
                _grid[r, c] = lines[r][c];
            }
        }
    }

    public char[,] Solve()
    {
        var maxIterations = _size * _size;
        for (var i = 0; i < maxIterations; i++)
        {
            if (!ApplyRules())
            {
                break;
            }
        }
        return _grid;
    }

    private bool ApplyRules()
    {
        var madeChange = false;
        for (var r = 0; r < _size; r++)
        {
            for (var c = 0; c < _size; c++)
            {
                if (_grid[r, c] == '.')
                {
                    if (TryFillCell(r, c))
                    {
                        madeChange = true;
                    }
                }
            }
        }
        return madeChange;
    }

    private bool TryFillCell(int r, int c)
    {
        foreach (var (dr1, dc1, dr2, dc2) in _offsets)
        {
            var r1 = r + dr1;
            var c1 = c + dc1;
            var r2 = r + dr2;
            var c2 = c + dc2;
            if (r1 < 0 || r1 >= _size || c1 < 0 || c1 >= _size ||
                r2 < 0 || r2 >= _size || c2 < 0 || c2 >= _size)
            {
                continue;
            }
            var val1 = _grid[r1, c1];
            var val2 = _grid[r2, c2];
            if (val1 != '.' && val1 == val2)
            {
                _grid[r, c] = GetOpposite(val1);
                return true;
            }
        }
        return false;
    }

    private char GetOpposite(char digit)
    {
        return digit == '0' ? '1' : '0';
    }
}