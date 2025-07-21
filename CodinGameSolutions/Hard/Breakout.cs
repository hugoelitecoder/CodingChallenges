using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main(string[] args)
    {
        var ballPosInput = Console.ReadLine().Split(' ');
        var bX = double.Parse(ballPosInput[0]);
        var bY = double.Parse(ballPosInput[1]);
        var ballPos = new PointD(bX, bY);

        var ballVelInput = Console.ReadLine().Split(' ');
        var vX = double.Parse(ballVelInput[0]);
        var vY = double.Parse(ballVelInput[1]);
        var ballVel = new PointD(vX, vY);

        var pN = int.Parse(Console.ReadLine());
        var kN = int.Parse(Console.ReadLine());

        var paddlePositions = new List<PointD>();
        for (var i = 0; i < pN; i++)
        {
            var pInputs = Console.ReadLine().Split(' ');
            var pX = double.Parse(pInputs[0]);
            var pY = double.Parse(pInputs[1]);
            paddlePositions.Add(new PointD(pX, pY));
        }

        var bricks = new List<Brick>();
        for (var i = 0; i < kN; i++)
        {
            var kInputs = Console.ReadLine().Split(' ');
            var kX = double.Parse(kInputs[0]);
            var kY = double.Parse(kInputs[1]);
            var kStrength = int.Parse(kInputs[2]);
            var kPoints = int.Parse(kInputs[3]);
            bricks.Add(new Brick(kX, kY, kStrength, kPoints));
        }

        var breakoutGame = new Breakout(ballPos, ballVel, paddlePositions, bricks);
        breakoutGame.Simulate();
        Console.WriteLine(breakoutGame.Score);
    }
}

internal class Breakout
{
    private PointD _ballPos;
    private PointD _ballVel;
    private List<PointD> _paddlePositions;
    private List<Brick> _bricks;
    private int _paddleIdx;
    private long _score;
    private const double Epsilon = 1e-9;
    private const double FieldWidth = 1600;
    private const double FieldHeight = 2400;
    private const double BrickWidth = 100;
    private const double BrickHeight = 30;
    private const double PaddleWidth = 200;

    public long Score => _score;

    public Breakout(PointD ballPos, PointD ballVel, List<PointD> paddlePositions, List<Brick> bricks)
    {
        _ballPos = ballPos;
        _ballVel = ballVel;
        _paddlePositions = paddlePositions;
        _bricks = bricks;
        _paddleIdx = 0;
        _score = 0;
    }

    public void Simulate()
    {
        while (_ballPos.Y < FieldHeight)
        {
            var timeToBottom = double.PositiveInfinity;
            if (_ballVel.Y > 0)
            {
                timeToBottom = (FieldHeight - _ballPos.Y) / _ballVel.Y;
            }
            var nextCollision = FindNextCollision();
            if (nextCollision == null || nextCollision.Time >= timeToBottom)
            {
                break;
            }
            _ballPos.X += _ballVel.X * nextCollision.Time;
            _ballPos.Y += _ballVel.Y * nextCollision.Time;
            HandleCollision(nextCollision);
        }
    }

    private CollisionInfo FindNextCollision()
    {
        var minTime = double.PositiveInfinity;
        CollisionInfo bestCollision = null;
        if (_ballVel.Y < 0)
        {
            var t = -_ballPos.Y / _ballVel.Y;
            if (t > Epsilon && t < minTime)
            {
                minTime = t;
                bestCollision = new CollisionInfo { Time = t, Surface = CollisionSurface.Horizontal };
            }
        }
        if (_ballVel.X < 0)
        {
            var t = -_ballPos.X / _ballVel.X;
            if (t > Epsilon && t < minTime)
            {
                minTime = t;
                bestCollision = new CollisionInfo { Time = t, Surface = CollisionSurface.Vertical };
            }
        }
        if (_ballVel.X > 0)
        {
            var t = (FieldWidth - _ballPos.X) / _ballVel.X;
            if (t > Epsilon && t < minTime)
            {
                minTime = t;
                bestCollision = new CollisionInfo { Time = t, Surface = CollisionSurface.Vertical };
            }
        }
        foreach (var brick in _bricks)
        {
            if (_ballVel.Y > 0)
            {
                var t = (brick.Y - _ballPos.Y) / _ballVel.Y;
                if (t > Epsilon && t < minTime)
                {
                    var xAtHit = _ballPos.X + t * _ballVel.X;
                    if (xAtHit >= brick.X && xAtHit <= brick.X + BrickWidth)
                    {
                        minTime = t;
                        bestCollision = new CollisionInfo { Time = t, HitBrick = brick, Surface = CollisionSurface.Horizontal };
                    }
                }
            }
            if (_ballVel.Y < 0)
            {
                var t = (brick.Y + BrickHeight - _ballPos.Y) / _ballVel.Y;
                if (t > Epsilon && t < minTime)
                {
                    var xAtHit = _ballPos.X + t * _ballVel.X;
                    if (xAtHit >= brick.X && xAtHit <= brick.X + BrickWidth)
                    {
                        minTime = t;
                        bestCollision = new CollisionInfo { Time = t, HitBrick = brick, Surface = CollisionSurface.Horizontal };
                    }
                }
            }
            if (_ballVel.X > 0)
            {
                var t = (brick.X - _ballPos.X) / _ballVel.X;
                if (t > Epsilon && t < minTime)
                {
                    var yAtHit = _ballPos.Y + t * _ballVel.Y;
                    if (yAtHit >= brick.Y && yAtHit <= brick.Y + BrickHeight)
                    {
                        minTime = t;
                        bestCollision = new CollisionInfo { Time = t, HitBrick = brick, Surface = CollisionSurface.Vertical };
                    }
                }
            }
            if (_ballVel.X < 0)
            {
                var t = (brick.X + BrickWidth - _ballPos.X) / _ballVel.X;
                if (t > Epsilon && t < minTime)
                {
                    var yAtHit = _ballPos.Y + t * _ballVel.Y;
                    if (yAtHit >= brick.Y && yAtHit <= brick.Y + BrickHeight)
                    {
                        minTime = t;
                        bestCollision = new CollisionInfo { Time = t, HitBrick = brick, Surface = CollisionSurface.Vertical };
                    }
                }
            }
        }
        if (_ballVel.Y > 0 && _paddlePositions.Any())
        {
            var paddlePos = _paddlePositions[_paddleIdx];
            var t = (paddlePos.Y - _ballPos.Y) / _ballVel.Y;
            if (t > Epsilon && t < minTime)
            {
                var xAtHit = _ballPos.X + t * _ballVel.X;
                if (xAtHit >= paddlePos.X && xAtHit <= paddlePos.X + PaddleWidth)
                {
                    minTime = t;
                    bestCollision = new CollisionInfo { Time = t, IsPaddle = true, Surface = CollisionSurface.Horizontal };
                }
            }
        }
        return bestCollision;
    }

    private void HandleCollision(CollisionInfo collision)
    {
        if (collision.Surface == CollisionSurface.Horizontal)
        {
            _ballVel.Y *= -1;
        }
        else
        {
            _ballVel.X *= -1;
        }
        if (collision.HitBrick != null)
        {
            collision.HitBrick.Strength--;
            if (collision.HitBrick.Strength == 0)
            {
                _score += collision.HitBrick.Points;
                _bricks.Remove(collision.HitBrick);
            }
        }
        else if (collision.IsPaddle)
        {
            if (_paddleIdx < _paddlePositions.Count - 1)
            {
                _paddleIdx++;
            }
        }
    }
}

internal class PointD
{
    public double X { get; set; }
    public double Y { get; set; }
    public PointD(double x, double y)
    {
        X = x;
        Y = y;
    }
}

internal class Brick
{
    public double X { get; }
    public double Y { get; }
    public int Strength { get; set; }
    public int Points { get; }
    public Brick(double x, double y, int strength, int points)
    {
        X = x;
        Y = y;
        Strength = strength;
        Points = points;
    }
}

internal enum CollisionSurface
{
    Horizontal,
    Vertical
}

internal class CollisionInfo
{
    public double Time { get; set; }
    public Brick HitBrick { get; set; }
    public bool IsPaddle { get; set; }
    public CollisionSurface Surface { get; set; }
}