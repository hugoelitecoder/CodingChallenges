using System;
using System.Collections.Generic;
using System.Diagnostics;
class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var nStr = Console.ReadLine();
        if (nStr == null) return;
        var n = int.Parse(nStr);
        var sP = Console.ReadLine().Split(' ');
        var sR = int.Parse(sP[0]) - 1;
        var sC = int.Parse(sP[1]) - 1;
        var gP = Console.ReadLine().Split(' ');
        var gR = int.Parse(gP[0]) - 1;
        var gC = int.Parse(gP[1]) - 1;
        var grid = new string[n];
        for (var i = 0; i < n; i++)
        {
            grid[i] = Console.ReadLine();
        }
        var finder = new PalindromicPathFinder(n, grid);
        var result = finder.FindShortestPath(sR, sC, gR, gC);
        Console.WriteLine(result);
        sw.Stop();
        Console.Error.WriteLine($"[DEBUG] Grid Size: {n}x{n}");
        Console.Error.WriteLine($"[DEBUG] Result: {result}");
        Console.Error.WriteLine($"[DEBUG] Execution: {sw.ElapsedMilliseconds}ms");
    }
}

public class PalindromicPathFinder
{
    private readonly int _n;
    private readonly string[] _grid;
    private readonly int[] _dr = { -1, 1, 0, 0 };
    private readonly int[] _dc = { 0, 0, -1, 1 };
    public PalindromicPathFinder(int n, string[] grid)
    {
        _n = n;
        _grid = grid;
    }
    public int FindShortestPath(int sR, int sC, int gR, int gC)
    {
        if (_grid[sR][sC] != _grid[gR][gC]) return -1;
        var visited = new bool[_n * _n * _n * _n];
        var q = new Queue<int>();
        var startState = Pack(sR, sC, gR, gC);
        q.Enqueue(startState);
        visited[startState] = true;
        var distance = 2;
        while (q.Count > 0)
        {
            var count = q.Count;
            var currentLevel = new int[count];
            for (var i = 0; i < count; i++)
            {
                currentLevel[i] = q.Dequeue();
            }
            for (var i = 0; i < count; i++)
            {
                Unpack(currentLevel[i], out var r1, out var c1, out var r2, out var c2);
                if (r1 == r2 && c1 == c2) return distance - 1;
            }
            for (var i = 0; i < count; i++)
            {
                Unpack(currentLevel[i], out var r1, out var c1, out var r2, out var c2);
                if (Math.Abs(r1 - r2) + Math.Abs(c1 - c2) == 1) return distance;
            }
            for (var i = 0; i < count; i++)
            {
                Unpack(currentLevel[i], out var r1, out var c1, out var r2, out var c2);
                for (var d1 = 0; d1 < 4; d1++)
                {
                    var nr1 = r1 + _dr[d1];
                    var nc1 = c1 + _dc[d1];
                    if (nr1 < 0 || nr1 >= _n || nc1 < 0 || nc1 >= _n) continue;
                    for (var d2 = 0; d2 < 4; d2++)
                    {
                        var nr2 = r2 + _dr[d2];
                        var nc2 = c2 + _dc[d2];
                        if (nr2 < 0 || nr2 >= _n || nc2 < 0 || nc2 >= _n) continue;
                        if (_grid[nr1][nc1] == _grid[nr2][nc2])
                        {
                            var state = Pack(nr1, nc1, nr2, nc2);
                            if (!visited[state])
                            {
                                visited[state] = true;
                                q.Enqueue(state);
                            }
                        }
                    }
                }
            }
            distance += 2;
        }
        return -1;
    }
    private int Pack(int r1, int c1, int r2, int c2)
    {
        return (((r1 * _n + c1) * _n + r2) * _n + c2);
    }
    private void Unpack(int state, out int r1, out int c1, out int r2, out int c2)
    {
        c2 = state % _n;
        state /= _n;
        r2 = state % _n;
        state /= _n;
        c1 = state % _n;
        r1 = state / _n;
    }
}