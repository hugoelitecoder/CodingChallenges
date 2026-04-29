using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        var first = Console.ReadLine().Split(' ');
        var w = int.Parse(first[0]);
        var h = int.Parse(first[1]);
        var grid = new string[h];
        for (var y = 0; y < h; y++)
        {
            grid[y] = Console.ReadLine();
            Console.Error.WriteLine("[DEBUG] Row " + y + ": " + grid[y]);
        }
        Console.Error.WriteLine("[DEBUG] Input W=" + w + " H=" + h);
        var counter = new HoleCounter(grid);
        var ans = counter.CountHoles();
        sw.Stop();
        Console.Error.WriteLine("[DEBUG] Holes=" + ans);
        Console.Error.WriteLine("[DEBUG] ElapsedMs=" + sw.ElapsedMilliseconds);
        Console.WriteLine(ans);
    }
}

class HoleCounter
{
    private readonly string[] _grid;
    private readonly int _h;
    private readonly int _w;
    private readonly bool[] _seen;
    private readonly int[] _qx;
    private readonly int[] _qy;

    public HoleCounter(string[] grid)
    {
        _grid = grid;
        _h = grid.Length;
        _w = grid[0].Length;
        _seen = new bool[_w * _h];
        _qx = new int[_w * _h];
        _qy = new int[_w * _h];
    }

    public int CountHoles()
    {
        var holes = 0;
        for (var y = 0; y < _h; y++)
        {
            for (var x = 0; x < _w; x++)
            {
                if (_grid[y][x] != '.') continue;
                var id = GetId(x, y);
                if (_seen[id]) continue;
                if (IsHole(x, y)) holes++;
            }
        }
        return holes;
    }

    private bool IsHole(int sx, int sy)
    {
        var head = 0;
        var tail = 0;
        var touchesBorder = false;
        var startId = GetId(sx, sy);
        _seen[startId] = true;
        _qx[tail] = sx;
        _qy[tail] = sy;
        tail++;
        while (head < tail)
        {
            var x = _qx[head];
            var y = _qy[head];
            head++;
            if (x == 0 || y == 0 || x == _w - 1 || y == _h - 1) touchesBorder = true;
            TryPush(x - 1, y, ref tail);
            TryPush(x + 1, y, ref tail);
            TryPush(x, y - 1, ref tail);
            TryPush(x, y + 1, ref tail);
        }
        return !touchesBorder;
    }

    private void TryPush(int x, int y, ref int tail)
    {
        if ((uint)x >= (uint)_w || (uint)y >= (uint)_h) return;
        if (_grid[y][x] != '.') return;
        var id = GetId(x, y);
        if (_seen[id]) return;
        _seen[id] = true;
        _qx[tail] = x;
        _qy[tail] = y;
        tail++;
    }

    private int GetId(int x, int y)
    {
        return y * _w + x;
    }
}