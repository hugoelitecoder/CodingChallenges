using System;
using System.Linq;
using System.Collections.Generic;

class Player
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        
        var game = new VoxCodeiGame(width, height);
        int round = 0;

        while (true)
        {
            round++;
            inputs = Console.ReadLine().Split(' ');
            var roundsLeft = int.Parse(inputs[0]);
            var bombsLeft = int.Parse(inputs[1]);
            
            var map = Enumerable.Range(0, height).Select(_ => Console.ReadLine()).ToList();

            var (command, activeNodes, activeBombs) = game.GetNextCommand(round, bombsLeft, roundsLeft, map);
            
            PrintAugmentedMap(map, activeNodes, activeBombs, round, width, height);
            Console.WriteLine(command);
        }
    }

    private static void PrintAugmentedMap(List<string> map, List<Node> nodes, List<Bomb> bombs, int round, int width, int height)
    {
        Console.Error.WriteLine($"--- Turn {round} ---");
        var grid = map.Select(r => r.ToCharArray()).ToArray();
        
        foreach (var bomb in bombs)
        {
            if (bomb.Position.Y >= 0 && bomb.Position.Y < height && bomb.Position.X >= 0 && bomb.Position.X < width)
                grid[bomb.Position.Y][bomb.Position.X] = '*';
        }
        
        foreach (var node in nodes)
        {
            var pos = node.GetPositionAt(round);
            if (pos.Y >= 0 && pos.Y < height && pos.X >= 0 && pos.X < width)
                grid[pos.Y][pos.X] = (char)('0' + node.Id % 10);
        }

        foreach (var row in grid)
        {
            Console.Error.WriteLine(new string(row));
        }
    }
}

public class VoxCodeiGame
{
    private readonly int _width, _height;
    private readonly char[,] _staticWalls;
    private readonly List<Node> _nodes = new();
    private readonly List<Bomb> _bombs = new();
    private bool _isDetectionComplete;
    private readonly List<List<string>> _observationMaps = new();
    private BombPlan _currentPlan;

    public VoxCodeiGame(int width, int height)
    {
        (_width, _height) = (width, height);
        _staticWalls = new char[_height, _width];
    }

    public (string, List<Node>, List<Bomb>) GetNextCommand(int round, int bombsLeft, int roundsLeft, List<string> map)
    {
        UpdateStateByExplosions(round);
        
        if (round <= 3)
        {
            _observationMaps.Add(map);
            return ("WAIT", new List<Node>(), _bombs);
        }
        
        if (!_isDetectionComplete)
        {
            AnalyzeObservationsAndBuildModels();
            if (!_isDetectionComplete) return ("WAIT", new List<Node>(), _bombs);
        }

        var realNodePositions = FindAllNodesInMap(map).ToHashSet();
        foreach (var node in _nodes.Where(n => !n.IsEliminated))
        {
            if (!realNodePositions.Contains(node.GetPositionAt(round)))
                node.IsEliminated = true;
        }

        var activeNodes = _nodes.Where(n => !n.IsEliminated).ToList();
        if (!activeNodes.Any())
        {
            return ("WAIT", activeNodes, _bombs);
        }

        if (_currentPlan != null && _currentPlan.TargetingSolutions.Any(s => s.Targets.All(t => t.IsEliminated)))
            _currentPlan = null;

        if (_currentPlan == null || _currentPlan.IsComplete(round))
            _currentPlan = AttackPlanner.FindBestPlan(activeNodes, _bombs, round, bombsLeft, roundsLeft, _width, _height, _staticWalls);

        var action = _currentPlan.GetActionForRound(round);
        if (action.HasValue)
        {
            _bombs.Add(new Bomb(action.Value, round));
            return ($"{action.Value.X} {action.Value.Y}", activeNodes, _bombs);
        }

        return ("WAIT", activeNodes, _bombs);
    }

    private void AnalyzeObservationsAndBuildModels()
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                _staticWalls[y, x] = _observationMaps[0][y][x] == '#' ? '#' : '.';

        var positions1 = FindAllNodesInMap(_observationMaps[0]);
        var positions2 = FindAllNodesInMap(_observationMaps[1]).ToHashSet();
        var positions3 = FindAllNodesInMap(_observationMaps[2]).ToHashSet();

        var allPossiblePaths = positions1.Select(p1 => 
            Node.Directions.Select(vel => (vel, p2: Node.PredictNextStep(p1, vel, _width, _height, _staticWalls)))
                .Select(t => (t.vel, t.p2.nextPos, p3: Node.PredictNextStep(t.p2.nextPos, t.p2.nextVel, _width, _height, _staticWalls).nextPos))
                .Where(t => positions2.Contains(t.nextPos) && positions3.Contains(t.p3))
                .Select(t => new Path(p1, t.nextPos, t.p3, t.vel))
                .ToList()
        ).ToList();

        var finalPaths = new List<Path>();
        if (FindPathSolution(allPossiblePaths, 0, new List<Path>(), finalPaths, positions2, positions3))
        {
            _isDetectionComplete = true;
            _nodes.AddRange(finalPaths.OrderBy(p => p.P1.Y).ThenBy(p => p.P1.X)
                .Select((p, i) => new Node(i, p.P1, p.Velocity))
            );
            foreach(var node in _nodes) node.PrecomputeFullPath(150, _width, _height, _staticWalls);
        }
    }

    private static bool FindPathSolution(List<List<Path>> allPaths, int idx, List<Path> current, List<Path> final, ISet<Point> p2, ISet<Point> p3)
    {
        if (idx == allPaths.Count)
        {
            if (current.Select(path => path.P2).ToHashSet().SetEquals(p2) &&
                current.Select(path => path.P3).ToHashSet().SetEquals(p3))
            {
                final.AddRange(current);
                return true;
            }
            return false;
        }

        foreach (var path in allPaths[idx])
        {
            current.Add(path);
            if (FindPathSolution(allPaths, idx + 1, current, final, p2, p3)) return true;
            current.RemoveAt(current.Count - 1);
        }
        return false;
    }

    private void UpdateStateByExplosions(int round)
    {
        var explodingNow = _bombs.Where(b => b.ExplosionRound == round).ToList();
        if (!explodingNow.Any()) return;
        
        var allExplodedThisTurn = new HashSet<Bomb>();
        while (explodingNow.Any())
        {
            foreach (var b in explodingNow) allExplodedThisTurn.Add(b);

            var currentBlastArea = explodingNow
                .SelectMany(b => AttackPlanner.GetBlastArea(b.Position, _width, _height, _staticWalls))
                .ToHashSet();

            explodingNow = _bombs
                .Where(b => !allExplodedThisTurn.Contains(b) && currentBlastArea.Contains(b.Position))
                .ToList();
        }

        if (allExplodedThisTurn.Any())
        {
            var totalBlastArea = allExplodedThisTurn
                .SelectMany(b => AttackPlanner.GetBlastArea(b.Position, _width, _height, _staticWalls))
                .ToHashSet();

            foreach (var node in _nodes.Where(n => !n.IsEliminated && totalBlastArea.Contains(n.GetPositionAt(round))))
                node.IsEliminated = true;

            _bombs.RemoveAll(b => allExplodedThisTurn.Contains(b));
        }
    }

    private List<Point> FindAllNodesInMap(List<string> map) =>
        Enumerable.Range(0, _height).SelectMany(y => 
            Enumerable.Range(0, _width).Where(x => map[y][x] == '@').Select(x => new Point(x, y))
        ).ToList();
}

internal static class AttackPlanner
{
    private static Dictionary<(HashSet<Node>, int), BombPlan> _memo;
    private static int _deadline;

    public static BombPlan FindBestPlan(List<Node> activeNodes, List<Bomb> bombs, int round, int bombsLeft, int roundsLeft, int w, int h, char[,] walls)
    {
        _memo = new Dictionary<(HashSet<Node>, int), BombPlan>(new PlanCacheKeyComparer());
        _deadline = round + roundsLeft;

        var doomedNodes = bombs.SelectMany(b => activeNodes.Where(n => GetBlastArea(b.Position, w, h, walls).Contains(n.GetPositionAt(b.ExplosionRound)))).ToHashSet();
        var initialNodes = activeNodes.Except(doomedNodes).ToHashSet();
        if (!initialNodes.Any()) return new BombPlan(new List<BombSolution>());

        var allSolutions = PrecomputeAllSolutions(initialNodes, round, w, h, walls);
        var solutionsHittingNode = initialNodes.ToDictionary(n => n, n => allSolutions.Where(s => s.Targets.Contains(n)).ToList());
        
        var plan = FindMinBombPlan(initialNodes, bombsLeft, solutionsHittingNode, round);
        return plan ?? new BombPlan(new List<BombSolution>());
    }

    private static BombPlan FindMinBombPlan(HashSet<Node> uncovered, int bombsLeft, Dictionary<Node, List<BombSolution>> solutionsHittingNode, int minRound)
    {
        if (!uncovered.Any()) return new BombPlan(new List<BombSolution>());
        if (bombsLeft == 0) return null;

        var key = (uncovered, minRound);
        if (_memo.TryGetValue(key, out var cachedPlan)) return cachedPlan;

        var targetNode = uncovered.MinBy(n => solutionsHittingNode[n].Count(s => s.PlacementRound >= minRound));
        if (targetNode == null || !solutionsHittingNode[targetNode].Any(s => s.PlacementRound >= minRound))
        {
            _memo[key] = null;
            return null;
        }

        var candidateSolutions = solutionsHittingNode[targetNode]
            .Where(s => s.PlacementRound >= minRound && s.ExplosionRound < _deadline)
            .OrderByDescending(s => s.Targets.Count(uncovered.Contains))
            .ThenBy(s => s.ExplosionRound)
            .Take(15);

        foreach (var solution in candidateSolutions)
        {
            var nextUncovered = new HashSet<Node>(uncovered);
            nextUncovered.ExceptWith(solution.Targets);
            var subPlan = FindMinBombPlan(nextUncovered, bombsLeft - 1, solutionsHittingNode, solution.PlacementRound + 1);
            if (subPlan != null)
            {
                var finalPlan = new BombPlan(new List<BombSolution>(subPlan.TargetingSolutions) { solution });
                _memo[key] = finalPlan;
                return finalPlan;
            }
        }

        _memo[key] = null;
        return null;
    }

    private static List<BombSolution> PrecomputeAllSolutions(HashSet<Node> nodes, int round, int w, int h, char[,] walls)
    {
        var maxRound = _deadline;
        var nodePosByRound = Enumerable.Range(round, Math.Max(0, maxRound - round)).ToDictionary(r => r, _ => new HashSet<Point>());
        
        foreach (var node in nodes)
            foreach (var r in nodePosByRound.Keys)
                if (r < 150)
                    nodePosByRound[r].Add(node.GetPositionAt(r));

        var allSolutions = new List<BombSolution>();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (walls[y, x] == '#') continue;
                var p = new Point(x, y);
                var blastArea = GetBlastArea(p, w, h, walls);
                foreach (var placementRound in nodePosByRound.Keys)
                {
                    if (placementRound + 3 >= maxRound || nodePosByRound[placementRound].Contains(p)) continue;
                    
                    var explosionRound = placementRound + 3;
                    var targets = nodes.Where(n => blastArea.Contains(n.GetPositionAt(explosionRound))).ToHashSet();
                    if (targets.Any()) allSolutions.Add(new BombSolution(p, placementRound, explosionRound, targets));
                }
            }
        }
        return allSolutions;
    }

    public static HashSet<Point> GetBlastArea(Point p, int w, int h, char[,] walls)
    {
        var area = new HashSet<Point> { p };
        for (int i = 1; i <= 3; i++) { if (p.X + i >= w || walls[p.Y, p.X + i] == '#') break; area.Add(new Point(p.X + i, p.Y)); }
        for (int i = 1; i <= 3; i++) { if (p.X - i < 0 || walls[p.Y, p.X - i] == '#') break; area.Add(new Point(p.X - i, p.Y)); }
        for (int i = 1; i <= 3; i++) { if (p.Y + i >= h || walls[p.Y + i, p.X] == '#') break; area.Add(new Point(p.X, p.Y + i)); }
        for (int i = 1; i <= 3; i++) { if (p.Y - i < 0 || walls[p.Y - i, p.X] == '#') break; area.Add(new Point(p.X, p.Y - i)); }
        return area;
    }
}

public class Node
{
    public int Id { get; }
    public bool IsEliminated { get; set; }
    private readonly Point _startPos, _startVel;
    private Point[] _pathCache;
    public static readonly Point[] Directions = { new(0, -1), new(0, 1), new(-1, 0), new(1, 0), new(0, 0) };

    public Node(int id, Point pos, Point vel) { Id = id; _startPos = pos; _startVel = vel; }
    
    public void PrecomputeFullPath(int maxRound, int w, int h, char[,] walls)
    {
        _pathCache = new Point[maxRound];
        _pathCache[0] = new Point(-1, -1);
        var (pos, vel) = (_startPos, _startVel);
        for (int i = 1; i < maxRound; i++)
        {
            _pathCache[i] = pos;
            (pos, vel) = PredictNextStep(pos, vel, w, h, walls);
        }
    }

    public Point GetPositionAt(int round) => _pathCache[round];

    public static (Point nextPos, Point nextVel) PredictNextStep(Point pos, Point vel, int w, int h, char[,] walls)
    {
        var nextVel = vel;
        var tentative = new Point(pos.X + vel.X, pos.Y + vel.Y);
        if (tentative.X < 0 || tentative.X >= w || tentative.Y < 0 || tentative.Y >= h || walls[tentative.Y, tentative.X] == '#')
            nextVel = new Point(-vel.X, -vel.Y);
        return (new Point(pos.X + nextVel.X, pos.Y + nextVel.Y), nextVel);
    }
    
    public override int GetHashCode() => Id;
    public override bool Equals(object obj) => obj is Node n && n.Id == Id;
}

public class BombPlan
{
    public List<BombSolution> TargetingSolutions { get; }
    private readonly int _lastPlacementRound;
    public BombPlan(List<BombSolution> solutions)
    {
        TargetingSolutions = solutions.OrderBy(s => s.PlacementRound).ToList();
        _lastPlacementRound = TargetingSolutions.Any() ? TargetingSolutions.Max(s => s.PlacementRound) : -1;
    }
    public bool IsComplete(int round) => round > _lastPlacementRound;
    public Point? GetActionForRound(int round) => TargetingSolutions.FirstOrDefault(s => s.PlacementRound == round)?.Position;
}

public record Bomb(Point Position, int PlacementRound)
{
    public int ExplosionRound => PlacementRound + 3;
}

public record BombSolution(Point Position, int PlacementRound, int ExplosionRound, HashSet<Node> Targets);

public readonly record struct Point(int X, int Y)
{
    public override string ToString() => $"{X} {Y}";
}

public record Path(Point P1, Point P2, Point P3, Point Velocity);

public class PlanCacheKeyComparer : IEqualityComparer<(HashSet<Node> nodes, int round)>
{
    public bool Equals((HashSet<Node> nodes, int round) x, (HashSet<Node> nodes, int round) y) => 
        x.round == y.round && x.nodes.SetEquals(y.nodes);

    public int GetHashCode((HashSet<Node> nodes, int round) obj) =>
        obj.nodes.OrderBy(n => n.Id).Aggregate(obj.round, (hash, node) => hash * 31 + node.GetHashCode());
}