using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Player
{
    static void Main(string[] args)
    {
        var io = new IO();
        int surfaceN = int.Parse(io.ReadLine());
        IList<Point2d> points = new List<Point2d>();
        for (int i = 0; i < surfaceN; i++)
        {
            var inputs = io.ReadLine().Split(' ');
            int landX = int.Parse(inputs[0]);
            int landY = int.Parse(inputs[1]);
            points.Add(new Point2d(landX, landY));
        }
        Board board = new Board(points);
        io.Debug($"landing spot: {board.GetLandingSpot()}");
        GameState state = new GameState(board);
        Population population = new Population();
        bool isFirstTurn = true;
        while (true)
        {
            var inputs = io.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]);
            int Y = int.Parse(inputs[1]);
            int hSpeed = int.Parse(inputs[2]);
            int vSpeed = int.Parse(inputs[3]);
            int fuel = int.Parse(inputs[4]);
            int rotate = int.Parse(inputs[5]);
            int power = int.Parse(inputs[6]);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Ship ship = new Ship(X, Y, hSpeed, vSpeed, fuel, rotate, power);
            if (isFirstTurn)
                state.SetShip(ship);
            var limit = isFirstTurn ? 995 : 95;
            if (isFirstTurn)
            {
                for (int i = 0; i < 100; i++)
                {
                    population.Add(new MarsLanderSolution(state));
                }
            }
            GeneticAlgorithm genetic = new GeneticAlgorithm(population, 0.01, 0.05, 0.2);
            Move move = (Move)genetic.GetNextMove(watch, limit);
            population = genetic.Population;
            io.Debug($"gen: {genetic.GenerationCounter}, move: {move}, score: {population.GetBestIndividual().Fitness}");
            state.ApplyMove(move, true);
            foreach (Individual i in population)
            {
                var solution = (MarsLanderSolution)i;
                solution.AdvanceTurn(state);
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
        public void Debug(string message) => Console.Error.WriteLine($"[DEBUG] {message}");
    }
}
public static class PhysicsConstants
{
    public static readonly Point2d GravityVector = new Point2d(0, -3.711);
    public static readonly double[] SinTable = new double[181];
    public static readonly double[] CosTable = new double[181];
    static PhysicsConstants()
    {
        for (var angle = -90; angle <= 90; ++angle)
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
    bool Equals(IGameState state);
    double Evaluate(bool isMax);
}
public class GeneticAlgorithm
{
    public Population Population;
    private Population _hiddenPopulation;
    private double _mutationRate;
    private double _elitePercent;
    private double _crossOver;
    public int GenerationCounter;
    public GeneticAlgorithm(Population initialPopulation, double mRate, double eP, double cO)
    {
        Population = initialPopulation;
        _hiddenPopulation = initialPopulation.Clone();
        _mutationRate = mRate;
        _elitePercent = eP;
        _crossOver = cO;
        GenerationCounter = 0;
    }
    public object GetNextMove(Stopwatch watch, int timeLimit)
    {
        do
        {
            var counter = 0;
            foreach (Individual i in Population)
            {
                if (watch.ElapsedMilliseconds >= timeLimit && counter > 1) break;
                if (i.Fitness == double.MinValue) i.CalculateFitness();
                counter++;
            }
            GenerateNextGeneration();
        }
        while (watch.ElapsedMilliseconds < timeLimit);
        return Population.GetBestIndividual().GetNextMove();
    }
    public void GenerateNextGeneration()
    {
        GenerationCounter++;
        Population.SortPopulationByFitness();
        for (int x = 0; x < (int)(Population.Count * _elitePercent); x++)
        {
            _hiddenPopulation[x] = Population[x];
        }
        double totalFit = Population.GetTotalFitness();
        for (int x = (int)(Population.Count * _elitePercent); x < Population.Count; x++)
        {
            var individual1 = Population.SelectRandomFromPopulation(totalFit);
            var individual2 = Population.SelectRandomFromPopulation(totalFit, individual1);
            var child = _hiddenPopulation[x].CreateBaby(individual1, individual2, _crossOver);
            child.Mutate(_mutationRate);
            child.Fitness = double.MinValue;
            x++;
            if (x < Population.Count)
            {
                child = _hiddenPopulation[x].CreateBaby(individual2, individual1, _crossOver);
                child.Mutate(_mutationRate);
                child.Fitness = double.MinValue;
            }
        }
        var swap = Population;
        Population = _hiddenPopulation;
        _hiddenPopulation = swap;
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
    private List<Individual> _individuals;
    private Random _rand;
    public int Count => _individuals.Count;
    public Individual this[int index] { get { return _individuals[index]; } set { _individuals[index] = value; } }
    public Population()
    {
        _individuals = new List<Individual>();
        _rand = new Random();
    }
    public Population(Population population)
    {
        _individuals = population._individuals.Select(i => i.Clone()).ToList();
        _rand = new Random();
    }
    public void SortPopulationByFitness() => _individuals.Sort((i1, i2) => i2.Fitness.CompareTo(i1.Fitness));
    public double GetTotalFitness() => _individuals.Sum(i => i.Fitness);
    public Individual SelectRandomFromPopulation(double totalFit, Individual i = null)
    {
        double randNum = _rand.NextDouble() * totalFit;
        int y = 0;
        if (_individuals.Count == 0) return null;
        double totalFitSoFar = _individuals[y].Fitness;
        while (totalFitSoFar < randNum && y < _individuals.Count - 1)
        {
            y++;
            totalFitSoFar += _individuals[y].Fitness;
        }
        if (i != null && i == _individuals[y] && _individuals.Count > 1)
        {
            y = (y == _individuals.Count - 1) ? y - 1 : y + 1;
        }
        return _individuals[y];
    }
    public Individual GetBestIndividual()
    {
        SortPopulationByFitness();
        return _individuals.First();
    }
    public void Add(Individual item) => _individuals.Add(item);
    public Population Clone() => new Population(this);
    public IEnumerator<Individual> GetEnumerator() => _individuals.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _individuals.GetEnumerator();
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
    public override string ToString() => $"({x},{y})";
    public int GetTruncatedX() => (int)x;
    public double GetDistance(Point2d point) => Math.Sqrt(Math.Pow(x - point.x, 2) + Math.Pow(y - point.y, 2));
    public Point2d GetMidPoint(Point2d point) => new Point2d((x + point.x) / 2, (y + point.y) / 2);
    public Point2d GetRoundedAwayFromZeroPoint() => new Point2d(Math.Round(x, MidpointRounding.AwayFromZero), Math.Round(y, MidpointRounding.AwayFromZero));
    public Point2d Clone() => new Point2d(x, y);
    public void Fill(Point2d point)
    {
        x = point.x;
        y = point.y;
    }
}
public class Board
{
    public IList<Point2d> Points;
    public Tuple<Point2d, Point2d> LandingSpot;
    public int[] MaxYAtX;
    public double[] pointIndexToDistanceFromLandingSpot;
    public Board(IList<Point2d> points)
    {
        Points = points;
        MaxYAtX = new int[7000];
        Point2d lastPoint = null;
        foreach (Point2d currentPoint in points)
        {
            if (lastPoint != null)
            {
                var slope = (lastPoint.y - currentPoint.y) / (lastPoint.x - currentPoint.x);
                var yIntercept = currentPoint.y - (slope * currentPoint.x);
                for (int x = lastPoint.GetTruncatedX(); x < currentPoint.x; x++)
                {
                    if (x >= 0 && x < 7000)
                        MaxYAtX[x] = (int)Math.Ceiling(slope * x + yIntercept);
                }
            }
            lastPoint = currentPoint;
        }
        var landingSpot = GetLandingSpot();
        pointIndexToDistanceFromLandingSpot = new double[points.Count];
        double distance = 0;
        lastPoint = null;
        bool foundLandingSpot = false;
        for (int i = 0; i < points.Count; i++)
        {
            var currentPoint = points[i];
            if (lastPoint != null)
            {
                var legDistance = lastPoint.GetDistance(currentPoint);
                if (!foundLandingSpot)
                {
                    for (int j = 0; j < i; j++) pointIndexToDistanceFromLandingSpot[j] += legDistance;
                }
                else
                {
                    distance += legDistance;
                    pointIndexToDistanceFromLandingSpot[i] = distance;
                }
            }
            if (currentPoint == landingSpot.Item1)
            {
                foundLandingSpot = true;
                distance = 0;
                pointIndexToDistanceFromLandingSpot[i] = distance;
            }
            lastPoint = currentPoint;
        }
    }
    public Tuple<Point2d, Point2d> GetLandingSpot()
    {
        if (LandingSpot == null)
        {
            Point2d lastPoint = null;
            foreach (var point in Points)
            {
                if (lastPoint != null && point.y == lastPoint.y)
                {
                    LandingSpot = new Tuple<Point2d, Point2d>(lastPoint, point);
                    break;
                }
                lastPoint = point;
            }
        }
        return LandingSpot;
    }
    public bool ShipLanded(Ship ship)
    {
        var landingSpot = GetLandingSpot();
        if (ship.LastLocation.y >= landingSpot.Item1.y && ship.Location.y <= landingSpot.Item1.y)
        {
            return ShipWithinVelocityForLanding(ship);
        }
        return false;
    }
    public bool ShipWithinVelocityForLanding(Ship ship) => Math.Abs(ship.VelocityVector.x) <= 19 && Math.Abs(ship.VelocityVector.y) <= 39 && ship.RotationAngle == 0;
    public bool IsInBounds(Ship ship)
    {
        if (ship.Location.x <= 0)
        {
            ship.CrashPoint = new Tuple<Point2d, Point2d>(Points[0], Points[1]);
            return false;
        }
        if (ship.Location.x >= 6999)
        {
            ship.CrashPoint = new Tuple<Point2d, Point2d>(Points[Points.Count - 2], Points[Points.Count - 1]);
            return false;
        }
        if (ship.Location.y <= 0 || ship.Location.y >= 2999)
        {
            ship.CrashPoint = null;
            return false;
        }
        return true;
    }
    private int FindPointIndexBeforeX(double x)
    {
        if (x < Points[0].x) return 0;
        if (x >= Points[Points.Count - 1].x) return Points.Count - 2;
        int low = 0, high = Points.Count - 1, result = 0;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (Points[mid].x <= x)
            {
                result = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        return result;
    }
    public bool? ShipCollision(Ship ship)
    {
        var minX = Math.Min(ship.Location.x, ship.LastLocation.x);
        var maxX = Math.Max(ship.Location.x, ship.LastLocation.x);
        int startIndex = FindPointIndexBeforeX(minX);
        int endIndex = FindPointIndexBeforeX(maxX);
        for (int i = startIndex; i <= endIndex && i < Points.Count - 1; i++)
        {
            var p1 = Points[i];
            var p2 = Points[i + 1];
            if (IsIntersecting(p1, p2, ship.Location, ship.LastLocation))
            {
                ship.CrashPoint = new Tuple<Point2d, Point2d>(p1, p2);
                var landingSpot = GetLandingSpot();
                if (landingSpot.Item1 == p1 && landingSpot.Item2 == p2)
                {
                    return !ShipLanded(ship);
                }
                return true;
            }
        }
        return null;
    }
    private bool IsIntersecting(Point2d a, Point2d b, Point2d c, Point2d d)
    {
        double denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
        if (denominator == 0) return false;
        double numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
        double numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));
        double r = numerator1 / denominator;
        double s = numerator2 / denominator;
        return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
    }
    public double CalculatePathDistance(Ship ship)
    {
        if (ship.CrashPoint == null) return 0;
        int p1Index = Points.IndexOf(ship.CrashPoint.Item1);
        if (p1Index < 0 || p1Index + 1 >= Points.Count) return 0;
        var distanceToPoint1 = pointIndexToDistanceFromLandingSpot[p1Index];
        var distanceToPoint2 = pointIndexToDistanceFromLandingSpot[p1Index + 1];
        if (distanceToPoint1 == 0 && distanceToPoint2 == 0) return 0;
        return Math.Min(ship.Location.GetDistance(ship.CrashPoint.Item1) + distanceToPoint1, ship.Location.GetDistance(ship.CrashPoint.Item2) + distanceToPoint2);
    }
}
public class MarsLanderSolution : Individual
{
    public StaticMove[] Moves;
    private Random _rand;
    private static int TotalMoves = 160;
    public double Fitness { get; set; }
    public GameState State;
    public int Turn = 0;
    public MarsLanderSolution(GameState state)
    {
        Fitness = double.MinValue;
        _rand = new Random();
        State = (GameState)state.Clone();
        Moves = new StaticMove[TotalMoves];
        for (int i = 0; i < TotalMoves; i++) Moves[i] = CreateRandomMove();
    }
    public MarsLanderSolution(MarsLanderSolution solution)
    {
        Fitness = solution.Fitness;
        _rand = new Random();
        State = (GameState)solution.State.Clone();
        Moves = solution.Moves.Select(m => (StaticMove)m.Clone()).ToArray();
        Turn = solution.Turn;
    }
    public double CalculateFitness()
    {
        var clonedState = State.Clone();
        double? winner;
        int counter = Turn;
        do
        {
            clonedState.ApplyMove(Moves[counter++], true);
            winner = clonedState.GetWinner();
        }
        while (winner == null && counter < TotalMoves);

        if (winner == null)
        {
            Fitness = clonedState.Evaluate(true);
        }
        else
        {
            Fitness = winner.Value;
        }
        return Fitness;
    }
    public StaticMove CreateRandomMove(StaticMove move = null)
    {
        if (move == null) return new StaticMove(_rand.Next(-15, 16), _rand.Next(-1, 2));
        move.Rotation = _rand.Next(-15, 16);
        move.Power = _rand.Next(-1, 2);
        return move;
    }
    public void AdvanceTurn(GameState updatedState)
    {
        State = (GameState)updatedState.Clone();
        Turn++;
    }
    public Individual CreateBaby(Individual parent1, Individual parent2, double crossOver)
    {
        var parentA = (MarsLanderSolution)parent1;
        var parentB = (MarsLanderSolution)parent2;
        var randomNum = _rand.NextDouble();
        State.Fill(((MarsLanderSolution)parent1).State);
        for (int i = parentA.Turn; i < TotalMoves; i++)
        {
            var p1 = parentA.Moves[i];
            var p2 = parentB.Moves[i];
            Moves[i].Rotation = (int)Math.Round(randomNum * p1.Rotation + (1 - randomNum) * p2.Rotation);
            Moves[i].Power = (int)Math.Round(randomNum * p1.Power + (1 - randomNum) * p2.Power);
        }
        Turn = parentA.Turn;
        return this;
    }
    public void Mutate(double mutationRate)
    {
        for (int i = 0; i < TotalMoves * mutationRate; i++)
        {
            var index = _rand.Next(Turn, TotalMoves);
            Moves[index] = CreateRandomMove(Moves[index]);
        }
    }
    public object GetNextMove() => StaticMove.ConvertToMove(State.Ship, Moves[Turn]);
    public Individual Clone() => new MarsLanderSolution(this);
}
public class Move
{
    public int Rotation;
    public int Power;
    public Move(int rotate, int power)
    {
        Rotation = rotate;
        Power = power;
    }
    public override string ToString() => $"{Rotation} {Power}";
    public virtual Move Clone() => new Move(Rotation, Power);
}
public class StaticMove : Move
{
    public StaticMove(int rotate, int power) : base(rotate, power) { }
    public static Move ConvertToMove(Ship ship, StaticMove move) => new Move(Math.Min(Math.Max(ship.RotationAngle + move.Rotation, -90), 90), Math.Max(Math.Min(ship.Power + move.Power, 4), 0));
    public override Move Clone() => new StaticMove(Rotation, Power);
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
    public Ship(int x, int y, int hSpeed, int vSpeed, int fuel, int rotate, int power)
    {
        Location = new Point2d(x, y);
        LastLocation = new Point2d(x, y);
        VelocityVector = new Point2d(hSpeed, vSpeed);
        Fuel = fuel;
        RotationAngle = rotate;
        Power = power;
    }
    public Ship(Ship ship)
    {
        Location = ship.Location.Clone();
        LastLocation = ship.LastLocation.Clone();
        VelocityVector = ship.VelocityVector.Clone();
        Fuel = ship.Fuel;
        RotationAngle = ship.RotationAngle;
        Power = ship.Power;
    }
    public void AdvanceTurn()
    {
        LastLocation.Fill(Location);
        if (Fuel < Power) Power = Fuel;

        var sinRad = PhysicsConstants.SinTable[RotationAngle + 90];
        var cosRad = PhysicsConstants.CosTable[RotationAngle + 90];
        var thrustX = -sinRad * Power;
        var thrustY = cosRad * Power;

        var accelX = thrustX;
        var accelY = thrustY + PhysicsConstants.GravityVector.y;

        var newVx = VelocityVector.x + accelX;
        var newVy = VelocityVector.y + accelY;

        Location.x += (VelocityVector.x + newVx) * 0.5;
        Location.y += (VelocityVector.y + newVy) * 0.5;

        VelocityVector.x = newVx;
        VelocityVector.y = newVy;

        Fuel -= Power;
    }
    public void SetPower(int newPower)
    {
        var difference = newPower - Power;
        if (difference < 0) Power--;
        else if (difference > 0) Power++;
        Power = Math.Max(0, Math.Min(Power, 4));
    }
    public void SetRotation(int rotate)
    {
        var difference = rotate - RotationAngle;
        if (difference > 15) difference = 15;
        else if (difference < -15) difference = -15;
        RotationAngle = Math.Max(-90, Math.Min(RotationAngle + difference, 90));
    }
    public bool Equals(Ship ship) => ship.Location.GetRoundedAwayFromZeroPoint().Equals(Location.GetRoundedAwayFromZeroPoint()) && ship.VelocityVector.GetRoundedAwayFromZeroPoint().Equals(VelocityVector.GetRoundedAwayFromZeroPoint()) && ship.Fuel == Fuel && ship.RotationAngle == RotationAngle;
    public Ship Clone() => new Ship(this);
    public void Fill(Ship ship)
    {
        Location = ship.Location;
        VelocityVector = ship.VelocityVector;
        Fuel = ship.Fuel;
        RotationAngle = ship.RotationAngle;
        Power = ship.Power;
    }
    public override string ToString() => $"l:{Location}, v:{VelocityVector}, f:{Fuel}, r:{RotationAngle}, p:{Power}";
}
public class GameState : IGameState
{
    public Board Board;
    public Ship Ship;
    public GameState(Board board)
    {
        Board = board;
    }
    public GameState(GameState state)
    {
        Board = state.Board;
        Ship = state.Ship.Clone();
    }
    public void SetShip(Ship ship) => Ship = ship;
    public void ApplyMove(object move, bool isMax)
    {
        if (isMax)
        {
            Move m = (Move)move;
            if (move is StaticMove)
            {
                Ship.SetRotation(m.Rotation + Ship.RotationAngle);
                Ship.SetPower(m.Power + Ship.Power);
            }
            else
            {
                Ship.SetRotation(m.Rotation);
                Ship.SetPower(m.Power);
            }
            Ship.AdvanceTurn();
        }
    }
    public IGameState Clone() => new GameState(this);
    public void Fill(GameState state)
    {
        Board = state.Board;
        Ship.Fill(state.Ship);
    }
    public bool Equals(IGameState state)
    {
        GameState game = (GameState)state;
        return Ship.Equals(game.Ship);
    }
    public double Evaluate(bool isMax)
    {
        var landingSpot = Board.GetLandingSpot();
        var midPoint = landingSpot.Item1.GetMidPoint(landingSpot.Item2);

        double hSpeed = Math.Abs(Ship.VelocityVector.x);
        double vSpeed = Math.Abs(Ship.VelocityVector.y);
        bool isOverLandingZone = Ship.Location.x >= landingSpot.Item1.x && Ship.Location.x <= landingSpot.Item2.x;
        if (isOverLandingZone)
        {
            double altitude = Ship.Location.y - landingSpot.Item1.y;
            double angleScore = 1.0 - (Math.Abs(Ship.RotationAngle) / 90.0);
            double hSpeedScore = Math.Max(0, 1.0 - (hSpeed - 19) / 81.0);
            double vSpeedScore = Math.Max(0, 1.0 - (vSpeed - 39) / 81.0);
            double finalScore = 10000 + (angleScore * 60) + (hSpeedScore * 500) + (vSpeedScore * 500);

            if (altitude < 500 && vSpeed > 30)
            {
                finalScore -= (vSpeed - 30) * 50;
            }

            return finalScore;
        }
        else
        {
            double MAX_SCORE = 5000;

            double distPenalty = Math.Abs(Ship.Location.x - midPoint.x) * 0.2;
            double hSpeedPenalty = hSpeed * hSpeed * 0.2;
            double vSpeedPenalty = Math.Max(0, vSpeed - 60) * 10;

            double score = MAX_SCORE - distPenalty - hSpeedPenalty - vSpeedPenalty;
            return Math.Max(0, score);
        }
    }

    public double? GetWinner()
    {
        if (!Board.IsInBounds(Ship)) return Evaluate(true);
        var truncX = Ship.Location.GetTruncatedX();
        var lastTruncX = Ship.LastLocation.GetTruncatedX();
        if (truncX >= 0 && truncX < 7000 && lastTruncX >= 0 && lastTruncX < 7000)
        {
            var maxY = Math.Max(Board.MaxYAtX[truncX], Board.MaxYAtX[lastTruncX]);
            if (Ship.Location.y > (maxY + 100)) return null;
        }
        var shipCollision = Board.ShipCollision(Ship);
        if (shipCollision.HasValue)
        {
            return shipCollision.Value ? Evaluate(true) : (double)(Ship.Fuel + 100000);
        }
        return null;
    }
}