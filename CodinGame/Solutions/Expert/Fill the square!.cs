using System;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var initialGrid = new char[n, n];
        for (var i = 0; i < n; i++)
        {
            var row = Console.ReadLine();
            for (var j = 0; j < n; j++)
            {
                initialGrid[i, j] = row[j];
            }
        }
        var solver = new LightsOutSolver(n, initialGrid);
        var solutionGrid = solver.Solve();
        for (var i = 0; i < n; i++)
        {
            var line = new StringBuilder(n);
            for (var j = 0; j < n; j++)
            {
                line.Append(solutionGrid[i, j]);
            }
            Console.WriteLine(line.ToString());
        }
    }
}

class LightsOutSolver
{
    private readonly int _size;
    private readonly char[,] _initialGrid;

    public LightsOutSolver(int size, char[,] initialGrid)
    {
        _size = size;
        _initialGrid = initialGrid;
    }

    public char[,] Solve()
    {
        var limit = 1 << _size;
        for (var i = 0; i < limit; i++)
        {
            var currentGrid = (char[,])_initialGrid.Clone();
            var solution = new char[_size, _size];

            for (var c = 0; c < _size; c++)
            {
                if (((i >> c) & 1) == 1)
                {
                    solution[0, c] = 'X';
                    ApplyTouch(currentGrid, 0, c);
                }
                else
                {
                    solution[0, c] = '.';
                }
            }

            for (var r = 1; r < _size; r++)
            {
                for (var c = 0; c < _size; c++)
                {
                    if (currentGrid[r - 1, c] == '.')
                    {
                        solution[r, c] = 'X';
                        ApplyTouch(currentGrid, r, c);
                    }
                    else
                    {
                        solution[r, c] = '.';
                    }
                }
            }
            
            if (IsLastRowLit(currentGrid))
            {
                return solution;
            }
        }
        return null;
    }

    private bool IsLastRowLit(char[,] grid)
    {
        for (var c = 0; c < _size; c++)
        {
            if (grid[_size - 1, c] == '.')
            {
                return false;
            }
        }
        return true;
    }

    private void ApplyTouch(char[,] grid, int r, int c)
    {
        grid[r, c] = Flip(grid[r, c]);
        if (r > 0)
        {
            grid[r - 1, c] = Flip(grid[r - 1, c]);
        }
        if (r < _size - 1)
        {
            grid[r + 1, c] = Flip(grid[r + 1, c]);
        }
        if (c > 0)
        {
            grid[r, c - 1] = Flip(grid[r, c - 1]);
        }
        if (c < _size - 1)
        {
            grid[r, c + 1] = Flip(grid[r, c + 1]);
        }
    }

    private char Flip(char state)
    {
        return state == '.' ? '*' : '.';
    }
}