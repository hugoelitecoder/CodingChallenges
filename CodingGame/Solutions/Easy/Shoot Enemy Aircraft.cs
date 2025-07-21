using System;
using System.Collections.Generic;

class Plane
{
    public int Row { get; }
    public int Col { get; }
    public int Dir { get; }

    public Plane(int row, int col, int dir)
    {
        Row = row;
        Col = col;
        Dir = dir;
    }

    public int? GetShootTurn(int launcherCol, int groundRow)
    {
        int dx = launcherCol - Col;
        if (dx * Dir < 0 || dx % Dir != 0) return null;
        int alignTime = dx / Dir;
        int fireTime = alignTime - (groundRow - Row) - 1;
        return fireTime >= 0 ? fireTime : null;
    }

    public (int r, int c)? GetPositionAt(int time)
    {
        int newCol = Col + Dir * time;
        return (newCol < 0 || newCol >= 1000) ? null : (Row, newCol);
    }
}

class Battlefield
{
    private readonly int _height;
    private readonly int _width;
    private readonly int _launcherCol;
    private readonly List<Plane> _planes = new();
    private readonly SortedSet<int> _shootTurns = new();

    public Battlefield(List<string> map)
    {
        _height = map.Count - 1;
        _width = map[0].Length;
        _launcherCol = map[^1].IndexOf('^');
        LoadPlanes(map);
    }

    private void LoadPlanes(List<string> map)
    {
        for (int r = 0; r < _height; r++)
        {
            for (int c = 0; c < _width; c++)
            {
                char ch = map[r][c];
                if (ch != '<' && ch != '>') continue;
                int dir = ch == '>' ? 1 : -1;
                var plane = new Plane(r, c, dir);
                var shootTurn = plane.GetShootTurn(_launcherCol, _height);
                if (shootTurn.HasValue)
                    _shootTurns.Add(shootTurn.Value);
                _planes.Add(plane);
            }
        }
    }

    public void Run()
    {
        int maxTurn = _shootTurns.Count > 0 ? Math.Max(0, _shootTurns.Max) : 0;
        for (int t = 0; t <= maxTurn; t++)
            Console.WriteLine(_shootTurns.Contains(t) ? "SHOOT" : "WAIT");
    }
}

class Solution
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        var lines = new List<string>();
        for (int i = 0; i < n; i++) lines.Add(Console.ReadLine());

        var battlefield = new Battlefield(lines);
        battlefield.Run();
    }
}
