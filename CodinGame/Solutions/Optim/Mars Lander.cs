using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    static void Main(string[] args)
    {
        var io = new IO();
        var surfaceCount = int.Parse(io.ReadLine());
        var points = new List<Point2d>(surfaceCount);
        for (var i = 0; i < surfaceCount; i++)
        {
            var input = io.ReadLine().Split(' ');
            points.Add(new Point2d(int.Parse(input[0]), int.Parse(input[1])));
        }
        var board = new Board(points);
        var state = new GameState(board);
        var population = new Population(100);
        var isFirstTurn = true;
        while (true)
        {
            var input = io.ReadLine().Split(' ');
            var ship = new Ship(
                int.Parse(input[0]),
                int.Parse(input[1]),
                int.Parse(input[2]),
                int.Parse(input[3]),
                int.Parse(input[4]),
                int.Parse(input[5]),
                int.Parse(input[6]));
            if (isFirstTurn)
            {
                state.SetShip(ship);
                for (var i = 0; i < 100; i++)
                {
                    population.Add(new MarsLanderSolution(state));
                }
            }
            var watch = Stopwatch.StartNew();
            var timeLimit = isFirstTurn ? 995 : 95;
            var genetic = new GeneticAlgorithm(population, 0.01, 0.05, 0.2);
            var move = (Move)genetic.GetNextMove(watch, timeLimit);
            population = genetic.Population;
            io.Debug($"gen: {genetic.GenerationCounter}, move: {move}, score: {population.GetBestIndividual().Fitness}");
            state.ApplyMove(move, true);
            for (var i = 0; i < population.Count; i++)
            {
                ((MarsLanderSolution)population[i]).AdvanceTurn(state);
            }
            watch.Stop();
            io.Debug("ms: " + watch.ElapsedMilliseconds);
            io.WriteLine(move.ToString());
            isFirstTurn = false;
        }
    }

    private class IO
    {
        public string ReadLine() => Console.ReadLine();
        public void WriteLine(string message) => Console.WriteLine(message);
        public void Debug(string message) => Console.Error.WriteLine("[DEBUG] " + message);
    }
}

public static class PhysicsConstants
{
    public const double Gravity = -3.711;
    public static readonly double[] SinTable = new double[181];
    public static readonly double[] CosTable = new double[181];

    static PhysicsConstants()
    {
        for (var angle = -90; angle <= 90; angle++)
        {
            var radians = angle * Math.PI / 180.0;
            SinTable[angle + 90] = Math.Sin(radians);
            CosTable[angle + 90] = Math.Cos(radians);
        }
    }
}

public interface IGameState
{
    void ApplyMove(object move, bool isMax);
    IGameState Clone();
    double? GetWinner();
    double Evaluate(bool isMax);
}

public class GeneticAlgorithm
{
    public Population Population;

    private Population _hiddenPopulation;
    private readonly double _mutationRate;
    private readonly double _elitePercent;
    private readonly double _crossOver;
    private readonly double[] _cumulativeFitness;

    public int GenerationCounter;

    public GeneticAlgorithm(Population initialPopulation, double mutationRate, double elitePercent, double crossOver)
    {
        Population = initialPopulation;
        _hiddenPopulation = initialPopulation.Clone();
        _mutationRate = mutationRate;
        _elitePercent = elitePercent;
        _crossOver = crossOver;
        _cumulativeFitness = new double[initialPopulation.Count];
    }

    public object GetNextMove(Stopwatch watch, int timeLimit)
    {
        while (true)
        {
            var complete = true;
            for (var i = 0; i < Population.Count; i++)
            {
                var individual = Population[i];
                if (individual.Fitness == double.MinValue)
                {
                    individual.CalculateFitness();
                }
                if (watch.ElapsedMilliseconds >= timeLimit)
                {
                    complete = false;
                    break;
                }
            }
            if (!complete)
            {
                break;
            }
            GenerateNextGeneration();
            if (watch.ElapsedMilliseconds >= timeLimit)
            {
                break;
            }
        }
        return Population.GetBestIndividual().GetNextMove();
    }

    private void GenerateNextGeneration()
    {
        GenerationCounter++;
        Population.SortPopulationByFitness();
        var count = Population.Count;
        var eliteCount = (int)(count * _elitePercent);
        if (eliteCount == 0)
        {
            eliteCount = 1;
        }
        for (var i = 0; i < eliteCount; i++)
        {
            _hiddenPopulation[i] = Population[i];
        }
        var totalFitness = Population.BuildCumulativeFitness(_cumulativeFitness);
        for (var i = eliteCount; i < count; i++)
        {
            var parent1 = Population.SelectRandom(_cumulativeFitness, totalFitness);
            var parent2 = Population.SelectRandom(_cumulativeFitness, totalFitness, parent1);
            var child = _hiddenPopulation[i].CreateBaby(parent1, parent2, _crossOver);
            child.Mutate(_mutationRate);
            child.Fitness = double.MinValue;
            i++;
            if (i < count)
            {
                child = _hiddenPopulation[i].CreateBaby(parent2, parent1, _crossOver);
                child.Mutate(_mutationRate);
                child.Fitness = double.MinValue;
            }
        }
        var previousPopulation = Population;
        Population = _hiddenPopulation;
        _hiddenPopulation = previousPopulation;
    }
}

public interface Individual
{
    double Fitness { get; set; }
    Individual CreateBaby(Individual parent1, Individual parent2, double crossOver);
    void Mutate(double mutationRate);
    double CalculateFitness();
    object GetNextMove();
    Individual Clone();
}

public class Population : IEnumerable<Individual>
{
    private static readonly Comparison<Individual> FitnessComparison = CompareByFitness;

    private readonly List<Individual> _individuals;
    private readonly Random _random;

    public int Count => _individuals.Count;

    public Individual this[int index]
    {
        get => _individuals[index];
        set => _individuals[index] = value;
    }

    public Population(int capacity = 0)
    {
        _individuals = new List<Individual>(capacity);
        _random = new Random();
    }

    private Population(Population population)
    {
        _individuals = new List<Individual>(population.Count);
        for (var i = 0; i < population.Count; i++)
        {
            _individuals.Add(population[i].Clone());
        }
        _random = new Random();
    }

    public void Add(Individual individual) => _individuals.Add(individual);

    public Population Clone() => new Population(this);

    public void SortPopulationByFitness() => _individuals.Sort(FitnessComparison);

    public Individual GetBestIndividual()
    {
        var best = _individuals[0];
        for (var i = 1; i < _individuals.Count; i++)
        {
            if (_individuals[i].Fitness > best.Fitness)
            {
                best = _individuals[i];
            }
        }
        return best;
    }

    public double BuildCumulativeFitness(double[] cumulativeFitness)
    {
        var totalFitness = 0.0;
        for (var i = 0; i < _individuals.Count; i++)
        {
            var fitness = _individuals[i].Fitness;
            if (fitness > 0)
            {
                totalFitness += fitness;
            }
            cumulativeFitness[i] = totalFitness;
        }
        if (totalFitness > 0)
        {
            return totalFitness;
        }
        for (var i = 0; i < _individuals.Count; i++)
        {
            cumulativeFitness[i] = i + 1;
        }
        return _individuals.Count;
    }

    public Individual SelectRandom(double[] cumulativeFitness, double totalFitness, Individual excluded = null)
    {
        var count = _individuals.Count;
        var target = _random.NextDouble() * totalFitness;
        var low = 0;
        var high = count - 1;
        while (low < high)
        {
            var middle = low + (high - low) / 2;
            if (target <= cumulativeFitness[middle])
            {
                high = middle;
            }
            else
            {
                low = middle + 1;
            }
        }
        if (excluded != null && ReferenceEquals(_individuals[low], excluded) && count > 1)
        {
            low = low == count - 1 ? low - 1 : low + 1;
        }
        return _individuals[low];
    }

    public IEnumerator<Individual> GetEnumerator() => _individuals.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static int CompareByFitness(Individual first, Individual second) => second.Fitness.CompareTo(first.Fitness);
}

public class Point2d
{
    public double x;
    public double y;

    public Point2d(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public int GetTruncatedX() => (int)x;

    public double GetDistance(Point2d point)
    {
        var dx = x - point.x;
        var dy = y - point.y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public Point2d GetMidPoint(Point2d point) => new Point2d((x + point.x) * 0.5, (y + point.y) * 0.5);

    public Point2d GetRoundedAwayFromZeroPoint() => new Point2d(Math.Round(x, MidpointRounding.AwayFromZero), Math.Round(y, MidpointRounding.AwayFromZero));

    public Point2d Clone() => new Point2d(x, y);

    public void Fill(Point2d point)
    {
        x = point.x;
        y = point.y;
    }

    public override string ToString() => "(" + x + "," + y + ")";
}

public class Board
{
    public List<Point2d> Points;
    public Tuple<Point2d, Point2d> LandingSpot;
    public int[] MaxYAtX;
    public double[] PointIndexToDistanceFromLandingSpot;
    public bool UseHighGroundFlightProfile;

    private int _landingStartIndex = -1;

    public Board(List<Point2d> points)
    {
        Points = points;
        MaxYAtX = new int[7000];
        for (var i = 1; i < points.Count; i++)
        {
            var previous = points[i - 1];
            var current = points[i];
            var slope = (previous.y - current.y) / (previous.x - current.x);
            var yIntercept = current.y - slope * current.x;
            for (var x = previous.GetTruncatedX(); x < current.x; x++)
            {
                if (x >= 0 && x < 7000)
                {
                    MaxYAtX[x] = (int)Math.Ceiling(slope * x + yIntercept);
                }
            }
        }
        GetLandingSpot();
        PointIndexToDistanceFromLandingSpot = new double[points.Count];
        var pathDistances = new double[points.Count];
        for (var i = 1; i < points.Count; i++)
        {
            pathDistances[i] = pathDistances[i - 1] + points[i - 1].GetDistance(points[i]);
        }
        var landingDistance = pathDistances[_landingStartIndex];
        for (var i = 0; i < points.Count; i++)
        {
            PointIndexToDistanceFromLandingSpot[i] = Math.Abs(pathDistances[i] - landingDistance);
        }
    }

    public Tuple<Point2d, Point2d> GetLandingSpot()
    {
        if (LandingSpot != null)
        {
            return LandingSpot;
        }
        for (var i = 1; i < Points.Count; i++)
        {
            if (Points[i - 1].y == Points[i].y)
            {
                _landingStartIndex = i - 1;
                LandingSpot = new Tuple<Point2d, Point2d>(Points[i - 1], Points[i]);
                break;
            }
        }
        return LandingSpot;
    }

    public void ConfigureFlightProfile(Ship ship)
    {
        var landingY = GetLandingSpot().Item1.y;
        var x = Math.Max(0, Math.Min(6999, ship.Location.GetTruncatedX()));
        UseHighGroundFlightProfile = landingY >= MaxYAtX[x] + 350;
    }

    public bool ShipLanded(Ship ship)
    {
        var landingSpot = GetLandingSpot();
        return ship.LastLocation.y >= landingSpot.Item1.y &&
               ship.Location.y <= landingSpot.Item1.y &&
               ShipWithinVelocityForLanding(ship);
    }

    public bool ShipWithinVelocityForLanding(Ship ship)
    {
        return Math.Abs(ship.VelocityVector.x) <= 19 &&
               Math.Abs(ship.VelocityVector.y) <= 39 &&
               ship.RotationAngle == 0;
    }

    public bool IsInBounds(Ship ship)
    {
        if (ship.Location.x <= 0)
        {
            ship.CrashPoint = new Tuple<Point2d, Point2d>(Points[0], Points[1]);
            ship.CrashSegmentIndex = 0;
            return false;
        }
        if (ship.Location.x >= 6999)
        {
            ship.CrashSegmentIndex = Points.Count - 2;
            ship.CrashPoint = new Tuple<Point2d, Point2d>(Points[ship.CrashSegmentIndex], Points[ship.CrashSegmentIndex + 1]);
            return false;
        }
        if (ship.Location.y <= 0 || ship.Location.y >= 2999)
        {
            ship.CrashPoint = null;
            ship.CrashSegmentIndex = -1;
            return false;
        }
        return true;
    }

    public bool? ShipCollision(Ship ship)
    {
        var minX = Math.Min(ship.Location.x, ship.LastLocation.x);
        var maxX = Math.Max(ship.Location.x, ship.LastLocation.x);
        var startIndex = FindPointIndexBeforeX(minX);
        var endIndex = FindPointIndexBeforeX(maxX);
        for (var i = startIndex; i <= endIndex && i < Points.Count - 1; i++)
        {
            var first = Points[i];
            var second = Points[i + 1];
            if (!IsIntersecting(first, second, ship.Location, ship.LastLocation))
            {
                continue;
            }
            ship.CrashSegmentIndex = i;
            ship.CrashPoint = new Tuple<Point2d, Point2d>(first, second);
            var landingSpot = GetLandingSpot();
            if (ReferenceEquals(landingSpot.Item1, first) && ReferenceEquals(landingSpot.Item2, second))
            {
                return !ShipLanded(ship);
            }
            return true;
        }
        return null;
    }

    public double CalculatePathDistance(Ship ship)
    {
        var index = ship.CrashSegmentIndex;
        if (index < 0 || index + 1 >= Points.Count)
        {
            return 0;
        }
        var distanceToFirst = PointIndexToDistanceFromLandingSpot[index];
        var distanceToSecond = PointIndexToDistanceFromLandingSpot[index + 1];
        if (distanceToFirst == 0 && distanceToSecond == 0)
        {
            return 0;
        }
        return Math.Min(
            ship.Location.GetDistance(Points[index]) + distanceToFirst,
            ship.Location.GetDistance(Points[index + 1]) + distanceToSecond);
    }

    private int FindPointIndexBeforeX(double x)
    {
        if (x < Points[0].x)
        {
            return 0;
        }
        if (x >= Points[Points.Count - 1].x)
        {
            return Points.Count - 2;
        }
        var low = 0;
        var high = Points.Count - 1;
        var result = 0;
        while (low <= high)
        {
            var middle = low + (high - low) / 2;
            if (Points[middle].x <= x)
            {
                result = middle;
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }
        return result;
    }

    private static bool IsIntersecting(Point2d a, Point2d b, Point2d c, Point2d d)
    {
        var denominator = (b.x - a.x) * (d.y - c.y) - (b.y - a.y) * (d.x - c.x);
        if (denominator == 0)
        {
            return false;
        }
        var numerator1 = (a.y - c.y) * (d.x - c.x) - (a.x - c.x) * (d.y - c.y);
        var numerator2 = (a.y - c.y) * (b.x - a.x) - (a.x - c.x) * (b.y - a.y);
        var r = numerator1 / denominator;
        var s = numerator2 / denominator;
        return r >= 0 && r <= 1 && s >= 0 && s <= 1;
    }
}

public class MarsLanderSolution : Individual
{
    private const int TotalMoves = 160;
    private static readonly Random Random = new Random();

    public StaticMove[] Moves;
    public double Fitness { get; set; }
    public GameState State;
    public int Turn;

    public MarsLanderSolution(GameState state)
    {
        Fitness = double.MinValue;
        State = (GameState)state.Clone();
        Moves = new StaticMove[TotalMoves];
        for (var i = 0; i < TotalMoves; i++)
        {
            Moves[i] = CreateRandomMove();
        }
    }

    private MarsLanderSolution(MarsLanderSolution solution)
    {
        Fitness = solution.Fitness;
        State = (GameState)solution.State.Clone();
        Moves = new StaticMove[TotalMoves];
        Turn = solution.Turn;
        for (var i = 0; i < TotalMoves; i++)
        {
            Moves[i] = (StaticMove)solution.Moves[i].Clone();
        }
    }

    public double CalculateFitness()
    {
        var clonedState = (GameState)State.Clone();
        var counter = Turn;
        double? winner;
        do
        {
            clonedState.ApplyMove(Moves[counter++], true);
            winner = clonedState.GetWinner();
        }
        while (winner == null && counter < TotalMoves);
        Fitness = winner ?? clonedState.Evaluate(true);
        return Fitness;
    }

    public Individual CreateBaby(Individual parent1, Individual parent2, double crossOver)
    {
        var firstParent = (MarsLanderSolution)parent1;
        var secondParent = (MarsLanderSolution)parent2;
        var weight = Random.NextDouble();
        State.Fill(firstParent.State);
        for (var i = firstParent.Turn; i < TotalMoves; i++)
        {
            var firstMove = firstParent.Moves[i];
            var secondMove = secondParent.Moves[i];
            Moves[i].Rotation = (int)Math.Round(weight * firstMove.Rotation + (1 - weight) * secondMove.Rotation);
            Moves[i].Power = (int)Math.Round(weight * firstMove.Power + (1 - weight) * secondMove.Power);
        }
        Turn = firstParent.Turn;
        return this;
    }

    public void Mutate(double mutationRate)
    {
        for (var i = 0; i < TotalMoves * mutationRate; i++)
        {
            var index = Random.Next(Turn, TotalMoves);
            CreateRandomMove(Moves[index]);
        }
    }

    public void AdvanceTurn(GameState updatedState)
    {
        State = (GameState)updatedState.Clone();
        Turn++;
        Fitness = double.MinValue;
    }

    public object GetNextMove()
    {
        if (State.Board.UseHighGroundFlightProfile)
        {
            return StaticMove.ConvertToMove(State.Ship, Moves[Turn]);
        }
        var move = Moves[Turn];
        var rotation = Math.Max(-90, Math.Min(90, State.Ship.RotationAngle + move.Rotation));
        var power = Math.Max(0, Math.Min(4, State.Ship.Power + move.Power));
        FlightCommandOptimizer.Optimize(State.Ship, State.Board, rotation, power, out rotation, out power);
        return new Move(rotation, power);
    }

    public Individual Clone() => new MarsLanderSolution(this);

    private static StaticMove CreateRandomMove()
    {
        return new StaticMove(Random.Next(-23, 23), Random.Next(-1, 2));
    }

    private static void CreateRandomMove(StaticMove move)
    {
        move.Rotation = Random.Next(-23, 23);
        move.Power = Random.Next(-1, 2);
    }
}

public class Move
{
    public int Rotation;
    public int Power;

    public Move(int rotation, int power)
    {
        Rotation = rotation;
        Power = power;
    }

    public virtual Move Clone() => new Move(Rotation, Power);

    public override string ToString() => Rotation + " " + Power;
}

public class StaticMove : Move
{
    public StaticMove(int rotation, int power) : base(rotation, power)
    {
    }

    public static Move ConvertToMove(Ship ship, StaticMove move)
    {
        var rotation = Math.Min(Math.Max(ship.RotationAngle + move.Rotation, -90), 90);
        var power = Math.Max(Math.Min(ship.Power + move.Power, 4), 0);
        return new Move(rotation, power);
    }

    public override Move Clone() => new StaticMove(Rotation, Power);
}

public static class FlightCommandOptimizer
{
    public static void Optimize(Ship ship, Board board, int rotation, int power, out int optimizedRotation, out int optimizedPower)
    {
        var landingSpot = board.GetLandingSpot();
        var targetX = (landingSpot.Item1.x + landingSpot.Item2.x) * 0.5;
        var landingY = landingSpot.Item1.y;
        var horizontalError = targetX - ship.Location.x;
        var distanceToTarget = Math.Abs(horizontalError);
        var direction = horizontalError > 30 ? 1 : horizontalError < -30 ? -1 : 0;
        var horizontalVelocity = ship.VelocityVector.x;
        var horizontalSpeed = Math.Abs(horizontalVelocity);
        var x = Math.Max(0, Math.Min(6999, ship.Location.GetTruncatedX()));
        var terrainY = board.MaxYAtX[x];
        var clearance = ship.Location.y - terrainY;
        var isOverLandingZone = ship.Location.x >= landingSpot.Item1.x && ship.Location.x <= landingSpot.Item2.x;
        var progressSpeed = direction * horizontalVelocity;
        var stoppingDistance = horizontalVelocity * horizontalVelocity / 5.5;
        var cruiseSpeed = Math.Min(42.0, 15.0 + distanceToTarget * 0.018);
        var brakeSpeed = isOverLandingZone ? 19.0 : 16.0;
        var needsBrake = horizontalSpeed > brakeSpeed && (isOverLandingZone || direction == 0 || progressSpeed <= 0 || stoppingDistance + 180.0 >= distanceToTarget);
        var needsAcceleration = !isOverLandingZone && direction != 0 && distanceToTarget > 450.0 && progressSpeed < cruiseSpeed && stoppingDistance + 240.0 < distanceToTarget;
        var verticalVelocity = ship.VelocityVector.y;
        var descentSpeed = Math.Max(0.0, -verticalVelocity);
        var heightAboveLanding = ship.Location.y - landingY;
        var brakingHeight = descentSpeed > 8.0 ? (descentSpeed * descentSpeed - 64.0) / 0.55 : 0.0;
        var holdHighLandingApproach = !isOverLandingZone &&
                                      landingY >= terrainY + 350.0 &&
                                      heightAboveLanding > 0.0 &&
                                      heightAboveLanding <= brakingHeight + 500.0;
        if (holdHighLandingApproach)
        {
            if (needsBrake)
            {
                optimizedRotation = horizontalVelocity > 0.0 ? 10 : horizontalVelocity < 0.0 ? -10 : 0;
            }
            else if (direction != 0 && progressSpeed < 8.0)
            {
                optimizedRotation = -direction * 10;
            }
            else
            {
                optimizedRotation = 0;
            }
            var holdDescentSpeed = distanceToTarget > 1200.0 ? 4.0 : heightAboveLanding > 700.0 ? 6.0 : heightAboveLanding > 350.0 ? 4.0 : 2.0;
            optimizedPower = descentSpeed >= holdDescentSpeed ? 4 : 3;
            return;
        }
        if (isOverLandingZone && !needsBrake)
        {
            optimizedRotation = 0;
            var safeDescentSpeed = 30.0 + Math.Min(9.0, Math.Max(0.0, heightAboveLanding) * 0.018);
            optimizedPower = descentSpeed > safeDescentSpeed ? 4 : 3;
            return;
        }
        optimizedRotation = rotation;
        if ((isOverLandingZone || clearance > 600.0) && !needsBrake && !needsAcceleration && (isOverLandingZone || progressSpeed >= cruiseSpeed * 0.85))
        {
            optimizedRotation = 0;
        }
        else if (clearance > 800.0 && !needsBrake && needsAcceleration && progressSpeed >= cruiseSpeed * 0.65)
        {
            optimizedRotation = Math.Max(-15, Math.Min(15, rotation));
        }
        optimizedPower = power;
        if (optimizedRotation == 0 && clearance > 750.0)
        {
            var targetDescentSpeed = 38.0 + Math.Min(14.0, clearance * 0.012);
            if (descentSpeed < targetDescentSpeed - 7.0 && power > 3)
            {
                optimizedPower = 3;
            }
        }
    }
}

public class Ship
{
    public Point2d Location;
    public Point2d LastLocation;
    public Point2d VelocityVector;
    public int Fuel;
    public int RotationAngle;
    public int Power;
    public Tuple<Point2d, Point2d> CrashPoint;
    public int CrashSegmentIndex;

    public Ship(int x, int y, int horizontalSpeed, int verticalSpeed, int fuel, int rotationAngle, int power)
    {
        Location = new Point2d(x, y);
        LastLocation = new Point2d(x, y);
        VelocityVector = new Point2d(horizontalSpeed, verticalSpeed);
        Fuel = fuel;
        RotationAngle = rotationAngle;
        Power = power;
        CrashSegmentIndex = -1;
    }

    private Ship(Ship ship)
    {
        Location = ship.Location.Clone();
        LastLocation = ship.LastLocation.Clone();
        VelocityVector = ship.VelocityVector.Clone();
        Fuel = ship.Fuel;
        RotationAngle = ship.RotationAngle;
        Power = ship.Power;
        CrashPoint = ship.CrashPoint;
        CrashSegmentIndex = ship.CrashSegmentIndex;
    }

    public void AdvanceTurn()
    {
        LastLocation.Fill(Location);
        if (Fuel < Power)
        {
            Power = Fuel;
        }
        var sin = PhysicsConstants.SinTable[RotationAngle + 90];
        var cos = PhysicsConstants.CosTable[RotationAngle + 90];
        var accelerationX = -sin * Power;
        var accelerationY = cos * Power + PhysicsConstants.Gravity;
        var nextVelocityX = VelocityVector.x + accelerationX;
        var nextVelocityY = VelocityVector.y + accelerationY;
        Location.x += (VelocityVector.x + nextVelocityX) * 0.5;
        Location.y += (VelocityVector.y + nextVelocityY) * 0.5;
        VelocityVector.x = nextVelocityX;
        VelocityVector.y = nextVelocityY;
        Fuel -= Power;
    }

    public void SetPower(int newPower)
    {
        if (newPower > Power)
        {
            Power++;
        }
        else if (newPower < Power)
        {
            Power--;
        }
        Power = Math.Max(0, Math.Min(Power, 4));
    }

    public void SetRotation(int rotation)
    {
        var change = rotation - RotationAngle;
        if (change > 15)
        {
            change = 15;
        }
        else if (change < -15)
        {
            change = -15;
        }
        RotationAngle = Math.Max(-90, Math.Min(RotationAngle + change, 90));
    }

    public Ship Clone() => new Ship(this);

    public void Fill(Ship ship)
    {
        Location.Fill(ship.Location);
        LastLocation.Fill(ship.LastLocation);
        VelocityVector.Fill(ship.VelocityVector);
        Fuel = ship.Fuel;
        RotationAngle = ship.RotationAngle;
        Power = ship.Power;
        CrashPoint = ship.CrashPoint;
        CrashSegmentIndex = ship.CrashSegmentIndex;
    }

    public override string ToString() => "l:" + Location + ", v:" + VelocityVector + ", f:" + Fuel + ", r:" + RotationAngle + ", p:" + Power;
}

public class GameState : IGameState
{
    public Board Board;
    public Ship Ship;

    public GameState(Board board)
    {
        Board = board;
    }

    private GameState(GameState state)
    {
        Board = state.Board;
        Ship = state.Ship.Clone();
    }

    public void SetShip(Ship ship)
    {
        Ship = ship;
        Board.ConfigureFlightProfile(ship);
    }

    public void ApplyMove(object move, bool isMax)
    {
        if (!isMax)
        {
            return;
        }
        var currentMove = (Move)move;
        var rotation = currentMove.Rotation;
        var power = currentMove.Power;
        if (move is StaticMove)
        {
            rotation = Math.Max(-90, Math.Min(90, Ship.RotationAngle + rotation));
            power = Math.Max(0, Math.Min(4, Ship.Power + power));
        }
        if (!Board.UseHighGroundFlightProfile)
        {
            FlightCommandOptimizer.Optimize(Ship, Board, rotation, power, out rotation, out power);
        }
        Ship.SetRotation(rotation);
        Ship.SetPower(power);
        Ship.AdvanceTurn();
    }

    public IGameState Clone() => new GameState(this);

    public void Fill(GameState state)
    {
        Board = state.Board;
        Ship.Fill(state.Ship);
    }

    public double Evaluate(bool isMax)
    {
        var landingSpot = Board.GetLandingSpot();
        var midpoint = landingSpot.Item1.GetMidPoint(landingSpot.Item2);
        var horizontalSpeed = Math.Abs(Ship.VelocityVector.x);
        var verticalVelocity = Ship.VelocityVector.y;
        var descentSpeed = Math.Max(0, -verticalVelocity);
        var climbSpeed = Math.Max(0, verticalVelocity);
        var isOverLandingZone = Ship.Location.x >= landingSpot.Item1.x && Ship.Location.x <= landingSpot.Item2.x;
        if (isOverLandingZone)
        {
            var altitude = Ship.Location.y - landingSpot.Item1.y;
            var targetDescentSpeed = 32.0 + Math.Min(18.0, altitude * 0.018);
            var safeDescentSpeed = 30.0 + Math.Min(9.0, altitude * 0.018);
            var angleScore = 1.0 - Math.Abs(Ship.RotationAngle) / 90.0;
            var horizontalSpeedScore = Math.Max(0, 1.0 - Math.Max(0, horizontalSpeed - 19.0) / 81.0);
            var verticalSpeedScore = Math.Max(0, 1.0 - Math.Abs(descentSpeed - targetDescentSpeed) / 35.0);
            var fuelScore = Ship.Fuel * 0.35;
            var score = 10000 + angleScore * 60 + horizontalSpeedScore * 500 + verticalSpeedScore * 500 + fuelScore - climbSpeed * 15;
            if (altitude < 500 && descentSpeed > safeDescentSpeed)
            {
                score -= (descentSpeed - safeDescentSpeed) * 50;
            }
            return score;
        }
        var distanceToLandingZone = Math.Abs(Ship.Location.x - midpoint.x);
        var allowedHorizontalSpeed = Math.Min(45.0, 19.0 + distanceToLandingZone * 0.03);
        var excessHorizontalSpeed = Math.Max(0, horizontalSpeed - allowedHorizontalSpeed);
        var horizontalSpeedPenalty = excessHorizontalSpeed * excessHorizontalSpeed * 0.2;
        var verticalSpeedPenalty = Math.Max(0, descentSpeed - 60) * 10 + climbSpeed * 2;
        var anglePenalty = 0.0;
        if (!Board.UseHighGroundFlightProfile)
        {
            var direction = midpoint.x > Ship.Location.x + 30 ? 1 : midpoint.x < Ship.Location.x - 30 ? -1 : 0;
            var progressSpeed = direction * Ship.VelocityVector.x;
            var stoppingDistance = Ship.VelocityVector.x * Ship.VelocityVector.x / 5.5;
            var cruiseSpeed = Math.Min(42.0, 15.0 + distanceToLandingZone * 0.018);
            var needsBrake = horizontalSpeed > 16.0 && (direction == 0 || progressSpeed <= 0 || stoppingDistance + 180.0 >= distanceToLandingZone);
            var needsAcceleration = direction != 0 && distanceToLandingZone > 450.0 && progressSpeed < cruiseSpeed && stoppingDistance + 240.0 < distanceToLandingZone;
            var x = Math.Max(0, Math.Min(6999, Ship.Location.GetTruncatedX()));
            var clearance = Ship.Location.y - Board.MaxYAtX[x];
            if (clearance > 600.0 && !needsBrake && !needsAcceleration)
            {
                anglePenalty = Math.Abs(Ship.RotationAngle) * 4.0;
            }
            else if (clearance > 800.0 && !needsBrake && progressSpeed >= cruiseSpeed * 0.65)
            {
                anglePenalty = Math.Max(0, Math.Abs(Ship.RotationAngle) - 15) * 4.0;
            }
        }
        var fuelScoreOutsideLandingZone = Ship.Fuel * 0.12;
        var outsideScore = 5000 - distanceToLandingZone * 0.2 - horizontalSpeedPenalty - verticalSpeedPenalty - anglePenalty + fuelScoreOutsideLandingZone;
        return Math.Max(0, outsideScore);
    }

    public double? GetWinner()
    {
        if (!Board.IsInBounds(Ship))
        {
            return Evaluate(true);
        }
        var x = Ship.Location.GetTruncatedX();
        var previousX = Ship.LastLocation.GetTruncatedX();
        if (x >= 0 && x < 7000 && previousX >= 0 && previousX < 7000)
        {
            var maxY = Math.Max(Board.MaxYAtX[x], Board.MaxYAtX[previousX]);
            if (Ship.Location.y > maxY + 100)
            {
                return null;
            }
        }
        var collision = Board.ShipCollision(Ship);
        if (collision.HasValue)
        {
            return collision.Value ? Evaluate(true) : Ship.Fuel + 100000.0;
        }
        return null;
    }
}
