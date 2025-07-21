using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var nd = (n + 1) * (n + 2) / 2;
        var dims = Console.ReadLine().Split();
        var h = int.Parse(dims[0]);
        var w = int.Parse(dims[1]);
        var grid = new char[h][];
        for (var y = 0; y < h; y++) grid[y] = Console.ReadLine().ToCharArray();
        var domino = new DominoGrid(n, h, w, grid);
        domino.Solve();
        for (var y = 0; y < h; y++) Console.WriteLine(new string(domino.Grid[y]));
    }
}

class DominoGrid
{
    public char[][] Grid { get; }
    readonly int _n, _h, _w, _nd;
    readonly bool[,] _remaining;
    List<string>[,] _possible;
    int[,] _possibleCount;
    List<int[]>[,] _positions;
    static readonly int[] _dx = { 1, 0, 0, -1 };
    static readonly int[] _dy = { 0, 1, -1, 0 };

    public DominoGrid(int n, int h, int w, char[][] grid)
    {
        _n = n;
        _h = h;
        _w = w;
        _nd = (n + 1) * (n + 2) / 2;
        Grid = grid;
        _remaining = new bool[n + 1, n + 1];
        for (var a = 0; a <= n; a++)
            for (var b = 0; b <= n; b++)
                _remaining[a, b] = a <= b;
        UpdateDominoState();
    }

    public void Solve()
    {
        for (var step = 0; step < _nd; step++)
        {
            var found = false;
            for (var a = 0; a <= _n && !found; a++)
                for (var b = a; b <= _n && !found; b++)
                    if (_possibleCount[a, b] == 2)
                    {
                        var d1 = _positions[a, b][0];
                        var d2 = _positions[a, b][1];
                        var c = (d1[0] == d2[0]) ? '=' : '|';
                        Grid[d1[0]][d1[1]] = c;
                        Grid[d2[0]][d2[1]] = c;
                        _remaining[a, b] = false;
                        UpdateDominoState();
                        found = true;
                    }
            if (found) continue;
            for (var y = 0; y < _h && !found; y++)
                for (var x = 0; x < _w && !found; x++)
                    if (Grid[y][x] != '=' && Grid[y][x] != '|' && _possible[y, x]?.Count == 1)
                    {
                        var domino = _possible[y, x][0];
                        var a = domino[0] - '0';
                        var b = domino[1] - '0';
                        var mate = Grid[y][x] == domino[0] ? domino[1] : domino[0];
                        int yy = -1, xx = -1;
                        for (var d = 0; d < 4; d++)
                        {
                            var ny = y + _dy[d];
                            var nx = x + _dx[d];
                            if (ny >= 0 && ny < _h && nx >= 0 && nx < _w && Grid[ny][nx] == mate)
                            {
                                yy = ny;
                                xx = nx;
                                break;
                            }
                        }
                        var c = (y == yy) ? '=' : '|';
                        Grid[y][x] = c;
                        Grid[yy][xx] = c;
                        _remaining[a, b] = false;
                        UpdateDominoState();
                        found = true;
                    }
        }
    }

    void UpdateDominoState()
    {
        _possible = new List<string>[_h, _w];
        _possibleCount = new int[_n + 1, _n + 1];
        _positions = new List<int[]>[_n + 1, _n + 1];
        for (var a = 0; a <= _n; a++)
            for (var b = 0; b <= _n; b++)
                _positions[a, b] = new List<int[]>();
        for (var y = 0; y < _h; y++)
            for (var x = 0; x < _w; x++)
            {
                _possible[y, x] = GetDominoList(y, x);
                if (_possible[y, x] != null)
                    foreach (var dom in _possible[y, x])
                    {
                        var a = dom[0] - '0';
                        var b = dom[1] - '0';
                        _possibleCount[a, b]++;
                        _positions[a, b].Add(new[] { y, x });
                    }
            }
    }

    List<string> GetDominoList(int y, int x)
    {
        var li = new List<string>();
        var d1 = Grid[y][x];
        if (d1 == '|' || d1 == '=') return null;
        for (var d = 0; d < 4; d++)
        {
            var ny = y + _dy[d];
            var nx = x + _dx[d];
            if (ny >= 0 && ny < _h && nx >= 0 && nx < _w)
            {
                var d2 = Grid[ny][nx];
                if (d2 != '|' && d2 != '=')
                {
                    var a = Math.Min(d1 - '0', d2 - '0');
                    var b = Math.Max(d1 - '0', d2 - '0');
                    if (_remaining[a, b])
                        li.Add($"{a}{b}");
                }
            }
        }
        return li;
    }
}
