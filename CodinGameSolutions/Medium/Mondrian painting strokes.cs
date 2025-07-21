using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var width = int.Parse(inputs[0]);
        var height = int.Parse(inputs[1]);
        var grid = new string[height];
        for (var i = 0; i < height; i++)
        {
            grid[i] = Console.ReadLine();
        }
        var solver = new MondrianSolver(width, height, grid);
        var strokes = solver.CountStrokes();
        Console.WriteLine(strokes);
    }
}

class MondrianSolver
{
    private readonly int _width;
    private readonly int _height;
    private readonly string[] _grid;
    public MondrianSolver(int width, int height, string[] grid)
    {
        _width = width;
        _height = height;
        _grid = grid;
    }
    public int CountStrokes()
    {
        var horizontalStrokes = CountHorizontalStrokes();
        var verticalStrokes = CountVerticalStrokes();
        return horizontalStrokes + verticalStrokes;
    }
    private int CountHorizontalStrokes()
    {
        var count = 0;
        for (var row = 0; row < _height - 1; row++)
        {
            for (var col = 0; col < _width; col++)
            {
                if (_grid[row][col] != _grid[row + 1][col])
                {
                    if (col == 0 || _grid[row][col - 1] == _grid[row + 1][col - 1])
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }
    private int CountVerticalStrokes()
    {
        var count = 0;
        for (var col = 0; col < _width - 1; col++)
        {
            for (var row = 0; row < _height; row++)
            {
                if (_grid[row][col] != _grid[row][col + 1])
                {
                    if (row == 0 || _grid[row - 1][col] == _grid[row - 1][col + 1])
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }
}