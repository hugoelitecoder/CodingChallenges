using System;
using System.Collections.Generic;

public class Solution
{
    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var w = int.Parse(inputs[0]);
        var h = int.Parse(inputs[1]);
        var n = int.Parse(inputs[2]);
        var objective = Console.ReadLine();
        var grid = new string[h];
        for (var i = 0; i < h; i++)
        {
            grid[i] = Console.ReadLine();
        }
        var poi = new Dictionary<Point2D, string>();
        var iop = new Dictionary<string, Point2D>();
        for (var i = 0; i < n; i++)
        {
            var poiInput = Console.ReadLine().Split(' ');
            var type = poiInput[0];
            var x = int.Parse(poiInput[1]);
            var y = int.Parse(poiInput[2]);
            var pos = new Point2D(x, y);
            poi[pos] = type;
            iop[type] = pos;
        }

        var game = new Game(w, h, objective, grid, poi, iop);
        var solver = new Solver(game);
        var result = solver.FindShortestPath();
        Console.WriteLine(result);
    }
}

public record Point2D(int X, int Y);

public record SearchState(Point2D Location, bool HasObjective, bool HasSword, bool HasHorse, int ActsBitmask);

public record AStarState(Point2D Location, int Bitmask);

public record PathResult(int Cost, int FinalBitmask);

public class Game
{
    public int W { get; }
    public int H { get; }
    public string Objective { get; }
    public string[] Grid { get; }
    public IReadOnlyDictionary<Point2D, string> POI { get; }
    public IReadOnlyDictionary<string, Point2D> IOP { get; }
    public IReadOnlyDictionary<string, int> MajorPoiBitmaskMap { get; }

    public Game(int w, int h, string objective, string[] grid, Dictionary<Point2D, string> poi, Dictionary<string, Point2D> iop)
    {
        W = w;
        H = h;
        Objective = objective;
        Grid = grid;
        POI = poi;
        IOP = iop;
        var bitmaskMap = new Dictionary<string, int>();
        var currentBit = 1;
        var actPois = new[] { "PRINCESS", "DRAGON", "TREASURE" };
        foreach (var p in actPois)
        {
            if (IOP.ContainsKey(p))
            {
                bitmaskMap[p] = currentBit;
                currentBit <<= 1;
            }
        }
        MajorPoiBitmaskMap = bitmaskMap;
    }
}

public class Solver
{
    private readonly Game _game;
    private readonly Dictionary<SearchState, int> _memo;
    private const int Infinity = int.MaxValue / 2;

    public Solver(Game game)
    {
        _game = game;
        _memo = new Dictionary<SearchState, int>();
    }

    public int FindShortestPath()
    {
        if (!_game.IOP.ContainsKey("HOUSE") || !_game.IOP.ContainsKey("CASTLE"))
        {
            return Infinity;
        }
        return Search(_game.IOP["HOUSE"], hasObjective: false, hasSword: false, hasHorse: false, actsBitmask: 0);
    }
    
    private int Search(Point2D location, bool hasObjective, bool hasSword, bool hasHorse, int actsBitmask)
    {
        if (_game.POI.TryGetValue(location, out var currentPoiType) && currentPoiType == "CASTLE")
        {
            return 0;
        }
        var state = new SearchState(location, hasObjective, hasSword, hasHorse, actsBitmask);
        if (_memo.TryGetValue(state, out var cachedResult))
        {
            return cachedResult;
        }

        var minCost = Infinity;

        if (!hasObjective)
        {
            var result = AStar(location, _game.IOP[_game.Objective], hasSword, hasHorse, actsBitmask);
            if (result.Cost < Infinity)
            {
                var remainingCost = Search(_game.IOP[_game.Objective], true, hasSword, hasHorse, result.FinalBitmask);
                if (remainingCost < Infinity)
                {
                    minCost = Math.Min(minCost, result.Cost + remainingCost);
                }
            }
        }
        else
        {
            var result = AStar(location, _game.IOP["CASTLE"], hasSword, hasHorse, actsBitmask);
            if (result.Cost < Infinity)
            {
                var remainingCost = Search(_game.IOP["CASTLE"], true, hasSword, hasHorse, result.FinalBitmask);
                if (remainingCost < Infinity)
                {
                    minCost = Math.Min(minCost, result.Cost + remainingCost);
                }
            }
        }

        if (!hasSword && _game.IOP.ContainsKey("BLACKSMITH"))
        {
            var result = AStar(location, _game.IOP["BLACKSMITH"], hasSword, hasHorse, actsBitmask);
            if (result.Cost < Infinity)
            {
                var remainingCost = Search(_game.IOP["BLACKSMITH"], hasObjective, true, hasHorse, result.FinalBitmask);
                if (remainingCost < Infinity)
                {
                    minCost = Math.Min(minCost, result.Cost + remainingCost);
                }
            }
        }

        if (!hasHorse && _game.IOP.ContainsKey("STABLE"))
        {
            var result = AStar(location, _game.IOP["STABLE"], hasSword, hasHorse, actsBitmask);
            if (result.Cost < Infinity)
            {
                var remainingCost = Search(_game.IOP["STABLE"], hasObjective, hasSword, true, result.FinalBitmask);
                if (remainingCost < Infinity)
                {
                    minCost = Math.Min(minCost, result.Cost + remainingCost);
                }
            }
        }

        _memo[state] = minCost;
        return minCost;
    }

    private PathResult AStar(Point2D start, Point2D target, bool sword, bool horse, int initialBitmask)
    {
        var openSet = new PriorityQueue<AStarState, int>();
        var gScore = new Dictionary<AStarState, int>();
        var startState = new AStarState(start, initialBitmask);
        gScore[startState] = 0;
        openSet.Enqueue(startState, Heuristic(start, target));

        while (openSet.Count > 0)
        {
            var currentState = openSet.Dequeue();
            var current = currentState.Location;
            var currentBitmask = currentState.Bitmask;

            if (current.Equals(target))
            {
                return new PathResult(gScore[currentState], currentBitmask);
            }

            var weight = 1;
            var nextBitmask = currentBitmask;
            var terrain = _game.Grid[current.Y][current.X];
            
            switch (terrain)
            {
                case 'W':
                case 'G': weight = horse ? 1 : 2; break;
                case 'M': weight = 4; break;
                case 'S': weight = horse ? 3 : 6; break;
                case 'I':
                    if (_game.POI.TryGetValue(current, out var poiType) && _game.MajorPoiBitmaskMap.TryGetValue(poiType, out var bit))
                    {
                        if ((currentBitmask & bit) == 0)
                        {
                            weight = sword ? 2 : 4;
                            nextBitmask |= bit;
                        }
                    }
                    break;
            }

            var neighbors = GetNeighbors(current);

            foreach (var neighbor in neighbors)
            {
                if (!IsMoveValid(neighbor, sword, horse) || !IsPassThroughValid(neighbor, target, sword, horse, nextBitmask))
                {
                    continue;
                }
                var tgScore = gScore[currentState] + weight;
                var neighborState = new AStarState(neighbor, nextBitmask);

                if (tgScore < gScore.GetValueOrDefault(neighborState, Infinity))
                {
                    gScore[neighborState] = tgScore;
                    var fScore = tgScore + Heuristic(neighbor, target);
                    openSet.Enqueue(neighborState, fScore);
                }
            }
        }
        return new PathResult(Infinity, -1);
    }
    
    private List<Point2D> GetNeighbors(Point2D current)
    {
        var neighbors = new List<Point2D>();
        if (_game.POI.TryGetValue(current, out var poiType) && poiType == "WIZARD")
        {
            var minDist = int.MaxValue;
            Point2D closestPoiCoord = null;
            foreach (var kvp in _game.POI)
            {
                if (kvp.Value != "WIZARD")
                {
                    var pCoord = kvp.Key;
                    var dist = Math.Abs(current.X - pCoord.X) + Math.Abs(current.Y - pCoord.Y);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPoiCoord = pCoord;
                    }
                }
            }
            if (closestPoiCoord != null) neighbors.Add(closestPoiCoord);
        }
        else
        {
            for (var i = -1; i <= 1; i++)
            for (var j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                var nCoord = new Point2D(current.X + j, current.Y + i);
                if (nCoord.X >= 0 && nCoord.X < _game.W && nCoord.Y >= 0 && nCoord.Y < _game.H)
                {
                    neighbors.Add(nCoord);
                }
            }
        }
        return neighbors;
    }

    private bool IsMoveValid(Point2D dest, bool sword, bool horse)
    {
        var terrain = _game.Grid[dest.Y][dest.X];
        if (terrain == 'R') return false;
        if (sword && terrain == 'W') return false;
        if (horse && terrain == 'M') return false;
        return true;
    }

    private bool IsPassThroughValid(Point2D neighbor, Point2D target, bool sword, bool horse, int bitmask)
    {
        if (!_game.POI.TryGetValue(neighbor, out var poiType))
        {
            return true;
        }
        if (_game.POI[target] == poiType) return true;
        if (poiType == _game.Objective && _game.MajorPoiBitmaskMap.ContainsKey(_game.Objective))
        {
            return (bitmask & _game.MajorPoiBitmaskMap[_game.Objective]) != 0;
        }
        if (poiType == "STABLE") return horse;
        if (poiType == "BLACKSMITH") return sword;
        return true;
    }

    private int Heuristic(Point2D a, Point2D b)
    {
        return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }
}