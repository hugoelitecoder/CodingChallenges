using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
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

        var tspSolver = new TspSolver(n, points, stopwatch);
        var result = tspSolver.Solve();
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

public class TspResult
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

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static double Distance(Point p1, Point p2)
    {
        var dx = (long)p1.X - p2.X;
        var dy = (long)p1.Y - p2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

public class TspSolver
{
    private readonly int _n;
    private readonly double[,] _distances;
    private readonly Stopwatch _stopwatch;
    private readonly Random _random;
    private const int TimeLimitMs = 4990; 
    private const double StartTemperature = 100.0;
    private const double CoolingRate = 0.99;

    public TspSolver(int n, Point[] points, Stopwatch stopwatch)
    {
        _n = n;
        _stopwatch = stopwatch;
        _random = new Random();
        _distances = new double[n, n];
        PrecomputeDistances(points);
    }
    
    public TspResult Solve()
    {
        var initialPath = GenerateInitialTour();
        var initialEnergy = CalculateTourLength(initialPath);

        var (finalPath, finalEnergy, iterations) = RunSimulatedAnnealing(initialPath, initialEnergy);
        
        var normalizedPath = NormalizePathToStartWithZero(finalPath);

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
        var path = new List<int>(_n);
        var visited = new bool[_n];
        var current = 0;
        path.Add(current);
        visited[current] = true;
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
            path.Add(current);
            visited[current] = true;
        }
        return path.ToArray();
    }

    private double CalculateTourLength(int[] path)
    {
        var length = 0.0;
        for (var i = 0; i < _n - 1; i++)
        {
            length += _distances[path[i], path[i + 1]];
        }
        length += _distances[path[_n - 1], path[0]];
        return length;
    }
    
    private (int[] path, double energy, long iterations) RunSimulatedAnnealing(int[] initialPath, double initialEnergy)
    {
        var currentPath = (int[])initialPath.Clone();
        var bestPath = (int[])initialPath.Clone();
        var currentEnergy = initialEnergy;
        var bestEnergy = initialEnergy;
        var temperature = StartTemperature;
        long iterations = 0;
        
        while (_stopwatch.ElapsedMilliseconds < TimeLimitMs)
        {
            iterations++;
            var i = _random.Next(0, _n);
            var j = _random.Next(0, _n);
            if (i == j) continue;
            if (i > j) { var temp = i; i = j; j = temp; }
            var p_i_prev = currentPath[(i + _n - 1) % _n];
            var p_i = currentPath[i];
            var p_j = currentPath[j];
            var p_j_next = currentPath[(j + 1) % _n];
            
            var delta = (_distances[p_i_prev, p_j] + _distances[p_i, p_j_next])
                      - (_distances[p_i_prev, p_i] + _distances[p_j, p_j_next]);

            if (delta < 0 || _random.NextDouble() < Math.Exp(-delta / temperature))
            {
                Array.Reverse(currentPath, i, j - i + 1);
                currentEnergy += delta;
                if (currentEnergy < bestEnergy)
                {
                    bestEnergy = currentEnergy;
                    Array.Copy(currentPath, bestPath, _n);
                }
            }
            temperature *= CoolingRate;
        }
        return (bestPath, bestEnergy, iterations);
    }

    private int[] NormalizePathToStartWithZero(int[] path)
    {
        var zeroIndex = Array.IndexOf(path, 0);
        var normalizedPath = new int[_n];
        for (var i = 0; i < _n; i++)
        {
            normalizedPath[i] = path[(zeroIndex + i) % _n];
        }
        return normalizedPath;
    }
}