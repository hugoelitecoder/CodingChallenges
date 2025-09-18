using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class Solution
{
    public static void Main(string[] args)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var n = int.Parse(Console.ReadLine());
        var points = new Point[n];
        for (var i = 0; i < n; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            points[i] = new Point(x, y);
        }

        var tspSolver = new TspSolver(n, points);
        var result = tspSolver.Solve(() => stopwatch.ElapsedMilliseconds >= 4950);
        stopwatch.Stop();

        Console.Error.WriteLine($"[DEBUG] Solving TSP for {n} points.");
        Console.Error.WriteLine($"[DEBUG] Algorithm: Simulated Annealing.");
        Console.Error.WriteLine($"[DEBUG] Initial (NN) Tour Length: {result.InitialEnergy:F2}.");
        Console.Error.WriteLine($"[DEBUG] SA Start Temp: {result.StartTemperature:F2}, Cooling: {result.CoolingRate}");
        Console.Error.WriteLine($"[DEBUG] Optimization complete.");
        Console.Error.WriteLine($"[DEBUG] Final Tour Length: {result.FinalEnergy:F2}.");
        if (result.InitialEnergy > 0)
        {
            var improvement = (result.InitialEnergy - result.FinalEnergy) / result.InitialEnergy;
            Console.Error.WriteLine($"[DEBUG] Improvement: {improvement:P2}.");
        }
        Console.Error.WriteLine($"[DEBUG] SA Iterations: {result.Iterations}.");

        Console.WriteLine(string.Join(" ", result.Path));

        Console.Error.WriteLine($"[DEBUG] Path found. Tour contains {result.Path.Length} points.");
        Console.Error.WriteLine($"[DEBUG] Total execution time: {stopwatch.ElapsedMilliseconds} ms");
    }
}

public sealed class TspResult
{
    public int[] Path { get; set; }
    public double InitialEnergy { get; set; }
    public double FinalEnergy { get; set; }
    public long Iterations { get; set; }
    public double StartTemperature { get; set; }
    public double CoolingRate { get; set; }
}

public readonly struct Point
{
    public readonly int X;
    public readonly int Y;

    public Point(int x, int y) { X = x; Y = y; }

    public static double Distance(Point p1, Point p2)
    {
        var dx = (long)p1.X - p2.X;
        var dy = (long)p1.Y - p2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

public interface IAnnealingProblem<TState> where TState : class
{
    double CalculateEnergy(TState state);
    (double energyDelta, Action commitChange) ProposeChange(TState state);
    TState CloneState(TState state);
    void CopyState(TState source, TState destination);
}

public sealed class SimulatedAnnealingSolver<TState> where TState : class
{
    private readonly IAnnealingProblem<TState> _problem;
    private readonly Random _random;
    private readonly double _initialTemperature;
    private readonly double _coolingRate;

    public SimulatedAnnealingSolver(IAnnealingProblem<TState> problem, double initialTemperature, double coolingRate)
    {
        _problem = problem;
        _initialTemperature = initialTemperature;
        _coolingRate = coolingRate;
        _random = new Random();
    }

    public (TState bestState, double bestEnergy, long iterations) Solve(TState initialState, Func<bool> shouldStop)
    {
        var currentState = _problem.CloneState(initialState);
        var bestState = _problem.CloneState(initialState);

        var currentEnergy = _problem.CalculateEnergy(currentState);
        var bestEnergy = currentEnergy;

        var temperature = _initialTemperature;
        long iterations = 0;

        while (!shouldStop())
        {
            iterations++;

            var (energyDelta, commitChange) = _problem.ProposeChange(currentState);

            if (energyDelta < 0 || (temperature > 1e-9 && _random.NextDouble() < Math.Exp(-energyDelta / temperature)))
            {
                commitChange();
                currentEnergy += energyDelta;

                if (currentEnergy < bestEnergy)
                {
                    bestEnergy = currentEnergy;
                    _problem.CopyState(currentState, bestState);
                }
            }

            temperature *= _coolingRate;
        }

        return (bestState, bestEnergy, iterations);
    }
}

public sealed class TspState
{
    public readonly int[] Path;
    public TspState(int[] path) => Path = path;
}

public sealed class TspProblem : IAnnealingProblem<TspState>
{
    private readonly int _n;
    private readonly double[,] _distances;
    private readonly Random _random;

    public TspProblem(int n, double[,] distances)
    {
        _n = n;
        _distances = distances;
        _random = new Random();
    }

    public double CalculateEnergy(TspState state)
    {
        var path = state.Path;
        var length = 0.0;
        for (var i = 0; i < _n - 1; i++)
        {
            length += _distances[path[i], path[i + 1]];
        }
        length += _distances[path[_n - 1], path[0]];
        return length;
    }

    public (double energyDelta, Action commitChange) ProposeChange(TspState state)
    {
        var path = state.Path;
        var i = _random.Next(0, _n);
        var j = _random.Next(0, _n);

        while (i == j) j = _random.Next(0, _n);

        if (i > j) (i, j) = (j, i);

        var p_i = path[i];
        var p_j = path[j];
        var p_i_prev = path[i == 0 ? _n - 1 : i - 1];
        var p_j_next = path[j == _n - 1 ? 0 : j + 1];

        var delta = (_distances[p_i_prev, p_j] + _distances[p_i, p_j_next])
                  - (_distances[p_i_prev, p_i] + _distances[p_j, p_j_next]);

        void CommitChange() => Array.Reverse(path, i, j - i + 1);

        return (delta, CommitChange);
    }

    public TspState CloneState(TspState state) => new TspState((int[])state.Path.Clone());
    public void CopyState(TspState source, TspState destination) => Array.Copy(source.Path, destination.Path, _n);
}

public sealed class TspSolver
{
    private readonly int _n;
    private readonly double[,] _distances;
    private const double StartTemperature = 100000.0;
    private const double CoolingRate = 0.99999;

    public TspSolver(int n, Point[] points)
    {
        _n = n;
        _distances = new double[n, n];
        PrecomputeDistances(points);
    }

    public TspResult Solve(Func<bool> shouldStop)
    {
        var initialTour = GenerateInitialTour();
        var tspProblem = new TspProblem(_n, _distances);
        var saSolver = new SimulatedAnnealingSolver<TspState>(tspProblem, StartTemperature, CoolingRate);

        var initialState = new TspState(initialTour);
        var initialEnergy = tspProblem.CalculateEnergy(initialState);

        var (finalState, finalEnergy, iterations) = saSolver.Solve(initialState, shouldStop);

        var normalizedPath = NormalizePathToStartWithZero(finalState.Path);

        var finalTour = new int[_n + 1];
        Array.Copy(normalizedPath, finalTour, _n);
        finalTour[_n] = 0;

        return new TspResult
        {
            Path = finalTour,
            InitialEnergy = initialEnergy,
            FinalEnergy = finalEnergy,
            Iterations = iterations,
            StartTemperature = StartTemperature,
            CoolingRate = CoolingRate
        };
    }

    private void PrecomputeDistances(Point[] points)
    {
        for (var i = 0; i < _n; i++)
        {
            for (var j = i; j < _n; j++)
            {
                var dist = Point.Distance(points[i], points[j]);
                _distances[i, j] = dist;
                _distances[j, i] = dist;
            }
        }
    }

    private int[] GenerateInitialTour()
    {
        var path = new int[_n];
        var visited = new bool[_n];
        path[0] = 0;
        visited[0] = true;
        var current = 0;

        for (var i = 1; i < _n; i++)
        {
            var nearest = -1;
            var minDist = double.MaxValue;
            for (var next = 0; next < _n; next++)
            {
                if (!visited[next])
                {
                    var dist = _distances[current, next];
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = next;
                    }
                }
            }
            current = nearest;
            path[i] = current;
            visited[current] = true;
        }
        return path;
    }

    private int[] NormalizePathToStartWithZero(int[] path)
    {
        var zeroIndex = Array.IndexOf(path, 0);
        if (zeroIndex == 0) return path;

        var normalizedPath = new int[_n];
        Array.Copy(path, zeroIndex, normalizedPath, 0, _n - zeroIndex);
        Array.Copy(path, 0, normalizedPath, _n - zeroIndex, zeroIndex);

        return normalizedPath;
    }
}