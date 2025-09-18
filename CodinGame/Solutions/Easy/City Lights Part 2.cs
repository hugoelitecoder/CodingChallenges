using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    static void Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var l = int.Parse(Console.ReadLine());
        var w = int.Parse(Console.ReadLine());
        var d = int.Parse(Console.ReadLine());
        var n = int.Parse(Console.ReadLine());
        
        var rawGridLines = new List<string>(n);
        for (var i = 0; i < n; i++)
        {
            rawGridLines.Add(Console.ReadLine());
        }

        Console.Error.WriteLine($"[DEBUG] Input: l={l}, w={w}, d={d}");

        var simulator = new BobvilleSimulator(l, w, d, rawGridLines);
        var resultGrid = simulator.CalculateBrightness();

        PrintGrid(l, w, d, resultGrid);
        
        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Execution time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private static void PrintGrid(int l, int w, int d, char[,,] grid)
    {
        var sb = new StringBuilder(l);
        for (var z = 0; z < d; z++)
        {
            if (z > 0)
            {
                Console.WriteLine();
            }
            for (var y = 0; y < w; y++)
            {
                sb.Clear();
                for (var x = 0; x < l; x++)
                {
                    sb.Append(grid[z, y, x]);
                }
                Console.WriteLine(sb.ToString());
            }
        }
    }
}

class BobvilleSimulator
{
    private readonly int _l;
    private readonly int _w;
    private readonly int _d;
    private readonly List<LightSource> _sources;

    private record LightSource(int X, int Y, int Z, int Radius);

    public BobvilleSimulator(int l, int w, int d, List<string> rawGridLines)
    {
        _l = l;
        _w = w;
        _d = d;
        _sources = new List<LightSource>();
        ParseGrid(rawGridLines);
    }

    public char[,,] CalculateBrightness()
    {
        var brightnessGrid = new int[_d, _w, _l];
        
        foreach (var source in _sources)
        {
            var r = source.Radius;
            var rBound = r - 1;

            var sx = source.X; 
            var sy = source.Y; 
            var sz = source.Z;
            
            var txStart = Math.Max(0, sx - rBound);
            var txEnd = Math.Min(_l - 1, sx + rBound);
            var tyStart = Math.Max(0, sy - rBound);
            var tyEnd = Math.Min(_w - 1, sy + rBound);
            var tzStart = Math.Max(0, sz - rBound);
            var tzEnd = Math.Min(_d - 1, sz + rBound);

            for (var tz = tzStart; tz <= tzEnd; tz++)
            {
                var dz = tz - sz;
                for (var ty = tyStart; ty <= tyEnd; ty++)
                {
                    var dy = ty - sy;
                    for (var tx = txStart; tx <= txEnd; tx++)
                    {
                        var dx = tx - sx;
                        
                        var distSq = dx * dx + dy * dy + dz * dz;
                        var dist = (int)Math.Round(Math.Sqrt(distSq));
                        
                        var brightness = r - dist;
                        if (brightness > 0)
                        {
                            brightnessGrid[tz, ty, tx] += brightness;
                        }
                    }
                }
            }
        }
        
        var charGrid = new char[_d, _w, _l];
        for (var z = 0; z < _d; z++)
        {
            for (var y = 0; y < _w; y++)
            {
                for (var x = 0; x < _l; x++)
                {
                    charGrid[z, y, x] = GetCharFromBrightness(brightnessGrid[z, y, x]);
                }
            }
        }
        return charGrid;
    }

    private void ParseGrid(List<string> rawGridLines)
    {
        var lineIdx = 0;
        for (var z = 0; z < _d; z++)
        {
            for (var y = 0; y < _w; y++)
            {
                var line = rawGridLines[lineIdx++];
                for (var x = 0; x < _l; x++)
                {
                    var c = line[x];
                    var radius = GetRadiusFromChar(c);
                    if (radius > 0)
                    {
                        _sources.Add(new LightSource(x, y, z, radius));
                    }
                }
            }
            if (z < _d - 1)
            {
                lineIdx++; // Skip blank line
            }
        }
        Console.Error.WriteLine($"[DEBUG] Found {_sources.Count} light sources.");
    }

    private static int GetRadiusFromChar(char c)
    {
        if (c >= '1' && c <= '9') return c - '0';
        if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
        return 0;
    }

    private static char GetCharFromBrightness(int b)
    {
        if (b <= 9) return (char)('0' + b);
        if (b <= 35) return (char)('A' + b - 10);
        return 'Z';
    }
}