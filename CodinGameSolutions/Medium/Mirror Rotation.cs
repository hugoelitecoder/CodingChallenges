using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine()!.Split(' ');
        var l = int.Parse(inputs[0]);
        var w = int.Parse(inputs[1]);
        var grid = new string[w];
        var start = default(Point);
        var target = default(Point);
        for (var i = 0; i < w; i++)
        {
            var line = Console.ReadLine()!;
            grid[i] = line;
            var lIndex = line.IndexOf('L');
            if (lIndex != -1)
            {
                start = new Point(lIndex, i);
            }
            var tIndex = line.IndexOf('T');
            if (tIndex != -1)
            {
                target = new Point(tIndex, i);
            }
        }
        var startDir = Console.ReadLine()![0];

        var solver = new LaserSolver(l, w, grid, start, target, startDir);
        var flippedMirrors = solver.FindFlippedMirrors();
        
        foreach (var p in flippedMirrors)
        {
            Console.WriteLine($"{p.C} {p.R}");
        }
    }
}

internal class LaserSolver
{
    private readonly int _l;
    private readonly int _w;
    private readonly string[] _grid;
    private readonly Point _start;
    private readonly Point _target;
    private readonly char _startDir;

    private static readonly Dictionary<char, (int dr, int dc)> Dirs = new()
    {
        ['N'] = (-1, 0), ['E'] = (0, 1), ['S'] = (1, 0), ['W'] = (0, -1)
    };
    private static readonly Dictionary<char, Dictionary<char, char>> Reflect = new()
    {
        ['/'] = new() { ['N'] = 'E', ['E'] = 'N', ['S'] = 'W', ['W'] = 'S' },
        ['\\'] = new() { ['N'] = 'W', ['W'] = 'N', ['S'] = 'E', ['E'] = 'S' }
    };
    
    public LaserSolver(int l, int w, string[] grid, Point start, Point target, char startDir)
    {
        _l = l;
        _w = w;
        _grid = grid;
        _start = start;
        _target = target;
        _startDir = startDir;
    }

    public List<Point> FindFlippedMirrors()
    {
        var pq = new PriorityQueue<State, int>();
        var costs = new Dictionary<State, int>();
        var parents = new Dictionary<State, (State, bool)>();

        var startState = new State(_start, _startDir);
        costs[startState] = 0;
        parents[startState] = (null, false);
        pq.Enqueue(startState, 0);
        
        while (pq.TryDequeue(out var currentState, out var currentFlips))
        {
            if (costs.TryGetValue(currentState, out var knownCost) && knownCost < currentFlips)
            {
                continue;
            }
            
            TraceBeam(currentState, currentFlips, pq, costs, parents);
        }
        
        return ReconstructPath(costs, parents);
    }
    
    private void TraceBeam(State fromState, int fromFlips, PriorityQueue<State, int> pq, Dictionary<State, int> costs, Dictionary<State, (State, bool)> parents)
    {
        var (dr, dc) = Dirs[fromState.Dir];
        var r = fromState.Pos.R;
        var c = fromState.Pos.C;

        while (true)
        {
            r += dr;
            c += dc;
            if (r < 0 || r >= _w || c < 0 || c >= _l || _grid[r][c] == '#')
            {
                break;
            }
            var cell = _grid[r][c];
            if (cell is '/' or '\\')
            {
                var mirrorPos = new Point(c, r);
                ProcessMirrorHit(fromState, fromFlips, mirrorPos, cell, pq, costs, parents);
                break;
            }
        }
    }

    private void ProcessMirrorHit(State fromState, int fromFlips, Point mirrorPos, char mirrorType, PriorityQueue<State, int> pq, Dictionary<State, int> costs, Dictionary<State, (State, bool)> parents)
    {
        void ProcessMove(bool isFlipped)
        {
            var newFlips = fromFlips + (isFlipped ? 1 : 0);
            var currentMirrorType = isFlipped ? (mirrorType == '/' ? '\\' : '/') : mirrorType;
            var newDir = Reflect[currentMirrorType][fromState.Dir];
            var newState = new State(mirrorPos, newDir);

            if (!costs.ContainsKey(newState) || newFlips < costs[newState])
            {
                costs[newState] = newFlips;
                parents[newState] = (fromState, isFlipped);
                pq.Enqueue(newState, newFlips);
            }
        }
        ProcessMove(isFlipped: false);
        ProcessMove(isFlipped: true);
    }

    private List<Point> ReconstructPath(Dictionary<State, int> costs, Dictionary<State, (State, bool)> parents)
    {
        var minFlips = int.MaxValue;
        State bestFinalState = null;

        foreach (var (state, flips) in costs)
        {
            if (CanSeeTarget(state))
            {
                if (flips < minFlips)
                {
                    minFlips = flips;
                    bestFinalState = state;
                }
            }
        }
        
        var flippedMirrors = new List<Point>();
        if (bestFinalState == null)
        {
            return flippedMirrors;
        }

        var backtrackState = bestFinalState;
        while (backtrackState != null && parents.TryGetValue(backtrackState, out var entry))
        {
            var (prevState, wasFlipped) = entry;
            if (wasFlipped)
            {
                flippedMirrors.Add(backtrackState.Pos);
            }
            backtrackState = prevState;
        }
        
        return flippedMirrors.OrderBy(p => p.R).ThenBy(p => p.C).ToList();
    }

    private bool CanSeeTarget(State fromState)
    {
        var (dr, dc) = Dirs[fromState.Dir];
        var r = fromState.Pos.R;
        var c = fromState.Pos.C;

        while (true)
        {
            r += dr;
            c += dc;
            if (r < 0 || r >= _w || c < 0 || c >= _l || _grid[r][c] == '#')
            {
                return false;
            }
            var currentPos = new Point(c, r);
            if (currentPos.Equals(_target))
            {
                return true;
            }
            if (_grid[r][c] is '/' or '\\')
            {
                return false;
            }
        }
    }
}

internal record Point(int C, int R);

internal record State(Point Pos, char Dir);