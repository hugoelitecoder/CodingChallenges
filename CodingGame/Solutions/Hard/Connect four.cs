using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var rows = 6;
        var cols = 7;
        var grid = new char[rows, cols];
        for (var r = 0; r < rows; r++)
        {
            var line = Console.ReadLine();
            for (var c = 0; c < cols; c++)
                grid[r, c] = line[c];
        }
        var board = new ConnectFourBoard(grid);

        var winCols1 = board.GetImmediateWinningColumns('1');
        var winCols2 = board.GetImmediateWinningColumns('2');

        Console.WriteLine(winCols1.Count > 0 ? string.Join(" ", winCols1) : "NONE");
        Console.WriteLine(winCols2.Count > 0 ? string.Join(" ", winCols2) : "NONE");
    }
}

class ConnectFourBoard
{
    private readonly char[,] _cells;
    private const int Rows = 6;
    private const int Cols = 7;

    public ConnectFourBoard(char[,] cells)
    {
        _cells = cells;
    }

    public List<int> GetImmediateWinningColumns(char player)
    {
        var winningCols = new List<int>();
        for (int col = 0; col < Cols; col++)
        {
            int dropRow = GetFirstEmptyRow(col);
            if (dropRow == -1) continue;
            if (IsWinningMove(dropRow, col, player))
                winningCols.Add(col);
        }
        return winningCols;
    }

    private int GetFirstEmptyRow(int col)
    {
        for (int row = Rows - 1; row >= 0; row--)
            if (_cells[row, col] == '.')
                return row;
        return -1;
    }

    private bool IsWinningMove(int row, int col, char player)
    {
        _cells[row, col] = player;
        bool win = CheckFourInLine(row, col, player);
        _cells[row, col] = '.';
        return win;
    }

    private bool CheckFourInLine(int row, int col, char player)
    {
        int[][] directions = new int[][]
        {
            new int[]{ 1, 0 }, 
            new int[]{ 0, 1 }, 
            new int[]{ 1, 1 }, 
            new int[]{ 1, -1 } 
        };
        foreach (var dir in directions)
        {
            int count = 1;
            count += CountDirection(row, col, dir[0], dir[1], player);
            count += CountDirection(row, col, -dir[0], -dir[1], player);
            if (count >= 4) return true;
        }
        return false;
    }

    private int CountDirection(int row, int col, int dRow, int dCol, char player)
    {
        int r = row + dRow;
        int c = col + dCol;
        int count = 0;
        while (IsWithinBounds(r, c) && _cells[r, c] == player)
        {
            count++;
            r += dRow;
            c += dCol;
        }
        return count;
    }

    private bool IsWithinBounds(int row, int col)
    {
        return row >= 0 && row < Rows && col >= 0 && col < Cols;
    }
}
