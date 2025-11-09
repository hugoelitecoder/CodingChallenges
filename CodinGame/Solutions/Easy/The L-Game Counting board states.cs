using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    public static void Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();
        
        var inputs = Console.ReadLine().Split(' ');
        var height = int.Parse(inputs[0]);
        var width = int.Parse(inputs[1]);
        var n = int.Parse(inputs[2]);
        
        var game = new LGameConfigurator(height, width, n);
        var result = game.CountConfigurations();
        
        Console.WriteLine(result);
        
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Input: height={height}, width={width}, N={n}");
        Console.Error.WriteLine($"[DEBUG] Total configurations: {result}");
        Console.Error.WriteLine($"[DEBUG] Execution time: {sw.ElapsedMilliseconds} ms");
    }
}

public class LGameConfigurator
{
    private readonly int _height;
    private readonly int _width;
    private readonly int _n;
    private readonly int _totalCells;
    
    private static readonly List<List<(int r, int c)>> _lShapes = new List<List<(int r, int c)>>
    {
        new List<(int, int)> { (0, 0), (1, 0), (2, 0), (2, 1) },
        new List<(int, int)> { (0, 0), (0, -1), (0, -2), (1, -2) },
        new List<(int, int)> { (0, 0), (-1, 0), (-2, 0), (-2, -1) },
        new List<(int, int)> { (0, 0), (0, 1), (0, 2), (-1, 2) },
        new List<(int, int)> { (0, 0), (1, 0), (2, 0), (2, -1) },
        new List<(int, int)> { (0, 0), (0, -1), (0, -2), (-1, -2) },
        new List<(int, int)> { (0, 0), (-1, 0), (-2, 0), (-2, 1) },
        new List<(int, int)> { (0, 0), (0, 1), (0, 2), (1, 2) }
    };
    
    public LGameConfigurator(int height, int width, int n)
    {
        _height = height;
        _width = width;
        _n = n;
        _totalCells = height * width;
    }
    
    public ulong CountConfigurations()
    {
        var requiredCells = 8 + _n;
        
        if (_totalCells < requiredCells || _totalCells > 64)
        {
            return 0;
        }
        
        var allPlacements = GetAllPlacements();
        var totalConfigs = 0UL;
        var numPlacements = allPlacements.Count;
        var freeCellsForBlockers = _totalCells - 8;
        var waysToPlaceBlockers = Combinations(freeCellsForBlockers, _n);
        if (waysToPlaceBlockers == 0 && _n > 0)
        {
            return 0;
        }
        
        for (var i = 0; i < numPlacements; i++)
        {
            var redL = allPlacements[i];
            for (var j = 0; j < numPlacements; j++)
            {
                var blueL = allPlacements[j];
                
                if (!DoOverlap(redL, blueL))
                {
                    totalConfigs += waysToPlaceBlockers;
                }
            }
        }
        
        return totalConfigs;
    }
    
    private List<ulong> GetAllPlacements()
    {
        var placements = new List<ulong>();
        foreach (var shape in _lShapes)
        {
            for (var r = 0; r < _height; r++)
            {
                for (var c = 0; c < _width; c++)
                {
                    var currentMask = 0UL;
                    var isValid = true;
                    foreach (var (dr, dc) in shape)
                    {
                        var nr = r + dr;
                        var nc = c + dc;
                        
                        if (nr < 0 || nr >= _height || nc < 0 || nc >= _width)
                        {
                            isValid = false;
                            break;
                        }
                        currentMask |= (1UL << (nr * _width + nc));
                    }
                    
                    if (isValid)
                    {
                        placements.Add(currentMask);
                    }
                }
            }
        }
        return placements;
    }
    
    private static bool DoOverlap(ulong a, ulong b)
    {
        return (a & b) != 0;
    }
    
    private static ulong Combinations(int n, int k)
    {
        if (k < 0 || k > n)
        {
            return 0;
        }
        if (k == 0 || k == n)
        {
            return 1;
        }
        if (k > n / 2)
        {
            k = n - k;
        }
        
        ulong res = 1;
        for (var i = 1; i <= k; i++)
        {
            if (res > ulong.MaxValue / (ulong)(n - i + 1))
            {
                return 0;
            }
            res = res * (ulong)(n - i + 1) / (ulong)i;
        }
        return res;
    }
}