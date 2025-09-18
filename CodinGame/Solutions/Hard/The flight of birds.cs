using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

class Solution
{
    public static void Main(string[] args)
    {
        var dims = Console.ReadLine().Split(' ');
        var h = int.Parse(dims[0]);
        var w = int.Parse(dims[1]);
        var t = double.Parse(Console.ReadLine());
        var d = double.Parse(Console.ReadLine());
        var n = int.Parse(Console.ReadLine());
        var birds = new List<Bird>();
        var allStill = true;
        for (var i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            var id = int.Parse(parts[0]);
            var x = double.Parse(parts[1]);
            var y = double.Parse(parts[2]);
            var vx = double.Parse(parts[3]);
            var vy = double.Parse(parts[4]);
            if (vx != 0 || vy != 0) allStill = false;
            birds.Add(new Bird(id, new Vector2D(x, y), new Vector2D(vx, vy)));
        }
        if (allStill)
        {
            Console.WriteLine("No movement possible!");
            return;
        }
        var sim = new AviarySim(h, w, d, birds);
        sim.Simulate(t);
        if (Math.Abs(sim.GetRemainingTime() - t) < 1e-8)
        {
            Console.WriteLine("No movement possible!");
            return;
        }
        foreach (var bird in sim.GetBirds().OrderBy(b => b.Id))
            Console.WriteLine($"{bird.Id} [{Math.Round(bird.Pos.X)},{Math.Round(bird.Pos.Y)}]");
    }
}

class Vector2D
{
    public double X { get; }
    public double Y { get; }
    public Vector2D(double x, double y) { X = x; Y = y; }
    public Vector2D Add(Vector2D v) { return new Vector2D(X + v.X, Y + v.Y); }
    public Vector2D Mult(double c) { return new Vector2D(X * c, Y * c); }
    public Vector2D CentralSymOrtho() { return new Vector2D(-X, -Y); }
    public Vector2D HorizSymOrtho() { return new Vector2D(-X, Y); }
    public Vector2D VertSymOrtho() { return new Vector2D(X, -Y); }
    public double Dot(Vector2D v) { return X * v.X + Y * v.Y; }
    public double Length() { return Math.Sqrt(X * X + Y * Y); }
    public Vector2D Normalize() { var len = Length(); return len == 0 ? new Vector2D(0, 0) : new Vector2D(X / len, Y / len); }
}

class Bird
{
    public int Id { get; }
    public Vector2D Pos { get; set; }
    public Vector2D Speed { get; set; }
    public Bird(int id, Vector2D pos, Vector2D speed) { Id = id; Pos = pos; Speed = speed; }
}

enum WallSide { None, Left, Top, Right, Bottom }

class AviarySim
{
    private int _height;
    private int _width;
    private double _d;
    private List<Bird> _birds;
    private double _remainingTime;
    public AviarySim(int height, int width, double d, List<Bird> birds)
    {
        _height = height;
        _width = width;
        _d = d;
        _birds = birds;
    }
    public void Simulate(double t)
    {
        for (var i = 0; i < _birds.Count; i++)
            _birds[i].Speed = _birds[i].Speed.CentralSymOrtho();
        _remainingTime = t;
        var sw = Stopwatch.StartNew();
        while (_remainingTime > 0.0 && sw.Elapsed.TotalSeconds < 0.295)
        {
            List<int> birdsInCollision;
            WallSide wallSide;
            var time = GetTimeBeforeNextCollision(out birdsInCollision, out wallSide);
            if (time > _remainingTime) time = _remainingTime;
            for (var i = 0; i < _birds.Count; i++)
                _birds[i].Pos = _birds[i].Pos.Add(_birds[i].Speed.Mult(time));
            if (time < _remainingTime)
            {
                if (wallSide != WallSide.None)
                {
                    var index = birdsInCollision[0];
                    var bird = _birds[index];
                    switch (wallSide)
                    {
                        case WallSide.Left:
                        case WallSide.Right:
                            bird.Speed = bird.Speed.HorizSymOrtho();
                            break;
                        case WallSide.Top:
                        case WallSide.Bottom:
                            bird.Speed = bird.Speed.VertSymOrtho();
                            break;
                    }
                }
                else
                {
                    var b1 = _birds[birdsInCollision[0]];
                    var b2 = _birds[birdsInCollision[1]];
                    var dx = b2.Pos.X - b1.Pos.X;
                    var dy = b2.Pos.Y - b1.Pos.Y;
                    var normal = new Vector2D(dx, dy).Normalize();
                    var reflectB1 = GetReflection(b1.Speed, normal);
                    var reflectB2 = GetReflection(b2.Speed, normal);
                    b1.Speed = reflectB1;
                    b2.Speed = reflectB2;
                }
            }
            _remainingTime -= time;
        }
    }
    public double GetRemainingTime() { return _remainingTime; }
    public List<Bird> GetBirds() { return _birds; }
    private static Vector2D GetReflection(Vector2D incident, Vector2D normal)
    {
        var dotProduct = incident.Dot(normal);
        return new Vector2D(
            incident.X - 2 * dotProduct * normal.X,
            incident.Y - 2 * dotProduct * normal.Y
        );
    }
    private double GetTimeBeforeNextCollision(out List<int> birdsInCollision, out WallSide wallSide)
    {
        var timeMin = double.MaxValue;
        birdsInCollision = new List<int>();
        wallSide = WallSide.None;
        for (var i = 0; i < _birds.Count; i++)
        {
            WallSide side;
            var time = GetTimeBeforeWallCollision(_birds[i].Pos, _birds[i].Speed, out side);
            if (time < timeMin)
            {
                timeMin = time;
                birdsInCollision.Clear();
                birdsInCollision.Add(i);
                wallSide = side;
            }
            for (var j = i + 1; j < _birds.Count; j++)
            {
                var tBird = GetTimeBeforeBirdCollision(_birds[i].Pos, _birds[i].Speed, _birds[j].Pos, _birds[j].Speed);
                if (tBird < timeMin)
                {
                    timeMin = tBird;
                    birdsInCollision.Clear();
                    birdsInCollision.Add(i);
                    birdsInCollision.Add(j);
                    wallSide = WallSide.None;
                }
            }
        }
        return timeMin;
    }
    private double GetTimeBeforeWallCollision(Vector2D pos, Vector2D speed, out WallSide wallSide)
    {
        var timeMin = double.MaxValue;
        wallSide = WallSide.None;
        if (speed.X < 0)
        {
            var time = (_d - pos.X) / speed.X;
            if (time < timeMin) { timeMin = time; wallSide = WallSide.Left; }
        }
        if (speed.X > 0)
        {
            var time = (_width - _d - pos.X) / speed.X;
            if (time < timeMin) { timeMin = time; wallSide = WallSide.Right; }
        }
        if (speed.Y < 0)
        {
            var time = (_d - pos.Y) / speed.Y;
            if (time < timeMin) { timeMin = time; wallSide = WallSide.Bottom; }
        }
        if (speed.Y > 0)
        {
            var time = (_height - _d - pos.Y) / speed.Y;
            if (time < timeMin) { timeMin = time; wallSide = WallSide.Top; }
        }
        return timeMin;
    }
    private double GetTimeBeforeBirdCollision(Vector2D p1, Vector2D v1, Vector2D p2, Vector2D v2)
    {
        var xRel0 = p2.X - p1.X;
        var yRel0 = p2.Y - p1.Y;
        var vxRel = v2.X - v1.X;
        var vyRel = v2.Y - v1.Y;
        var a = vxRel * vxRel + vyRel * vyRel;
        var b = 2 * (xRel0 * vxRel + yRel0 * vyRel);
        var c = xRel0 * xRel0 + yRel0 * yRel0 - _d * _d;
        if (a == 0) return double.MaxValue;
        var delta = b * b - 4 * a * c;
        if (delta < 0) return double.MaxValue;
        var t1 = (-b - Math.Sqrt(delta)) / (2 * a);
        if (t1 < 0) return double.MaxValue;
        return t1;
    }
}

