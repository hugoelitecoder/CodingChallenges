using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var line1 = Console.ReadLine();
        if (line1 == null) return;
        var dims = line1.Split(' ');
        var h = int.Parse(dims[0]);
        var w = int.Parse(dims[1]);
        var grid = new string[h];
        for (var i = 0; i < h; i++)
        {
            grid[i] = Console.ReadLine();
        }
        var analyzer = new SnowflakeAnalyzer(h, w, grid);
        var total = analyzer.TotalSnowflakes;
        var unique = analyzer.UniqueShapesCount;
        Console.WriteLine(total);
        Console.WriteLine(unique);
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Photo Size: {h}x{w}");
        Console.Error.WriteLine($"[DEBUG] Total: {total}, Unique: {unique}");
        Console.Error.WriteLine($"[DEBUG] Time: {sw.ElapsedMilliseconds}ms");
    }
}

public class SnowflakeAnalyzer
{
    private readonly int _h;
    private readonly int _w;
    private readonly string[] _grid;
    private readonly bool[,] _visited;
    private readonly List<List<(int r, int c)>> _snowflakes = new List<List<(int r, int c)>>();
    public int TotalSnowflakes => _snowflakes.Count;
    public int UniqueShapesCount { get; private set; }
    public SnowflakeAnalyzer(int h, int w, string[] grid)
    {
        _h = h;
        _w = w;
        _grid = grid;
        _visited = new bool[h, w];
        ExtractSnowflakes();
        CalculateUniqueShapes();
    }
    private void ExtractSnowflakes()
    {
        for (var r = 0; r < _h; r++)
        {
            for (var c = 0; c < _w; c++)
            {
                if (_grid[r][c] == '*' && !_visited[r, c])
                {
                    var points = new List<(int r, int c)>();
                    DFS(r, c, points);
                    _snowflakes.Add(points);
                }
            }
        }
    }
    private void DFS(int r, int c, List<(int r, int c)> points)
    {
        _visited[r, c] = true;
        points.Add((r, c));
        var dr = new int[] { -1, 1, 0, 0 };
        var dc = new int[] { 0, 0, -1, 1 };
        for (var i = 0; i < 4; i++)
        {
            var nr = r + dr[i];
            var nc = c + dc[i];
            if (nr >= 0 && nr < _h && nc >= 0 && nc < _w && _grid[nr][nc] == '*' && !_visited[nr, nc])
            {
                DFS(nr, nc, points);
            }
        }
    }
    private void CalculateUniqueShapes()
    {
        var uniqueShapes = new HashSet<string>();
        foreach (var snowflake in _snowflakes)
        {
            uniqueShapes.Add(GetCanonicalForm(snowflake));
        }
        UniqueShapesCount = uniqueShapes.Count;
    }
    private string GetCanonicalForm(List<(int r, int c)> points)
    {
        var variants = new List<string>();
        var current = points;
        for (var i = 0; i < 4; i++)
        {
            variants.Add(Normalize(current));
            variants.Add(Normalize(Mirror(current)));
            current = Rotate90(current);
        }
        variants.Sort(StringComparer.Ordinal);
        return variants[0];
    }
    private List<(int r, int c)> Rotate90(List<(int r, int c)> points)
    {
        return points.Select(p => (p.c, -p.r)).ToList();
    }
    private List<(int r, int c)> Mirror(List<(int r, int c)> points)
    {
        return points.Select(p => (p.r, -p.c)).ToList();
    }
    private string Normalize(List<(int r, int c)> points)
    {
        var minR = points.Min(p => p.r);
        var minC = points.Min(p => p.c);
        var normalized = points.Select(p => (r: p.r - minR, c: p.c - minC)).OrderBy(p => p.r).ThenBy(p => p.c).ToList();
        return string.Join(";", normalized.Select(p => $"{p.r},{p.c}"));
    }
}