using System;
using System.Diagnostics;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var dimensionsLine = Console.ReadLine();
        var dimensions = dimensionsLine.Split(' ').Select(int.Parse).ToArray();
        var r = dimensions[0];
        var c = dimensions[1];
        var grid = new char[r][];
        for (var i = 0; i < r; i++)
        {
            grid[i] = Console.ReadLine().ToCharArray();
        }
        Console.Error.WriteLine($"[DEBUG] Grid: {r}x{c}");
        var finder = new RectangleFinder(r, c, grid);
        var stopwatch = Stopwatch.StartNew();
        var result = finder.CountRectangles();
        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Result: {result}");
        Console.Error.WriteLine($"[DEBUG] Execution time: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine(result);
    }
}

public class RectangleFinder
{
    private readonly int _r;
    private readonly int _c;
    private readonly char[][] _grid;
    private readonly int[][] _rightDash;
    private readonly int[][] _downPipe;

    public RectangleFinder(int r, int c, char[][] grid)
    {
        _r = r;
        _c = c;
        _grid = grid;
        _rightDash = new int[r][];
        _downPipe = new int[r][];
        for (var i = 0; i < r; i++)
        {
            _rightDash[i] = new int[c];
            _downPipe[i] = new int[c];
        }
    }

    public int CountRectangles()
    {
        PrecomputeDP();
        var count = 0;
        for (var i = 0; i < _r; i++)
        {
            for (var j = 0; j < _c; j++)
            {
                if (_grid[i][j] != '+') continue;
                for (var k = 0; ; k++)
                {
                    var h = k + 1;
                    var w = 2 * k + 2;
                    if (i + h >= _r || j + w >= _c)
                    {
                        break;
                    }
                    if (_grid[i + h][j] != '+' || _grid[i][j + w] != '+' || _grid[i + h][j + w] != '+')
                    {
                        continue;
                    }
                    if (_rightDash[i][j] >= w + 1 &&
                        _rightDash[i + h][j] >= w + 1 &&
                        _downPipe[i][j] >= h + 1 &&
                        _downPipe[i][j + w] >= h + 1)
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    private void PrecomputeDP()
    {
        for (var i = _r - 1; i >= 0; i--)
        {
            for (var j = _c - 1; j >= 0; j--)
            {
                if (_grid[i][j] == '-' || _grid[i][j] == '+')
                {
                    _rightDash[i][j] = 1 + (j + 1 < _c ? _rightDash[i][j + 1] : 0);
                }
                else
                {
                    _rightDash[i][j] = 0;
                }
                if (_grid[i][j] == '|' || _grid[i][j] == '+')
                {
                    _downPipe[i][j] = 1 + (i + 1 < _r ? _downPipe[i + 1][j] : 0);
                }
                else
                {
                    _downPipe[i][j] = 0;
                }
            }
        }
    }
}

