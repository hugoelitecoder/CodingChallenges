using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        var h = int.Parse(Console.ReadLine());
        var gridLines = new string[h];
        for (var i = 0; i < h; i++)
        {
            gridLines[i] = Console.ReadLine();
        }

        Console.Error.WriteLine($"[DEBUG] Grid dimensions: {h}x{(h > 0 ? gridLines[0].Split(' ').Length : 0)}");
        
        var sw = Stopwatch.StartNew();

        var finder = new PeakAndValleyFinder(gridLines);
        finder.FindFeatures();
        
        sw.Stop();
        
        var peaksOutput = FormatCoordinates(finder.Peaks);
        var valleysOutput = FormatCoordinates(finder.Valleys);
        
        Console.WriteLine(peaksOutput);
        Console.WriteLine(valleysOutput);

        Console.Error.WriteLine($"[DEBUG] Peaks found: {finder.Peaks.Count}");
        Console.Error.WriteLine($"[DEBUG] Valleys found: {finder.Valleys.Count}");
        Console.Error.WriteLine($"[DEBUG] Calculation time: {sw.Elapsed.TotalMilliseconds}ms");
    }

    private static string FormatCoordinates(List<(int x, int y)> coords)
    {
        if (coords.Count == 0)
        {
            return "NONE";
        }
        return string.Join(", ", coords.Select(c => $"({c.x}, {c.y})"));
    }
}

public class PeakAndValleyFinder
{
    private readonly int[,] _grid;
    private readonly int _height;
    private readonly int _width;
    private static readonly (int dy, int dx)[] _directions = 
    {
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1),           (0, 1),
        (1, -1), (1, 0), (1, 1)
    };

    public List<(int x, int y)> Peaks { get; private set; }
    public List<(int x, int y)> Valleys { get; private set; }

    public PeakAndValleyFinder(string[] gridLines)
    {
        _height = gridLines.Length;
        _width = _height > 0 ? gridLines[0].Split(' ').Length : 0;
        _grid = new int[_height, _width];
        Peaks = new List<(int x, int y)>();
        Valleys = new List<(int x, int y)>();
        ParseGrid(gridLines);
    }
    
    public void FindFeatures()
    {
        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                CheckCell(x, y);
            }
        }
    }

    private void ParseGrid(string[] gridLines)
    {
        for (var y = 0; y < _height; y++)
        {
            var values = gridLines[y].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            for (var x = 0; x < _width; x++)
            {
                _grid[y, x] = values[x];
            }
        }
    }

    private void CheckCell(int x, int y)
    {
        var isPeak = true;
        var isValley = true;
        var currentVal = _grid[y, x];

        foreach (var (dy, dx) in _directions)
        {
            var nx = x + dx;
            var ny = y + dy;

            if (ny >= 0 && ny < _height && nx >= 0 && nx < _width)
            {
                var neighborVal = _grid[ny, nx];
                if (currentVal <= neighborVal)
                {
                    isPeak = false;
                }
                if (currentVal >= neighborVal)
                {
                    isValley = false;
                }
                if (!isPeak && !isValley)
                {
                    break;
                }
            }
        }
        if (isPeak) Peaks.Add((x, y));
        if (isValley) Valleys.Add((x, y));
    }
}