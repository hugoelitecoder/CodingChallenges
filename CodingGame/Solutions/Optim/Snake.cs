using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public static class DebugLogger
{
    private static readonly bool IsEnabled = true;
    private static List<string> _logBuffer = new List<string>();

    public static void Clear() => _logBuffer.Clear();
    public static void Log(string message) => _logBuffer.Add(message);

    public static void PrintSummary(long timeMs, Point finalMove)
    {
        if (!IsEnabled) return;
        
        Log($"Decision: Move to {finalMove} (took {timeMs}ms)");
        foreach(var line in _logBuffer)
        {
            Console.Error.WriteLine(line);
        }
    }
}

public struct Point : IEquatable<Point>
{
    public readonly int X;
    public readonly int Y;
    public Point(int x, int y) { X = x; Y = y; }
    public bool Equals(Point other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is Point other && Equals(other);
    public override int GetHashCode() => (X * 397) ^ Y;
    public override string ToString() => $"{X} {Y}";
}

public class PathNode
{
    public Point Position { get; }
    public int GCost { get; }
    public int HCost { get; }
    public int FCost => GCost + HCost;
    public PathNode Parent { get; }
    public PathNode(Point position, int gCost, int hCost, PathNode parent)
    {
        Position = position; GCost = gCost; HCost = hCost; Parent = parent;
    }
}

class Player
{
    private const int WIDTH = 96;
    private const int HEIGHT = 54;
    private static List<Point> _rabbits;
    private static List<Point> _snake;
    private static int _turnCount = 0;
    private static int _turnsSinceLastCatch = 0;

    static void Main(string[] args)
    {
        ReadInitialRabbits();
        while (true)
        {
            var stopwatch = Stopwatch.StartNew();
            DebugLogger.Clear();
            _turnCount++;
            _turnsSinceLastCatch++;
            ReadSnakeState();
            
            if (_rabbits.RemoveAll(r => r.Equals(_snake[0])) > 0)
            {
                DebugLogger.Log($"[STATE] Ate rabbit at {_snake[0]}.");
                _turnsSinceLastCatch = 0;
            }
            
            DebugLogger.Log($"--- Turn {_turnCount} | SnakeLen={_snake.Count} | SinceCatch={_turnsSinceLastCatch} ---");

            var nextMove = DecideNextMove();
            CheckImminentCollision(nextMove, _snake);

            stopwatch.Stop();
            DebugLogger.PrintSummary(stopwatch.ElapsedMilliseconds, nextMove);
            Console.WriteLine(nextMove);
        }
    }

    private static Point DecideNextMove()
    {
        if (_rabbits.Count > 0)
        {
            var bestPath = FindBestRabbitSequence();
            if (bestPath != null && bestPath.Count > 0)
            {
                return bestPath[0];
            }
        }
        
        DebugLogger.Log("[STRATEGY] No safe rabbit paths found. Switching to survival.");
        return FindSurvivalMove();
    }
    
    private static List<Point> FindBestRabbitSequence()
    {
        var snakeHead = _snake[0];
        var candidates = new List<(double Score, List<Point> Path, string Desc)>();

        var primaryCandidates = _rabbits.OrderBy(r => ManhattanDistance(snakeHead, r)).Take(5).ToList();

        foreach (var r1 in primaryCandidates)
        {
            var path1 = FindPathAStar(snakeHead, r1, _snake, true);
            if (path1 == null) continue;
            
            var snakeAtR1 = GenerateFutureSnake(_snake, path1);
            if (!IsPathSafe(r1, snakeAtR1))
            {
                DebugLogger.Log($"[SAFETY] Path to {r1} is a TRAP. Discarding.");
                continue;
            }

            var (score1, space1) = EvaluateSequence(new List<List<Point>> { path1 }, snakeAtR1);
            if(score1 > double.MinValue) candidates.Add((score1, path1, $"r1:{r1} | len:{path1.Count}, space:{space1}"));

            var secondaryCandidates = _rabbits.Where(r => !r.Equals(r1)).OrderBy(r => ManhattanDistance(r1, r)).Take(3).ToList();
            foreach (var r2 in secondaryCandidates)
            {
                var path2 = FindPathAStar(r1, r2, snakeAtR1, true);
                if (path2 == null) continue;

                var snakeAtR2 = GenerateFutureSnake(snakeAtR1, path2);
                if (!IsPathSafe(r2, snakeAtR2))
                {
                    DebugLogger.Log($"[SAFETY] Path to {r1}->{r2} is a TRAP. Discarding.");
                    continue;
                }

                var (score2, space2) = EvaluateSequence(new List<List<Point>> { path1, path2 }, snakeAtR2);
                if(score2 > double.MinValue) candidates.Add((score2, path1, $"r1:{r1}->r2:{r2} | len:{path1.Count}+{path2.Count}, space:{space2}"));
            }
        }

        if (!candidates.Any()) return null;

        var bestCandidates = candidates.OrderByDescending(c => c.Score).ToList();
        
        DebugLogger.Log("[STRATEGY] Rabbit Chase Analysis (Post-Safety Filter):");
        foreach (var (score, _, desc) in bestCandidates.Take(3))
        {
            DebugLogger.Log($"  - Cand: {desc} | score:{score:F0}");
        }

        DebugLogger.Log($"[CHOICE] Best safe target: {bestCandidates[0].Desc}");
        return bestCandidates[0].Path;
    }

    private static (double score, int space) EvaluateSequence(List<List<Point>> pathSegments, List<Point> finalSnakeState)
    {
        double score = 0.0;
        int totalTurns = 0;

        for (var i = 0; i < pathSegments.Count; i++)
        {
            var segment = pathSegments[i];
            totalTurns += segment.Count;
        }

        score = 100000.0 / (totalTurns + _turnsSinceLastCatch); // Heavily prioritize shorter paths

        var finalHead = finalSnakeState[0];
        var finalObstacles = new HashSet<Point>(finalSnakeState);
        int space = CountReachableSpace(finalHead, finalObstacles);

        score += space * 5; // Reward open space

        return (score, space);
    }

    private static List<Point> GenerateFutureSnake(List<Point> initialSnake, List<Point> path)
    {
        var pathLength = path.Count;
        var newLength = initialSnake.Count + 1;
        var futureSnake = new List<Point>(newLength);
        futureSnake.AddRange(path.AsEnumerable().Reverse());
        var tailPartsToKeep = newLength - pathLength;
        if (tailPartsToKeep > 0) futureSnake.AddRange(initialSnake.Take(tailPartsToKeep));
        return futureSnake.Take(newLength).ToList();
    }
    
    private static bool IsPathSafe(Point destination, List<Point> futureSnake)
    {
        if (futureSnake == null || futureSnake.Count < 2) return true;
        
        Point futureHead = destination;
        Point futureTail = futureSnake.Last();
        var obstacles = new HashSet<Point>(futureSnake);
        obstacles.Remove(futureHead);
        obstacles.Remove(futureTail);
        
        return CanFindSimplePath(futureHead, futureTail, obstacles);
    }
    
    private static bool CanFindSimplePath(Point start, Point goal, HashSet<Point> obstacles)
    {
        var q = new Queue<Point>();
        q.Enqueue(start);
        var visited = new HashSet<Point>(obstacles);
        visited.Add(start);
        int exploredNodes = 0;

        while(q.Any())
        {
            if(exploredNodes++ > 2500) return true;

            Point current = q.Dequeue();
            if(current.Equals(goal)) return true;

            foreach(var neighbor in GetNeighbors(current))
            {
                if(neighbor.X >= 0 && neighbor.X < WIDTH && neighbor.Y >= 0 && neighbor.Y < HEIGHT && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    q.Enqueue(neighbor);
                }
            }
        }
        return false;
    }

    private static List<Point> FindPathAStar(Point start, Point goal, List<Point> snakeBody, bool isEating)
    {
        var effectiveSnakeBody = new List<Point>(snakeBody);
        if (isEating && effectiveSnakeBody.Any())
        {
            effectiveSnakeBody.Add(effectiveSnakeBody.Last());
        }

        var openSet = new PriorityQueue<PathNode, int>();
        var visited = new Dictionary<Point, int>();
        var startNode = new PathNode(start, 0, ManhattanDistance(start, goal), null);
        openSet.Enqueue(startNode, startNode.FCost);
        visited[start] = 0;

        while (openSet.TryDequeue(out var current, out _))
        {
            if (current.Position.Equals(goal)) return ReconstructPath(current);
            if (current.GCost > 250) continue; 

            foreach (var neighbor in GetNeighbors(current.Position))
            {
                if (neighbor.X < 0 || neighbor.X >= WIDTH || neighbor.Y < 0 || neighbor.Y >= HEIGHT) continue;
                var time = current.GCost + 1;
                if (visited.ContainsKey(neighbor) && time >= visited[neighbor]) continue;
                if (IsCollision(neighbor, time, current, effectiveSnakeBody)) continue;
                
                visited[neighbor] = time;
                
                var neighborNode = new PathNode(neighbor, time, ManhattanDistance(neighbor, goal), current);
                openSet.Enqueue(neighborNode, neighborNode.FCost);
            }
        }
        return null;
    }
    
    private static bool IsCollision(Point pos, int time, PathNode pathNode, List<Point> snakeBody)
    {
        var temp = pathNode;
        while (temp != null && temp.Parent != null)
        {
            if (temp.Position.Equals(pos)) return true;
            temp = temp.Parent;
        }
        for (var i = 0; i < snakeBody.Count; i++)
        {
            if (snakeBody[i].Equals(pos) && time <= i) return true;
        }
        return false;
    }

    private static Point FindSurvivalMove()
    {
        var head = _snake[0];
        var obstacles = new HashSet<Point>(_snake.Take(_snake.Count - 1));

        var validMoves = GetNeighbors(head)
            .Where(move => move.X >= 0 && move.X < WIDTH && move.Y >= 0 && move.Y < HEIGHT && !obstacles.Contains(move))
            .ToList();

        if (!validMoves.Any())
        {
            DebugLogger.Log("[SURVIVAL] TRAPPED! No valid moves. Last resort.");
            var neck = _snake.Count > 1 ? _snake[1] : new Point(-1, -1);
            var nonNeckMoves = GetNeighbors(head).Where(m => !m.Equals(neck)).ToList();
            if (nonNeckMoves.Any()) return nonNeckMoves.First();
            return GetNeighbors(head).First();
        }
        
        var bestMove = validMoves
            .Select(move => new { Move = move, Space = CountReachableSpace(move, new HashSet<Point>(_snake)) })
            .OrderByDescending(x => x.Space)
            .First();

        DebugLogger.Log($"[SURVIVAL] Chose {bestMove.Move} with space score: {bestMove.Space}");
        return bestMove.Move;
    }

    private static int CountReachableSpace(Point startPos, HashSet<Point> obstacles)
    {
        var q = new Queue<Point>();
        q.Enqueue(startPos);
        var visited = new HashSet<Point>(obstacles);
        visited.Add(startPos);
        int count = 0;
        int limit = 500;
        
        while (q.Count > 0 && count < limit)
        {
            var current = q.Dequeue();
            count++;
            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.X >= 0 && neighbor.X < WIDTH && neighbor.Y >= 0 && neighbor.Y < HEIGHT && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    q.Enqueue(neighbor);
                }
            }
        }
        return count;
    }
    
    private static List<Point> ReconstructPath(PathNode node)
    {
        var path = new List<Point>();
        while (node.Parent != null) { path.Add(node.Position); node = node.Parent; }
        path.Reverse();
        return path;
    }

    private static IEnumerable<Point> GetNeighbors(Point p)
    {
        yield return new Point(p.X, p.Y - 1); yield return new Point(p.X, p.Y + 1);
        yield return new Point(p.X + 1, p.Y); yield return new Point(p.X - 1, p.Y);
    }

    private static int ManhattanDistance(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    private static void ReadInitialRabbits()
    {
        var n = int.Parse(Console.ReadLine()); _rabbits = new List<Point>(n);
        for (var i = 0; i < n; i++) { var inputs = Console.ReadLine().Split(' '); _rabbits.Add(new Point(int.Parse(inputs[0]), int.Parse(inputs[1]))); }
    }
    private static void ReadSnakeState()
    {
        var nsnake = int.Parse(Console.ReadLine()); _snake = new List<Point>(nsnake);
        for (var i = 0; i < nsnake; i++) { var inputs = Console.ReadLine().Split(' '); _snake.Add(new Point(int.Parse(inputs[0]), int.Parse(inputs[1]))); }
    }
    
    private static void CheckImminentCollision(Point move, List<Point> snake)
    {
        for (int i = 0; i < snake.Count - 1; i++)
        {
            if (snake[i].Equals(move))
            {
                DebugLogger.Log($"!! FLAW: Suicidal move {move} collides with body part @ index {i} ({snake[i]}) !!");
            }
        }
    }
}