using System;
using System.Collections.Generic;
using System.Diagnostics;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var w = int.Parse(inputs[0]);
        var h = int.Parse(inputs[1]);
        var gridRows = new string[h];

        Console.Error.WriteLine($"Grid dimensions: {w}x{h}");
        Console.Error.WriteLine("Input Grid:");
        for (var i = 0; i < h; i++)
        {
            gridRows[i] = Console.ReadLine();
            Console.Error.WriteLine(gridRows[i]);
        }
        Console.Error.WriteLine("--------------------");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var solver = new MazeSolver(w, h, gridRows);
        var path = solver.Solve();

        stopwatch.Stop();
        
        Console.Error.WriteLine($"Generated Output: {path}");
        solver.PrintStats();
        Console.Error.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.Error.WriteLine("====================");

        Console.WriteLine(path);
    }
}

class MazeSolver
{
    private readonly int _w;
    private readonly int _h;
    private readonly char[][] _grid;
    private readonly Point _startPos;
    private readonly Point _endPos;
    private int _visitedStatesCount;
    private int _maxQueueSize;

    private static readonly string[] DirNames = { "UL", "UR", "R", "DR", "DL", "L" };
    private static readonly (int dx, int dy)[][] Offsets = {
        new[] { (-1, -1), (0, -1), (1, 0), (0, 1), (-1, 1), (-1, 0) },
        new[] { (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 0) }
    };

    private record Point(int X, int Y);
    private record State(Point Pos, int Keys);

    public MazeSolver(int w, int h, string[] gridRows)
    {
        _w = w;
        _h = h;
        _grid = new char[h][];
        for (var i = 0; i < h; i++)
        {
            _grid[i] = gridRows[i].ToCharArray();
            for (var j = 0; j < w; j++)
            {
                if (_grid[i][j] == 'S')
                {
                    _startPos = new Point(j, i);
                }
                if (_grid[i][j] == 'E')
                {
                    _endPos = new Point(j, i);
                }
            }
        }
    }

    public void PrintStats()
    {
        Console.Error.WriteLine("BFS Statistics:");
        Console.Error.WriteLine($"  - States dequeued: {_visitedStatesCount}");
        Console.Error.WriteLine($"  - Max queue size: {_maxQueueSize}");
    }

    public string Solve()
    {
        _visitedStatesCount = 0;
        _maxQueueSize = 0;
        
        var queue = new Queue<State>();
        var cameFrom = new Dictionary<State, (State predecessor, string direction)>();
        var startState = new State(_startPos, 0);
        
        queue.Enqueue(startState);
        cameFrom[startState] = (null, null);

        while (queue.Count > 0)
        {
            _maxQueueSize = Math.Max(_maxQueueSize, queue.Count);
            
            var currentState = queue.Dequeue();
            _visitedStatesCount++;

            if (currentState.Pos.Equals(_endPos))
            {
                return ReconstructPath(cameFrom, currentState);
            }

            for (var i = 0; i < 6; i++)
            {
                ExploreDirection(i, currentState, queue, cameFrom);
            }
        }
        return "";
    }

    private void ExploreDirection(int dirIndex, State currentState, Queue<State> queue, Dictionary<State, (State predecessor, string direction)> cameFrom)
    {
        var currentPos = currentState.Pos;
        var currentKeys = currentState.Keys;
        var (dx, dy) = Offsets[currentPos.Y % 2][dirIndex];
        var initialStepPos = new Point(currentPos.X + dx, currentPos.Y + dy);
        
        if (!IsPassable(initialStepPos, currentKeys))
        {
            return;
        }

        var finalPos = initialStepPos;
        if (_grid[initialStepPos.Y][initialStepPos.X] == '_')
        {
            var slidePos = initialStepPos;
            while (true)
            {
                var (slide_dx, slide_dy) = Offsets[slidePos.Y % 2][dirIndex];
                var nextSlidePos = new Point(slidePos.X + slide_dx, slidePos.Y + slide_dy);
                if (!IsPassable(nextSlidePos, currentKeys))
                {
                    finalPos = slidePos;
                    break;
                }
                if (_grid[nextSlidePos.Y][nextSlidePos.X] != '_')
                {
                    finalPos = nextSlidePos;
                    break;
                }
                slidePos = nextSlidePos;
            }
        }

        var nextKeys = currentKeys;
        var finalCell = _grid[finalPos.Y][finalPos.X];
        if (char.IsLower(finalCell))
        {
            var keyIndex = finalCell - 'a';
            nextKeys |= (1 << keyIndex);
        }

        var nextState = new State(finalPos, nextKeys);
        if (!cameFrom.ContainsKey(nextState))
        {
            var dirName = DirNames[dirIndex];
            cameFrom[nextState] = (currentState, dirName);
            queue.Enqueue(nextState);
        }
    }

    private bool IsPassable(Point pos, int keys)
    {
        if (pos.X < 0 || pos.X >= _w || pos.Y < 0 || pos.Y >= _h)
        {
            return false;
        }
        var cell = _grid[pos.Y][pos.X];
        if (cell == '#')
        {
            return false;
        }
        if (char.IsUpper(cell) && cell != 'S' && cell != 'E')
        {
            var doorIndex = cell - 'A';
            var doorBit = 1 << doorIndex;
            if ((keys & doorBit) == 0)
            {
                return false;
            }
        }
        return true;
    }

    private string ReconstructPath(Dictionary<State, (State predecessor, string direction)> cameFrom, State endState)
    {
        var path = new List<string>();
        var current = endState;
        while (cameFrom.TryGetValue(current, out var value) && value.predecessor != null)
        {
            path.Add(value.direction);
            current = value.predecessor;
        }
        path.Reverse();
        return string.Join(" ", path);
    }
}