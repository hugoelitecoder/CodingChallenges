using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var size = int.Parse(Console.ReadLine());
        var grid = new int[size, size];
        for (var row = 0; row < size; row++)
        {
            var line = Console.ReadLine();
            for (var col = 0; col < size; col++)
                grid[row, col] = line[col] - '0';
        }
        var solver = new ShadedPuzzleSolver(grid, size);
        var result = solver.Solve();
        for (var row = 0; row < size; row++)
        {
            for (var col = 0; col < size; col++)
                Console.Write(result[row, col] == 0 ? "*" : result[row, col].ToString());
            Console.WriteLine();
        }
    }
}

class ShadedPuzzleSolver
{
    private readonly int[,] _grid;
    private readonly int _size;
    private readonly int[] _dRow = { -1, 1, 0, 0 };
    private readonly int[] _dCol = { 0, 0, 1, -1 };
    public ShadedPuzzleSolver(int[,] grid, int size)
    {
        _grid = grid;
        _size = size;
    }
    public int[,] Solve()
    {
        var (valid, resultGrid) = CheckAndShade(_grid);
        return resultGrid;
    }
    private (bool, int[,]) CheckAndShade(int[,] current)
    {
        for (var row = 0; row < _size; row++)
        {
            var line = new List<int>();
            for (var col = 0; col < _size; col++) line.Add(current[row, col]);
            for (var col = 0; col < _size; col++)
            {
                var value = line[col];
                if (value != 0 && Count(line, value) != 1)
                {
                    var next = Clone(current);
                    next[row, col] = 0;
                    var (ok, result) = CheckAndShade(next);
                    if (ok) return (true, result);
                    if (col != line.IndexOf(value)) return (false, current);
                }
                if (row != 0 && value == 0 && current[row - 1, col] == 0) return (false, current);
            }
        }
        for (var col = 0; col < _size; col++)
        {
            var column = new List<int>();
            for (var row = 0; row < _size; row++) column.Add(current[row, col]);
            for (var row = 0; row < _size; row++)
            {
                var value = column[row];
                if (value != 0 && Count(column, value) != 1)
                {
                    var next = Clone(current);
                    next[row, col] = 0;
                    var (ok, result) = CheckAndShade(next);
                    if (ok) return (true, result);
                    if (row != column.IndexOf(value)) return (false, current);
                }
                if (col != 0 && value == 0 && current[row, col - 1] == 0) return (false, current);
            }
        }
        var sRow = 0;
        var sCol = 0;
        if (current[0, 0] == 0) sCol = 1;
        var cleared = DFS_Clear(current, sRow, sCol);
        if (Max(cleared) != 0) return (false, current);
        return (true, current);
    }
    private int[,] DFS_Clear(int[,] mat, int row, int col)
    {
        if (mat[row, col] == 0) return mat;
        var newMat = Clone(mat);
        newMat[row, col] = 0;
        for (var d = 0; d < 4; d++)
        {
            var nr = row + _dRow[d];
            var nc = col + _dCol[d];
            if (nr >= 0 && nr < _size && nc >= 0 && nc < _size && newMat[nr, nc] != 0)
                newMat = DFS_Clear(newMat, nr, nc);
        }
        return newMat;
    }
    private int[,] Clone(int[,] src)
    {
        var dst = new int[_size, _size];
        for (var i = 0; i < _size; i++)
            for (var j = 0; j < _size; j++)
                dst[i, j] = src[i, j];
        return dst;
    }
    private int Count(List<int> list, int value)
    {
        var cnt = 0;
        foreach (var v in list)
            if (v == value) cnt++;
        return cnt;
    }
    private int Max(int[,] mat)
    {
        var mx = 0;
        for (var i = 0; i < _size; i++)
            for (var j = 0; j < _size; j++)
                if (mat[i, j] > mx) mx = mat[i, j];
        return mx;
    }
}
