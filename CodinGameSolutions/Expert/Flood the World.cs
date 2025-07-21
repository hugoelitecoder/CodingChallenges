using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var surfacePointCount = int.Parse(Console.ReadLine());
        var mapPoints = new List<Point2D>(surfacePointCount);
        for (var i = 0; i < surfacePointCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            mapPoints.Add(new Point2D(x, y));
        }

        var portalX = int.Parse(Console.ReadLine());
        var townCount = int.Parse(Console.ReadLine());
        var towns = new List<Town>(townCount);
        for (var i = 0; i < townCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var townX = int.Parse(inputs[0]);
            var townName = inputs[1];
            towns.Add(new Town(townX, townName));
        }

        var terrain = new Terrain(mapPoints);
        var simulator = new FloodSimulator(terrain, towns, portalX);

        var winningTownName = simulator.FindLastSurvivingTown();
        Console.WriteLine(winningTownName);
    }
}

public class FloodSimulator
{
    private readonly List<Town> _towns;
    private readonly FloodBasin _basin;

    public FloodSimulator(Terrain terrain, List<Town> towns, int portalX)
    {
        _towns = towns;
        foreach (var town in _towns)
        {
            town.Initialize(terrain);
        }

        var initialLeftDamX = terrain.Points.Last(p => p.X <= portalX).X;
        var initialRightDamX = terrain.Points.First(p => p.X >= portalX).X;
        _basin = new FloodBasin(initialLeftDamX, initialRightDamX, terrain);
    }

    public string FindLastSurvivingTown()
    {
        while (_towns.Count(t => !t.IsFlooded) > 1)
        {
            var nextTownToFlood = FindNextTownInBasin();
            if (nextTownToFlood != null)
            {
                nextTownToFlood.IsFlooded = true;
            }
            else
            {
                _basin.Expand();
            }
        }
        return _towns.Single(t => !t.IsFlooded).Name;
    }

    private Town FindNextTownInBasin()
    {
        return _towns
            .Where(t => !t.IsFlooded && _basin.Contains(t) && t.Y < _basin.WaterLevel)
            .OrderBy(t => t.Elevation)
            .FirstOrDefault();
    }
}

public class Point2D
{
    public int X { get; }
    public double Y { get; }

    public Point2D(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Terrain
{
    public IReadOnlyList<Point2D> Points { get; }
    private readonly int _minX;
    private readonly int _maxX;

    public Terrain(List<Point2D> mapPoints)
    {
        Points = mapPoints.OrderBy(p => p.X).ToList();
        _minX = Points.First().X;
        _maxX = Points.Last().X;
    }

    public double GetYAtX(int x)
    {
        var p1 = Points.Last(p => p.X <= x);
        var p2 = Points.First(p => p.X >= x);

        if (p1 == p2)
        {
            if (p1.X == _minX || p1.X == _maxX)
            {
                return double.PositiveInfinity;
            }
            return p1.Y;
        }

        var m = (p1.Y - p2.Y) / (double)(p1.X - p2.X);
        var b = p1.Y - m * p1.X;
        return m * x + b;
    }

    public double GetLocalSpilloverY(int x, double initialY)
    {
        var lowL = initialY;
        foreach (var point in Points.Where(p => p.X < x).Reverse())
        {
            if (point.Y <= lowL)
            {
                lowL = point.Y;
            }
            else
            {
                break;
            }
        }

        var lowR = initialY;
        foreach (var point in Points.Where(p => p.X > x))
        {
            if (point.Y <= lowR)
            {
                lowR = point.Y;
            }
            else
            {
                break;
            }
        }
        return Math.Min(lowL, lowR);
    }

    public int GetNextDamX(int currentDamX, double currentDamY, bool lookRight)
    {
        Point2D nextDam;
        if (lookRight)
        {
            nextDam = Points.FirstOrDefault(p => p.X > currentDamX && p.Y > currentDamY);
            return nextDam?.X ?? _maxX;
        }
        else
        {
            nextDam = Points.Reverse().FirstOrDefault(p => p.X < currentDamX && p.Y > currentDamY);
            return nextDam?.X ?? _minX;
        }
    }
}

public class Town
{
    public int X { get; }
    public string Name { get; }
    public double Y { get; private set; }
    public double SpilloverY { get; private set; }
    public double Elevation => Y - SpilloverY;
    public bool IsFlooded { get; set; }

    public Town(int x, string name)
    {
        X = x;
        Name = name;
        IsFlooded = false;
    }

    public void Initialize(Terrain terrain)
    {
        Y = terrain.GetYAtX(X);
        SpilloverY = terrain.GetLocalSpilloverY(X, Y);
    }
}

public class FloodBasin
{
    private int _leftDamX;
    private int _rightDamX;
    private readonly Terrain _terrain;

    public double WaterLevel { get; private set; }

    public FloodBasin(int initialLeftX, int initialRightX, Terrain terrain)
    {
        _leftDamX = initialLeftX;
        _rightDamX = initialRightX;
        _terrain = terrain;
        UpdateWaterLevel();
    }

    public bool Contains(Town town)
    {
        return town.X >= _leftDamX && town.X <= _rightDamX;
    }

    public void Expand()
    {
        var yL = _terrain.GetYAtX(_leftDamX);
        var yR = _terrain.GetYAtX(_rightDamX);

        if (yL >= yR)
        {
            _rightDamX = _terrain.GetNextDamX(_rightDamX, yR, lookRight: true);
        }
        if (yL <= yR)
        {
            _leftDamX = _terrain.GetNextDamX(_leftDamX, yL, lookRight: false);
        }
        UpdateWaterLevel();
    }

    private void UpdateWaterLevel()
    {
        WaterLevel = Math.Min(_terrain.GetYAtX(_leftDamX), _terrain.GetYAtX(_rightDamX));
    }
}