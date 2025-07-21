using System;

class Solution
{
    public static void Main(string[] args)
    {
        var grid = new int[4, 4];
        for (var i = 0; i < 4; i++)
        {
            var line = Console.ReadLine();
            for (var j = 0; j < 4; j++)
            {
                grid[i, j] = line[j] - '0';
            }
        }
        var solver = new Sudoku4x4(grid);
        solver.Solve();
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                Console.Write(solver.Grid[i, j]);
            }
            Console.WriteLine();
        }
    }
}

class Sudoku4x4
{
    public int[,] Grid { get; }
    public Sudoku4x4(int[,] grid)
    {
        Grid = new int[4, 4];
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                Grid[i, j] = grid[i, j];
            }
        }
    }
    public bool Solve()
    {
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                if (Grid[i, j] == 0)
                {
                    for (var d = 1; d <= 4; d++)
                    {
                        if (IsValid(i, j, d))
                        {
                            Grid[i, j] = d;
                            if (Solve()) return true;
                            Grid[i, j] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }
    bool IsValid(int row, int col, int d)
    {
        for (var k = 0; k < 4; k++)
        {
            if (Grid[row, k] == d) return false;
            if (Grid[k, col] == d) return false;
        }
        var baseRow = (row / 2) * 2;
        var baseCol = (col / 2) * 2;
        for (var i = baseRow; i < baseRow + 2; i++)
        {
            for (var j = baseCol; j < baseCol + 2; j++)
            {
                if (Grid[i, j] == d) return false;
            }
        }
        return true;
    }
}
