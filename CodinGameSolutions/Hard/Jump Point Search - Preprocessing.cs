using System;
using System.Collections.Generic;
using System.Text;

public class Solution
{
    public static void Main(string[] args)
    {
        var wh = Console.ReadLine().Split(' ');
        var w = int.Parse(wh[0]);
        var h = int.Parse(wh[1]);
        var map = new char[h, w];
        for (var y = 0; y < h; y++)
        {
            var row = Console.ReadLine();
            for (var x = 0; x < w; x++)
                map[y, x] = row[x];
        }
        var jp = new JPSGrid(w, h, map);
        var outLines = jp.ComputeJPSPlus();
        foreach (var s in outLines)
            Console.WriteLine(s);
    }
}

class JPSGrid
{
    enum Dir { N = 0, NE = 1, E = 2, SE = 3, S = 4, SW = 5, W = 6, NW = 7 }
    private readonly int _w;
    private readonly int _h;
    private readonly char[,] _map;
    private readonly bool[,,] _jp;
    private readonly int[,,] _dist;
    private static readonly (int dy, int dx)[] Dirs = {
        (-1,0), (-1,1), (0,1), (1,1), (1,0), (1,-1), (0,-1), (-1,-1)
    };
    private static readonly Dir[] Cardinals = { Dir.N, Dir.E, Dir.S, Dir.W };
    private static readonly Dir[] Diagonals = { Dir.NE, Dir.SE, Dir.SW, Dir.NW };

    public JPSGrid(int w, int h, char[,] map)
    {
        _w = w;
        _h = h;
        _map = map;
        _jp = new bool[h, w, 8];
        _dist = new int[h, w, 8];
    }

    public List<string> ComputeJPSPlus()
    {
        MarkJumpPoints();
        ComputeCardinalDistances();
        ComputeDiagonalDistances();
        return FormatOutput();
    }

    private void MarkJumpPoints()
    {
        for (var y = 0; y < _h; y++)
        for (var x = 0; x < _w; x++)
        {
            if (_map[y, x] == '#') continue;
            foreach (var d in Cardinals)
                _jp[y, x, (int)d] = HasForcedNeighbor(y, x, d);
        }
    }

    private bool HasForcedNeighbor(int y, int x, Dir d)
    {
        return d switch
        {
            Dir.W =>  x > 0 && _map[y, x - 1] == '.' &&
                        ((y > 0 && _map[y - 1, x] == '.' && _map[y - 1, x - 1] == '#') ||
                         (y < _h - 1 && _map[y + 1, x] == '.' && _map[y + 1, x - 1] == '#')),
            Dir.E =>  x < _w - 1 && _map[y, x + 1] == '.' &&
                        ((y > 0 && _map[y - 1, x] == '.' && _map[y - 1, x + 1] == '#') ||
                         (y < _h - 1 && _map[y + 1, x] == '.' && _map[y + 1, x + 1] == '#')),
            Dir.N =>  y > 0 && _map[y - 1, x] == '.' &&
                        ((x > 0 && _map[y, x - 1] == '.' && _map[y - 1, x - 1] == '#') ||
                         (x < _w - 1 && _map[y, x + 1] == '.' && _map[y - 1, x + 1] == '#')),
            Dir.S =>  y < _h - 1 && _map[y + 1, x] == '.' &&
                        ((x > 0 && _map[y, x - 1] == '.' && _map[y + 1, x - 1] == '#') ||
                         (x < _w - 1 && _map[y, x + 1] == '.' && _map[y + 1, x + 1] == '#')),
            _ => false
        };
    }

    private void ComputeCardinalDistances()
    {
        for (var y = 0; y < _h; y++)
        for (var x = 0; x < _w; x++)
        {
            if (_map[y, x] == '#') continue;
            foreach (var d in Cardinals)
            {
                var (dy, dx) = Dirs[(int)d];
                var dist = 0;
                while (true)
                {
                    dist++;
                    var ny = y + dist * dy;
                    var nx = x + dist * dx;
                    if (ny < 0 || ny >= _h || nx < 0 || nx >= _w || _map[ny, nx] == '#')
                    {
                        _dist[y, x, (int)d] = -(dist - 1);
                        break;
                    }
                    var arrival = ((int)d + 4) % 8;
                    if (_jp[ny, nx, arrival])
                    {
                        _dist[y, x, (int)d] = dist;
                        break;
                    }
                }
            }
        }
    }

    private void ComputeDiagonalDistances()
    {
        for (var y = 0; y < _h; y++)
        for (var x = 0; x < _w; x++)
        {
            if (_map[y, x] == '#') continue;
            foreach (var d in Diagonals)
            {
                var (dy, dx) = Dirs[(int)d];
                var dist = 0;
                while (true)
                {
                    dist++;
                    var ny = y + dist * dy;
                    var nx = x + dist * dx;
                    var py = y + (dist - 1) * dy;
                    var px = x + (dist - 1) * dx;
                    if (ny < 0 || ny >= _h || nx < 0 || nx >= _w || _map[ny, nx] == '#' ||
                        _map[py, nx] == '#' || _map[ny, px] == '#')
                    {
                        _dist[y, x, (int)d] = -(dist - 1);
                        break;
                    }
                    var c1 = ((int)d + 7) % 8;
                    var c2 = ((int)d + 1) % 8;
                    if (_dist[ny, nx, c1] > 0 || _dist[ny, nx, c2] > 0)
                    {
                        _dist[y, x, (int)d] = dist;
                        break;
                    }
                }
            }
        }
    }

    private List<string> FormatOutput()
    {
        var res = new List<string>();
        for (var y = 0; y < _h; y++)
        for (var x = 0; x < _w; x++)
        {
            if (_map[y, x] == '#') continue;
            var sb = new StringBuilder();
            sb.Append(x).Append(' ').Append(y);
            for (var d = 0; d < 8; d++)
                sb.Append(' ').Append(_dist[y, x, d]);
            res.Add(sb.ToString());
        }
        return res;
    }
}
