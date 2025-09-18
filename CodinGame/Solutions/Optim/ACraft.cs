using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public enum Dir { U, R, D, L }

public readonly record struct Point(int X, int Y);

public class Robot
{
    public Point Pos;
    public Dir Direction;
    public bool Active;
    public readonly bool[] History;

    private const int GridWidth = 19;
    private const int GridHeight = 10;
    private static readonly int HistorySize = GridWidth * GridHeight * 4;

    public Robot(int x, int y, string direction)
    {
        Pos = new Point(x, y);
        Direction = Player.CharToDir(direction[0]);
        Active = true;
        History = new bool[HistorySize];
    }

    private Robot(Point pos, Dir direction, bool active, bool[] history)
    {
        Pos = pos;
        Direction = direction;
        Active = active;
        History = history;
    }

    public Robot Clone()
    {
        var historyClone = new bool[HistorySize];
        Array.Copy(History, historyClone, HistorySize);
        return new Robot(Pos, Direction, Active, historyClone);
    }

    public static int GetHistoryIndex(Point p, Dir d)
    {
        return (int)d * (GridWidth * GridHeight) + p.Y * GridWidth + p.X;
    }
}

public class Plan
{
    public Dictionary<Point, Dir> Arrows;
    public int Score;

    public Plan(Dictionary<Point, Dir> arrows)
    {
        Arrows = arrows;
        Score = 0;
    }

    public Plan Clone()
    {
        return new Plan(new Dictionary<Point, Dir>(Arrows));
    }
}

public class Player
{
    private const int Width = 19;
    private const int Height = 10;
    private const int MaxTurns = 400;
    private const long TimeLimitMs = 960;
    private const int RestartThreshold = 2500;
    private const double InitialTemperature = 10000;
    
    private static char[,] _grid;
    private static List<Robot> _initialRobots;
    private static List<Point> _placeableCells;
    private static readonly Random _random = new Random();
    private static readonly (int dx, int dy)[] _dirVectors = { (0, -1), (1, 0), (0, 1), (-1, 0) };

    public static void Main(string[] args)
    {
        var watch = Stopwatch.StartNew();
        ReadInput();
        var bestPlan = FindBestPlan();
        PrintOutput(bestPlan);
        watch.Stop();
        Console.Error.WriteLine($"[DEBUG] Total time: {watch.ElapsedMilliseconds}ms");
    }

    public static Plan FindBestPlan()
    {
        var mainWatch = Stopwatch.StartNew();
        var currentPlan = new Plan(new Dictionary<Point, Dir>());
        Simulate(currentPlan);
        var bestPlan = currentPlan.Clone();
        double temp = InitialTemperature;
        const double coolingRate = 0.9995;
        var iterations = 0;
        var iterationsWithoutImprovement = 0;
        while (mainWatch.ElapsedMilliseconds < TimeLimitMs)
        {
            var newPlan = Mutate(currentPlan);
            Simulate(newPlan);
            var deltaScore = newPlan.Score - currentPlan.Score;
            if (deltaScore > 0 || _random.NextDouble() < Math.Exp(deltaScore / temp))
            {
                currentPlan = newPlan;
            }
            if (currentPlan.Score > bestPlan.Score)
            {
                bestPlan = currentPlan.Clone();
                iterationsWithoutImprovement = 0;
            }
            else
            {
                iterationsWithoutImprovement++;
            }
            temp *= coolingRate;
            if (temp < 1) temp = 1;
            if (iterationsWithoutImprovement > RestartThreshold)
            {
                currentPlan = bestPlan.Clone();
                temp = InitialTemperature;
                iterationsWithoutImprovement = 0;
            }
            iterations++;
        }
        mainWatch.Stop();
        Console.Error.WriteLine($"[DEBUG] SA Iterations: {iterations}, Time: {mainWatch.ElapsedMilliseconds}ms");
        Console.Error.WriteLine($"[DEBUG] Best Score: {bestPlan.Score}");
        return bestPlan;
    }
    
    private static void ReadInput()
    {
        _grid = new char[Height, Width];
        _placeableCells = new List<Point>();
        for (var i = 0; i < Height; i++)
        {
            var line = Console.ReadLine();
            for (var j = 0; j < Width; j++)
            {
                _grid[i, j] = line[j];
                if (line[j] == '.')
                {
                    _placeableCells.Add(new Point(j, i));
                }
            }
        }
        var robotCount = int.Parse(Console.ReadLine());
        _initialRobots = new List<Robot>(robotCount);
        for (var i = 0; i < robotCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            var direction = inputs[2];
            _initialRobots.Add(new Robot(x, y, direction));
        }
        Console.Error.WriteLine($"[DEBUG] Input: {robotCount} robots, {_placeableCells.Count} placeable cells");
    }

    private static void PrintOutput(Plan plan)
    {
        if (plan.Arrows.Count == 0)
        {
            Console.WriteLine("WAIT");
            return;
        }
        var sb = new StringBuilder();
        foreach (var arrow in plan.Arrows)
        {
            sb.Append($"{arrow.Key.X} {arrow.Key.Y} {DirToChar(arrow.Value)} ");
        }
        Console.WriteLine(sb.ToString().TrimEnd());
    }

    private static Plan Mutate(Plan plan)
    {
        var newPlan = plan.Clone();
        if (_placeableCells.Count == 0) return newPlan;
        var cellToChange = _placeableCells[_random.Next(_placeableCells.Count)];
        if (newPlan.Arrows.ContainsKey(cellToChange) && _random.Next(2) == 0)
        {
            newPlan.Arrows.Remove(cellToChange);
            return newPlan;
        }
        var validDirs = new List<Dir>(4);
        var allDirs = (Dir[])Enum.GetValues(typeof(Dir));
        foreach (var dir in allDirs)
        {
            var (dx, dy) = _dirVectors[(int)dir];
            var nextX = (cellToChange.X + dx + Width) % Width;
            var nextY = (cellToChange.Y + dy + Height) % Height;
            var nextPos = new Point(nextX, nextY);
            if (_grid[nextY, nextX] == '#') continue;
            var oppositeDir = (Dir)(((int)dir + 2) % 4);
            var neighborChar = _grid[nextY, nextX];
            if (neighborChar != '.' && neighborChar != '#' && CharToDir(neighborChar) == oppositeDir) continue;
            if (plan.Arrows.TryGetValue(nextPos, out var neighborArrowDir) && neighborArrowDir == oppositeDir) continue;
            validDirs.Add(dir);
        }
        if (validDirs.Count > 0)
        {
            newPlan.Arrows[cellToChange] = validDirs[_random.Next(validDirs.Count)];
        }
        else
        {
            newPlan.Arrows.Remove(cellToChange);
        }
        return newPlan;
    }

    private static void Simulate(Plan plan)
    {
        var robots = new List<Robot>(_initialRobots.Count);
        foreach(var r in _initialRobots) { robots.Add(r.Clone()); }
        
        foreach (var r in robots)
        {
            if (plan.Arrows.TryGetValue(r.Pos, out var newDir)) r.Direction = newDir;
            r.History[Robot.GetHistoryIndex(r.Pos, r.Direction)] = true;
        }
        
        var score = 0;
        var activeCount = robots.Count;
        for (var turn = 0; turn < MaxTurns && activeCount > 0; turn++)
        {
            score += activeCount;
            foreach (var r in robots)
            {
                if (!r.Active) continue;
                var (dx, dy) = _dirVectors[(int)r.Direction];
                r.Pos = new Point((r.Pos.X + dx + Width) % Width, (r.Pos.Y + dy + Height) % Height);
            }
            var currentActive = 0;
            foreach (var r in robots)
            {
                if (!r.Active) continue;
                if (_grid[r.Pos.Y, r.Pos.X] == '#')
                {
                    r.Active = false;
                    continue;
                }
                if (plan.Arrows.TryGetValue(r.Pos, out var arrowDir))
                {
                    r.Direction = arrowDir;
                }
                else
                {
                    var cellChar = _grid[r.Pos.Y, r.Pos.X];
                    if (cellChar != '.' && cellChar != '#') r.Direction = CharToDir(cellChar);
                }
                var historyIndex = Robot.GetHistoryIndex(r.Pos, r.Direction);
                if (r.History[historyIndex])
                {
                    r.Active = false;
                    continue;
                }
                r.History[historyIndex] = true;
                currentActive++;
            }
            activeCount = currentActive;
        }
        plan.Score = score;
    }

    public static Dir CharToDir(char c)
    {
        return c switch { 'U' => Dir.U, 'R' => Dir.R, 'D' => Dir.D, 'L' => Dir.L, _ => throw new ArgumentException() };
    }

    private static char DirToChar(Dir d)
    {
        return d switch { Dir.U => 'U', Dir.R => 'R', Dir.D => 'D', Dir.L => 'L', _ => ' ' };
    }
}