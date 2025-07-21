using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var init = Console.ReadLine().Split();
        int startX = int.Parse(init[0]);
        int startY = int.Parse(init[1]);
        var thor = new Thor(startX, startY);

        while (true)
        {
            var parts = Console.ReadLine().Split();
            int hammerLeft = int.Parse(parts[0]);
            int n = int.Parse(parts[1]);
            var giants = new List<Point2D>(n);
            for (int i = 0; i < n; i++)
            {
                parts = Console.ReadLine().Split();
                giants.Add(new Point2D(int.Parse(parts[0]), int.Parse(parts[1])));
            }

            thor.Scan(hammerLeft, giants);
            string action = thor.Move();
            Console.WriteLine(action);
        }
    }
}

public struct Point2D
{
    public int X { get; }
    public int Y { get; }
    public Point2D(int x, int y) { X = x; Y = y; }
    public Point2D Offset(int dx, int dy) => new Point2D(X + dx, Y + dy);
}

public struct Move
{
    public string Dir { get; }
    public int Dx { get; }
    public int Dy { get; }
    public Move(string dir, int dx, int dy) { Dir = dir; Dx = dx; Dy = dy; }
}

public class Thor
{
    private const int Wide = 4;
    private const int MaxX = 39;
    private const int MaxY = 17;
    private const string STRIKE = "STRIKE";
    private const string WAIT = "WAIT";
    private Point2D _position;
    private int _hammerLeft;
    private readonly List<Point2D> _giants = new List<Point2D>();

    private static readonly Move[] Moves = new[]
    {
        new Move("N",  0, -1), new Move("NE", 1, -1),
        new Move("E",  1,  0), new Move("SE", 1,  1),
        new Move("S",  0,  1), new Move("SW", -1, 1),
        new Move("W", -1,  0), new Move("NW", -1, -1)
    };

    public Thor(int startX, int startY)
    {
        _position = new Point2D(startX, startY);
    }

    public void Scan(int hammerLeft, List<Point2D> giants)
    {
        _hammerLeft = hammerLeft;
        _giants.Clear();
        _giants.AddRange(giants);
    }

    private int CountGiants(Point2D p)
        => _giants.Count(g => Math.Abs(g.X - p.X) <= Wide
                          && Math.Abs(g.Y - p.Y) <= Wide);

    private bool AnyAdjacent(Point2D p)
        => _giants.Any(g => Math.Abs(g.X - p.X) <= 1
                        && Math.Abs(g.Y - p.Y) <= 1);

    private Point2D ComputeCentroid()
    {
        int cx = _giants.Sum(g => g.X) / _giants.Count;
        int cy = _giants.Sum(g => g.Y) / _giants.Count;
        return new Point2D(cx, cy);
    }

    private bool IsOnMap(Point2D p)
        => p.X >= 0 && p.X <= MaxX
        && p.Y >= 0 && p.Y <= MaxY;

    private static string Direction(int dx, int dy)
    {
        if (dy < 0) return dx < 0 ? "NW" : dx > 0 ? "NE" : "N";
        if (dy > 0) return dx < 0 ? "SW" : dx > 0 ? "SE" : "S";
        return dx < 0 ? "W" : dx > 0 ? "E" : WAIT;
    }

    public string Move()
    {
        string action;
        int totalGiants = _giants.Count;
        int currentKills = CountGiants(_position);

        if (currentKills == totalGiants && _hammerLeft > 0)
        {
            action = STRIKE;
            _hammerLeft--;
        }
        else
        {
            var safeMoves = Moves
                .Select(m => new { m.Dir, Pos = _position.Offset(m.Dx, m.Dy) })
                .Where(o => IsOnMap(o.Pos) && !AnyAdjacent(o.Pos))
                .ToList();

            if (AnyAdjacent(_position))
            {
                if (!safeMoves.Any() && _hammerLeft > 0)
                {
                    action = STRIKE;
                    _hammerLeft--;
                }
                else
                {
                    var center = ComputeCentroid();
                    var best = safeMoves
                        .Select(o => new
                        {
                            o.Dir,
                            o.Pos,
                            Kills = CountGiants(o.Pos),
                            Dist = Math.Abs(o.Pos.X - center.X) + Math.Abs(o.Pos.Y - center.Y)
                        })
                        .OrderByDescending(x => x.Kills)
                        .ThenByDescending(x => x.Dist)
                        .First();
                    action = best.Dir;
                    _position = best.Pos;
                }
            }
            else
            {
                var center = ComputeCentroid();
                int dx = Math.Sign(center.X - _position.X);
                int dy = Math.Sign(center.Y - _position.Y);
                action = Direction(dx, dy);
                _position = _position.Offset(dx, dy);
            }
        }

        return action;
    }
}
