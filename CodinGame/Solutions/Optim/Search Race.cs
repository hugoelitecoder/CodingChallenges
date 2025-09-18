using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    static void Main(string[] args)
    {
        var turnStopwatch = Stopwatch.StartNew(); 
        var game = new Game();
        var optimizer = new Optimizer();
        var totalCheckpoints = int.Parse(Console.ReadLine());
        game.SetTotalCheckpoints(totalCheckpoints);
        for (var i = 0; i < totalCheckpoints; i++)
        {
            game.AddCheckpoint(Console.ReadLine());
        }

        var isFirstTurn = true;
        while (true)
        {
            if (!isFirstTurn)
            {
                turnStopwatch.Restart();
            }

            var turnInput = Console.ReadLine();
            var currentState = game.ParseTurnInput(turnInput);
            
            var timeLimit = isFirstTurn ? 980 : 48;
            var bestSolution = optimizer.FindBestSolution(currentState, game.Checkpoints, game.TotalCheckpoints, timeLimit, turnStopwatch);
            var move = bestSolution.Moves[0];

            optimizer.ShiftSolutions(game.Checkpoints, currentState);

            Console.WriteLine($"{(int)move.Target.X} {(int)move.Target.Y} {move.Thrust} T:{move.Thrust}");
            
            Console.Error.WriteLine($"[DEBUG] Turn time: {turnStopwatch.ElapsedMilliseconds}ms");

            if (isFirstTurn)
            {
                isFirstTurn = false;
            }
        }
    }
}

public static class Rng
{
    private static readonly Random _random = new Random();
    public static double NextDouble() => _random.NextDouble();
    public static int Next(int min, int max) => _random.Next(min, max);
}

public static class Constants
{
    public const int CheckpointRadius = 600;
    public const double Friction = 0.85;
    public const double MaxTurnDeg = 18.0;
    public const double MaxTurnRad = MaxTurnDeg * Math.PI / 180.0;
    public const int MaxThrust = 200;
    public const int SolutionLength = 6;
    public const int PopulationSize = 40;
    public const double MutationRate = 0.15;
    public const double CrossoverRate = 0.7;
    public const double EliteRate = 0.1;
    public const double Epsilon = 1e-9;
}

public struct Point
{
    public readonly double X;
    public readonly double Y;

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double Distance(Point p) => Math.Sqrt((X - p.X) * (X - p.X) + (Y - p.Y) * (Y - p.Y));
    public static Point operator +(Point p, Vector v) => new Point(p.X + v.X, p.Y + v.Y);
    public static Vector operator -(Point p1, Point p2) => new Vector(p1.X - p2.X, p1.Y - p2.Y);
}

public struct Vector
{
    public readonly double X;
    public readonly double Y;

    public Vector(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);
    public static Vector operator *(Vector v, double s) => new Vector(v.X * s, v.Y * s);
    
    public double Magnitude() => Math.Sqrt(X * X + Y * Y);
    public double Angle() => Math.Atan2(Y, X);
}

public class CarState
{
    public Point Pos;
    public Vector Vel;
    public double Angle;
    public int NextCpId;

    public CarState(Point pos, Vector vel, double angle, int nextCpId)
    {
        Pos = pos;
        Vel = vel;
        Angle = angle;
        NextCpId = nextCpId;
    }
    
    public CarState Clone() => new CarState(Pos, Vel, Angle, NextCpId);
}

public class Move
{
    public Point Target;
    public int Thrust;

    public Move(Point target, int thrust)
    {
        Target = target;
        Thrust = thrust;
    }

    public Move Clone() => new Move(Target, Thrust);
    
    public void Mutate(Point checkpoint)
    {
        if (Rng.NextDouble() < 0.5)
        {
            var angle = Rng.NextDouble() * 2 * Math.PI;
            var radius = Rng.NextDouble() * 800 - 100;
            var offset = new Vector(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
            Target = checkpoint + offset;
        } else {
             Thrust = Rng.Next(0, Constants.MaxThrust + 1);
        }
    }
}

public class Solution
{
    public Move[] Moves;
    public double Fitness;

    public Solution()
    {
        Moves = new Move[Constants.SolutionLength];
        Fitness = double.MinValue;
    }
    
    public void Randomize(CarState initialState, List<Point> checkpoints)
    {
        var currentCpIndex = initialState.NextCpId;
        for (var i = 0; i < Moves.Length; i++)
        {
            var cpIndex = Math.Min(currentCpIndex + i, checkpoints.Count - 1);
            var currentCp = checkpoints[cpIndex];
            var angle = Rng.NextDouble() * 2 * Math.PI;
            var radius = Rng.NextDouble() * 500.0;
            var target = currentCp + new Vector(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
            var thrust = Rng.Next(150, Constants.MaxThrust + 1);
            Moves[i] = new Move(target, thrust);
        }
    }

    public Solution Clone()
    {
        var clone = new Solution();
        for (var i = 0; i < Moves.Length; i++)
        {
            clone.Moves[i] = Moves[i].Clone();
        }
        clone.Fitness = Fitness;
        return clone;
    }
}

public class Game
{
    public List<Point> Checkpoints { get; } = new List<Point>();
    public int TotalCheckpoints { get; private set; }

    private int ParseNextInt(string line, ref int cursor)
    {
        while (cursor < line.Length && line[cursor] == ' ')
        {
            cursor++;
        }
        var sign = 1;
        if (cursor < line.Length && line[cursor] == '-')
        {
            sign = -1;
            cursor++;
        }
        var value = 0;
        while (cursor < line.Length && line[cursor] >= '0' && line[cursor] <= '9')
        {
            value = value * 10 + (line[cursor] - '0');
            cursor++;
        }
        return value * sign;
    }
    
    public void SetTotalCheckpoints(int count)
    {
        TotalCheckpoints = count;
    }
    
    public void AddCheckpoint(string line)
    {
        var cursor = 0;
        var x = ParseNextInt(line, ref cursor);
        var y = ParseNextInt(line, ref cursor);
        Checkpoints.Add(new Point(x,y));
    }

    public CarState ParseTurnInput(string line)
    {
        var cursor = 0;
        var checkpointIndex = ParseNextInt(line, ref cursor);
        var x = ParseNextInt(line, ref cursor);
        var y = ParseNextInt(line, ref cursor);
        var vx = ParseNextInt(line, ref cursor);
        var vy = ParseNextInt(line, ref cursor);
        var angleDegrees = ParseNextInt(line, ref cursor);
        var angleRadians = angleDegrees * Math.PI / 180.0;

        return new CarState(new Point(x, y), new Vector(vx, vy), angleRadians, checkpointIndex);
    }
}

public struct Collision
{
    public readonly double Time;
    public static readonly Collision NoCollision = new Collision(-1.0);
    public Collision(double t) { Time = t; }
}

public static class Simulator
{
    private static double ShortestAngleDist(double from, double to)
    {
        var max = Math.PI * 2;
        var da = (to - from) % max;
        return (2 * da % max) - da;
    }

    private static Collision GetCollision(Point carPos, Vector carVel, Point cpPos, double cpRadius)
    {
        var x2 = carPos.X - cpPos.X;
        var y2 = carPos.Y - cpPos.Y;
        var vx2 = carVel.X;
        var vy2 = carVel.Y;

        var a = vx2 * vx2 + vy2 * vy2;
        if (a <= Constants.Epsilon) return Collision.NoCollision;
        
        var b = 2.0 * (x2 * vx2 + y2 * vy2);
        var c = x2 * x2 + y2 * y2 - cpRadius * cpRadius;
        var delta = b * b - 4.0 * a * c;

        if (delta < 0.0) return Collision.NoCollision;

        var t = (-b - Math.Sqrt(delta)) / (2.0 * a);
        return new Collision(t);
    }

    public static double Evaluate(CarState initialState, Solution solution, List<Point> checkpoints, int totalCheckpoints)
    {
        var car = initialState.Clone();
        var checkpointsPassed = 0;
        var turns = 0.0;

        for (var i = 0; i < solution.Moves.Length; i++)
        {
            var move = solution.Moves[i];
            
            var targetAngle = (move.Target - car.Pos).Angle();
            var angleDiff = ShortestAngleDist(car.Angle, targetAngle);
            angleDiff = Math.Max(-Constants.MaxTurnRad, Math.Min(Constants.MaxTurnRad, angleDiff));
            car.Angle += angleDiff;
            
            var thrustVec = new Vector(Math.Cos(car.Angle), Math.Sin(car.Angle)) * move.Thrust;
            car.Vel += thrustVec;
            
            var tInTurn = 0.0;
            while(tInTurn < 1.0)
            {
                if (car.NextCpId >= totalCheckpoints)
                {
                    car.Pos += car.Vel * (1.0 - tInTurn);
                    break;
                }
                var targetCp = checkpoints[car.NextCpId];
                var col = GetCollision(car.Pos, car.Vel, targetCp, Constants.CheckpointRadius);
                
                if (col.Time < 0.0 || col.Time >= 1.0 - tInTurn)
                {
                    car.Pos += car.Vel * (1.0 - tInTurn);
                    break;
                }
                
                car.Pos += car.Vel * col.Time;
                tInTurn += col.Time;
                checkpointsPassed++;
                car.NextCpId++;

                if (car.NextCpId >= totalCheckpoints)
                {
                    turns = i + tInTurn;
                    return 1e9 - turns * 1000;
                }
            }

            car.Pos = new Point((int)car.Pos.X, (int)car.Pos.Y);
            car.Vel *= Constants.Friction;
            car.Vel = new Vector((int)car.Vel.X, (int)car.Vel.Y);
            
            var degrees = Math.Round(car.Angle * 180.0 / Math.PI);
            car.Angle = degrees * Math.PI / 180.0;
            while (car.Angle >= Math.PI*2) car.Angle -= Math.PI*2;
            while (car.Angle < 0) car.Angle += Math.PI*2;
        }

        var finalTarget = checkpoints[Math.Min(car.NextCpId, checkpoints.Count - 1)];
        var distance = car.Pos.Distance(finalTarget);
        return checkpointsPassed * 500000.0 - distance - car.Vel.Magnitude() * 0.5;
    }
}

public class Optimizer
{
    private List<Solution> _population = new List<Solution>();
    
    public Solution FindBestSolution(CarState initialState, List<Point> checkpoints, int totalCheckpoints, int timeLimit, Stopwatch stopwatch)
    {
        if (_population.Count == 0)
        {
            InitializePopulation(initialState, checkpoints);
        }

        var generationCount = 0;
        while (stopwatch.ElapsedMilliseconds < timeLimit)
        {
            EvaluatePopulation(initialState, checkpoints, totalCheckpoints);
            
            var nextGeneration = new List<Solution>();
            var eliteCount = (int)(Constants.PopulationSize * Constants.EliteRate);
            
            _population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
            
            for (var i = 0; i < eliteCount; i++)
            {
                nextGeneration.Add(_population[i].Clone());
            }

            while (nextGeneration.Count < Constants.PopulationSize)
            {
                var parent1 = SelectParent();
                var parent2 = SelectParent();

                var child = Crossover(parent1, parent2);
                Mutate(child, initialState, checkpoints);
                
                nextGeneration.Add(child);
            }
            _population = nextGeneration;
            generationCount++;
        }
        
        EvaluatePopulation(initialState, checkpoints, totalCheckpoints);
        _population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
        
        Console.Error.WriteLine($"[DEBUG] Generations: {generationCount}, Best Fitness: {_population[0].Fitness:F0}");
        return _population[0];
    }
    
    public void ShiftSolutions(List<Point> checkpoints, CarState currentState)
    {
        foreach (var solution in _population)
        {
            Array.Copy(solution.Moves, 1, solution.Moves, 0, Constants.SolutionLength - 1);
            
            var cpIndex = Math.Min(currentState.NextCpId + Constants.SolutionLength - 1, checkpoints.Count - 1);
            var cp = checkpoints[cpIndex];
            var angle = Rng.NextDouble() * 2 * Math.PI;
            var radius = Rng.NextDouble() * 500.0;
            var target = cp + new Vector(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
            solution.Moves[Constants.SolutionLength - 1] = new Move(target, Constants.MaxThrust);
        }
    }

    private void InitializePopulation(CarState initialState, List<Point> checkpoints)
    {
        for (var i = 0; i < Constants.PopulationSize; i++)
        {
            var solution = new Solution();
            solution.Randomize(initialState, checkpoints);
            _population.Add(solution);
        }
    }

    private void EvaluatePopulation(CarState initialState, List<Point> checkpoints, int totalCheckpoints)
    {
        foreach (var solution in _population)
        {
            solution.Fitness = Simulator.Evaluate(initialState, solution, checkpoints, totalCheckpoints);
        }
    }
    
    private Solution SelectParent()
    {
        var best = _population[Rng.Next(0, _population.Count)];
        var contender = _population[Rng.Next(0, _population.Count)];
        return contender.Fitness > best.Fitness ? contender : best;
    }
    
    private Solution Crossover(Solution parent1, Solution parent2)
    {
        var child = parent1.Clone();
        if (Rng.NextDouble() < Constants.CrossoverRate)
        {
            var crossoverPoint = Rng.Next(1, Constants.SolutionLength);
            for (var i = crossoverPoint; i < Constants.SolutionLength; i++)
            {
                child.Moves[i] = parent2.Moves[i].Clone();
            }
        }
        return child;
    }
    
    private void Mutate(Solution solution, CarState initialState, List<Point> checkpoints)
    {
        for (var i = 0; i < Constants.SolutionLength; i++)
        {
            if (Rng.NextDouble() < Constants.MutationRate)
            {
                var cpIndex = Math.Min(initialState.NextCpId + i, checkpoints.Count - 1);
                var targetCp = checkpoints[cpIndex];
                solution.Moves[i].Mutate(targetCp);
            }
        }
    }
}