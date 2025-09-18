using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var ufos = new List<Ufo>();
        for (var i = 0; i < n; i++)
        {
            var sp = Console.ReadLine().Split(' ');
            var u = new Ufo(
                new Point3D(int.Parse(sp[0]), int.Parse(sp[1]), int.Parse(sp[2])),
                i
            );
            ufos.Add(u);
        }
        var planner = new AttackPlanner(ufos);
        var result = planner.GetMinimumTime();
        Console.WriteLine($"{result:F2}");
    }
}

class AttackPlanner
{
    private readonly List<Ufo> _ufos;
    private readonly ProjectileCalculator _proj;
    private readonly List<int>[] _sortedIndices;
    private float _bestTime;
    private readonly int _drawTimeA = 3;
    private readonly int _drawTimeB = 2;
    private readonly int _travelTime = 20;
    private readonly int _buffTime = 8;

    public AttackPlanner(List<Ufo> ufos)
    {
        _ufos = ufos;
        _proj = new ProjectileCalculator();
        foreach (var ufo in _ufos)
            ufo.ComputeAllTimes(_proj);
        _sortedIndices = new List<int>[3];
        MakeSortedLists();
        _bestTime = float.MaxValue;
    }

    public float GetMinimumTime()
    {
        var all = Enumerable.Range(0, _ufos.Count).ToList();
        FindSequence(new List<int>(), all, 0, 0, 0);
        return _bestTime;
    }

    private void MakeSortedLists()
    {
        var all = Enumerable.Range(0, _ufos.Count).ToList();
        _sortedIndices[0] = all.OrderByDescending(a => _ufos[a].Times[0]).ToList();
        _sortedIndices[1] = all.OrderByDescending(a => _ufos[a].Times[1]).ToList();
        _sortedIndices[2] = all.OrderByDescending(a => _ufos[a].Times[2]).ToList();
    }

    private List<int> SortActs(List<int> actions, int state)
    {
        var sorted = new List<int>();
        foreach (var a in _sortedIndices[state])
            if (actions.Contains(a)) sorted.Add(a);
        return sorted;
    }

    private void FindSequence(List<int> actsTaken, List<int> actsLeft, int actionTime, float flightTime, int state)
    {
        if (actsLeft.Count == 0)
        {
            if (flightTime < _bestTime) _bestTime = flightTime;
            return;
        }
        actsLeft = SortActs(actsLeft, state);
        if (state != 2)
        {
            int lastIndex = 0;
            for (; lastIndex < actsLeft.Count && _ufos[actsLeft[lastIndex]].Times[state] != -1; lastIndex++) { }
            for (int i = 0; i <= lastIndex; i++)
            {
                float branchFlight = flightTime;
                for (int j = 0; j < i; j++)
                {
                    var ufoIdx = actsLeft[j];
                    var cur = actionTime + (j + 1) * GetDrawTime(state) + _ufos[ufoIdx].Times[state];
                    if (cur > branchFlight) branchFlight = cur;
                }
                int newActTime = actionTime + GetDrawTime(state) * i;
                var newTaken = new List<int>(actsTaken);
                newTaken.AddRange(actsLeft.GetRange(0, i));
                if (i != actsLeft.Count)
                {
                    newActTime += state == 0 ? _travelTime : _buffTime;
                }
                FindSequence(newTaken, actsLeft.GetRange(i, actsLeft.Count - i), newActTime, branchFlight, state + 1);
            }
        }
        else
        {
            float branchFlight = flightTime;
            for (int i = 0; i < actsLeft.Count; i++)
            {
                var ufoIdx = actsLeft[i];
                var cur = actionTime + (i + 1) * GetDrawTime(state) + _ufos[ufoIdx].Times[state];
                if (cur > branchFlight) branchFlight = cur;
            }
            var actsAll = new List<int>(actsTaken);
            actsAll.AddRange(actsLeft);
            FindSequence(actsAll, new List<int>(), actionTime, branchFlight, state);
        }
    }

    private int GetDrawTime(int state) => state == 2 ? _drawTimeB : _drawTimeA;
}

class ProjectileCalculator
{
    private readonly float _gravity = 9.8f;
    private readonly int _fireSpdA = 60;
    private readonly int _fireSpdB = 80;
    private readonly Point3D _towerA = new Point3D(0, 80, 0);
    private readonly Point3D _towerB = new Point3D(200, 20, 0);

    public float GetFlightTime(Point3D target, int mode)
    {
        var fSpd = mode == 2 ? _fireSpdB : _fireSpdA;
        var origin = mode == 0 ? _towerA : _towerB;
        var v2 = fSpd * fSpd;
        var dx = target.X - origin.X;
        var dy = target.Y - origin.Y;
        var dz = target.Z - origin.Z;
        if (dx == 0 && dz == 0)
        {
            var sqrt = v2 - 2 * _gravity * dy;
            if (sqrt < 0) return -1;
            sqrt = MathF.Sqrt(sqrt);
            if (dy < 0) return (sqrt - fSpd) / _gravity;
            return (fSpd - sqrt) / _gravity;
        }
        var horiz = MathF.Sqrt(dx * dx + dz * dz);
        var gg = _gravity * (_gravity * horiz * horiz + 2 * dy * v2);
        var sqrtu = v2 * v2 - gg;
        if (sqrtu < 0) return -1;
        sqrtu = MathF.Sqrt(sqrtu);
        var tan = (v2 - sqrtu) / (_gravity * horiz);
        var angle = MathF.Atan(tan);
        var hVel = MathF.Cos(angle) * fSpd;
        return horiz / hVel;
    }
}

class Ufo
{
    public Point3D Pos { get; }
    public float[] Times { get; }
    public int Id { get; }
    public Ufo(Point3D pos, int id)
    {
        Pos = pos;
        Times = new float[3];
        Id = id;
    }
    public void ComputeAllTimes(ProjectileCalculator calc)
    {
        Times[0] = calc.GetFlightTime(Pos, 0);
        Times[1] = calc.GetFlightTime(Pos, 1);
        Times[2] = calc.GetFlightTime(Pos, 2);
    }
}

struct Point3D
{
    public float X, Y, Z;
    public Point3D(float x, float y, float z)
    {
        X = x; Y = y; Z = z;
    }
    public static Point3D operator +(Point3D a, Point3D b)
        => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Point3D operator -(Point3D a, Point3D b)
        => new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Point3D operator *(Point3D a, float f)
        => new Point3D(a.X * f, a.Y * f, a.Z * f);
    public static Point3D operator /(Point3D a, float f)
        => new Point3D(a.X / f, a.Y / f, a.Z / f);
    public float Mag() => MathF.Sqrt(X * X + Y * Y + Z * Z);
    public override string ToString() => $"({X}, {Y}, {Z})";
}
