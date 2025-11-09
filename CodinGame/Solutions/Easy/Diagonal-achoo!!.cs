using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var n_str = Console.ReadLine();
        var g_str = Console.ReadLine();

        var n = int.Parse(n_str);
        var g = int.Parse(g_str);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var initialGrids = new List<char[,]>();
        for (int i = 0; i < g; i++)
        {
            var grid = new char[n, n];
            for (int j = 0; j < n; j++)
            {
                var row = Console.ReadLine();
                for (int k = 0; k < n; k++)
                {
                    grid[j, k] = row[k];
                }
            }
            initialGrids.Add(grid);
        }

        var maxInfected = -1;
        var bestGridIndex = -1;
        char[,] bestFinalGrid = null;

        for (int i = 0; i < g; i++)
        {
            var simulator = new VirusSimulator(initialGrids[i]);
            simulator.RunSimulation();
            
            var currentInfected = simulator.InfectedCount;

            if (currentInfected > maxInfected)
            {
                maxInfected = currentInfected;
                bestGridIndex = i;
                bestFinalGrid = simulator.Grid;
            }
        }
        
        stopwatch.Stop();
        
        Console.WriteLine(bestGridIndex);
        PrintGrid(bestFinalGrid, n);
        
        Console.Error.WriteLine($"[DEBUG] Grid size n: {n}");
        Console.Error.WriteLine($"[DEBUG] Number of grids g: {g}");
        Console.Error.WriteLine($"[DEBUG] Best grid index: {bestGridIndex}");
        Console.Error.WriteLine($"[DEBUG] Max infected count: {maxInfected}");
        Console.Error.WriteLine($"[DEBUG] Calculation time: {stopwatch.ElapsedMilliseconds} ms");
    }
    
    private static void PrintGrid(char[,] grid, int n)
    {
        if (grid == null) return;
        var sb = new StringBuilder();
        for (var r = 0; r < n; r++)
        {
            for (var c = 0; c < n; c++)
            {
                sb.Append(grid[r, c]);
            }
            if (r < n - 1)
            {
                sb.AppendLine();
            }
        }
        Console.WriteLine(sb.ToString());
    }
}

internal class VirusSimulator
{
    private readonly char[,] _grid;
    private readonly int _n;

    public int InfectedCount { get; private set; }
    public char[,] Grid => (char[,])_grid.Clone();

    public VirusSimulator(char[,] initialGrid)
    {
        _grid = (char[,])initialGrid.Clone();
        _n = _grid.GetLength(0);
        InfectedCount = 0;
    }

    public void RunSimulation()
    {
        while (true)
        {
            var contagious = new List<Point>();
            for (var r = 0; r < _n; r++)
            {
                for (var c = 0; c < _n; c++)
                {
                    if (_grid[r, c] == 'C')
                    {
                        contagious.Add(new Point(r, c));
                    }
                }
            }

            var toInfect = new HashSet<Point>();
            int[] dr = { -1, -1, 1, 1 };
            int[] dc = { -1, 1, -1, 1 };

            foreach (var p in contagious)
            {
                for (var i = 0; i < 4; i++)
                {
                    var nr = p.R + dr[i];
                    var nc = p.C + dc[i];

                    if (nr >= 0 && nr < _n && nc >= 0 && nc < _n && _grid[nr, nc] == '.')
                    {
                        toInfect.Add(new Point(nr, nc));
                    }
                }
            }

            if (toInfect.Count == 0)
            {
                break;
            }

            foreach (var p in toInfect)
            {
                _grid[p.R, p.C] = 'C';
            }
        }
        
        CalculateInfectedCount();
    }

    private void CalculateInfectedCount()
    {
        var count = 0;
        for (var r = 0; r < _n; r++)
        {
            for (var c = 0; c < _n; c++)
            {
                if (_grid[r, c] == 'C')
                {
                    count++;
                }
            }
        }
        InfectedCount = count;
    }
}

internal readonly struct Point : IEquatable<Point>
{
    public readonly int R;
    public readonly int C;

    public Point(int r, int c)
    {
        R = r;
        C = c;
    }

    public bool Equals(Point other)
    {
        return R == other.R && C == other.C;
    }

    public override bool Equals(object obj)
    {
        return obj is Point other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (R * 397) ^ C;
    }
}