using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        Console.Error.WriteLine($"[DEBUG] Number of vehicles: {n}");
        var initialVehicles = new List<Vehicle>(n);
        for (int i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            int id     = int.Parse(parts[0]);
            int x      = int.Parse(parts[1]);
            int y      = int.Parse(parts[2]);
            int length = int.Parse(parts[3]);
            char axis  = parts[4][0];
            Console.Error.WriteLine($"[DEBUG] Read vehicle #{i}: ID={id}, X={x}, Y={y}, Len={length}, Axis={axis}");
            initialVehicles.Add(new Vehicle(id, x, y, length, axis));
        }

        Console.Error.WriteLine("[DEBUG] Starting BFS solve...");
        var swTotal = Stopwatch.StartNew();
        var solution = SolverBFS.Solve(initialVehicles);
        swTotal.Stop();
        Console.Error.WriteLine($"[DEBUG] Solve completed in {swTotal.ElapsedMilliseconds} ms");
        int moveIndex = 0;

        if (solution.Count > 0)
        {
            var first = solution[moveIndex++];
            OutputMove(first, initialVehicles);
        }
        else
        {
            Console.Error.WriteLine("[DEBUG] No solution found—defaulting to 0 RIGHT");
            Console.WriteLine("0 RIGHT");
        }

        while (true)
        {
            for (int i = 0; i < n; i++)
            {
                var line = Console.ReadLine();
                Console.Error.WriteLine($"[DEBUG] Turn input #{i}: {line}");
            }

            CarMove nextMove = moveIndex < solution.Count
                ? solution[moveIndex++]
                : new CarMove(0, +1);

            Console.Error.WriteLine($"[DEBUG] Turn {moveIndex}: applying Car {nextMove.CarId} Delta {nextMove.Delta}");
            OutputMove(nextMove, initialVehicles);
        }
    }

    static void OutputMove(CarMove move, List<Vehicle> vehicles)
    {
        var v = vehicles.First(vh => vh.Id == move.CarId);
        string dir = v.Axis == 'H'
            ? (move.Delta > 0 ? "RIGHT" : "LEFT")
            : (move.Delta > 0 ? "DOWN"  : "UP");
        Console.Error.WriteLine($"[DEBUG] Output: {move.CarId} {dir}");
        Console.WriteLine($"{move.CarId} {dir}");
    }
}

public class Vehicle
{
    public int Id, X, Y, Length;
    public char Axis;
    public Vehicle(int id, int x, int y, int length, char axis)
    {
        Id = id; X = x; Y = y; Length = length; Axis = axis;
    }
}

public class CarMove
{
    public int CarId, Delta;
    public CarMove(int carId, int delta) { CarId = carId; Delta = delta; }
}

public static class SolverBFS
{
    public static List<CarMove> Solve(List<Vehicle> initVehicles)
    {
        int K = initVehicles.Count;
        var sorted = initVehicles.OrderBy(v => v.Id).ToArray();

        int[] ids        = new int[K];
        bool[] hor       = new bool[K];
        int[] length     = new int[K];
        int[] fixedCoord = new int[K];
        int redIdx = 0;
        for (int i = 0; i < K; i++)
        {
            var v = sorted[i];
            ids[i]        = v.Id;
            hor[i]        = v.Axis == 'H';
            length[i]     = v.Length;
            fixedCoord[i] = hor[i] ? v.Y : v.X;
            if (v.Id == 0) redIdx = i;
        }

        int[] startVars = new int[K];
        for (int i = 0; i < K; i++)
            startVars[i] = hor[i] ? sorted[i].X : sorted[i].Y;

        var sw = Stopwatch.StartNew();
        var visited = new HashSet<string>();
        var queue   = new Queue<(int[] vars, List<CarMove> path)>();

        void Enqueue(int[] vars, List<CarMove> path)
        {
            string key = string.Join(",", vars);
            if (visited.Add(key))
                queue.Enqueue((vars, path));
        }

        Enqueue(startVars, new List<CarMove>());

        long expansions = 0;
        while (queue.Count > 0)
        {
            var (vars, path) = queue.Dequeue();
            expansions++;
            if (expansions % 5000 == 0)
                Console.Error.WriteLine($"[DEBUG] Expansions: {expansions}, Queue size: {queue.Count}");

            if (vars[redIdx] == 4) {
                Console.Error.WriteLine($"[DEBUG] Solution found after {expansions} expansions, visited {visited.Count} unique states.");
                return path;
            }
                

            for (int i = 0; i < K; i++)
            {
                int pos = vars[i], f = fixedCoord[i], ln = length[i];
                if (hor[i])
                {
                    if (pos > 0 && IsFree(vars, i, pos - 1, f, hor, length, fixedCoord))
                        Enqueue(CloneAndMove(vars, i, -1), Append(path, ids[i], -1));
                    if (pos + ln < 6 && IsFree(vars, i, pos + ln, f, hor, length, fixedCoord))
                        Enqueue(CloneAndMove(vars, i, +1), Append(path, ids[i], +1));
                }
                else
                {
                    if (pos > 0 && IsFree(vars, i, f, pos - 1, hor, length, fixedCoord))
                        Enqueue(CloneAndMove(vars, i, -1), Append(path, ids[i], -1));
                    if (pos + ln < 6 && IsFree(vars, i, f, pos + ln, hor, length, fixedCoord))
                        Enqueue(CloneAndMove(vars, i, +1), Append(path, ids[i], +1));
                }
            }
        }

        Console.Error.WriteLine($"[DEBUG] No solution found after {expansions} expansions");
        return new List<CarMove>();
    }

    static int[] CloneAndMove(int[] vars, int idx, int delta)
    {
        var nv = (int[])vars.Clone();
        nv[idx] += delta;
        return nv;
    }

    static List<CarMove> Append(List<CarMove> path, int carId, int delta)
    {
        var np = new List<CarMove>(path) { new CarMove(carId, delta) };
        return np;
    }

    static bool IsFree(int[] vars, int idx, int nx, int ny, bool[] hor, int[] length, int[] fixedCoord)
    {
        for (int j = 0; j < vars.Length; j++)
        {
            int p = vars[j], f = fixedCoord[j];
            if (hor[j] ? (ny == f && nx >= p && nx < p + length[j])
                      : (nx == f && ny >= p && ny < p + length[j]))
                return false;
        }
        return true;
    }
}
