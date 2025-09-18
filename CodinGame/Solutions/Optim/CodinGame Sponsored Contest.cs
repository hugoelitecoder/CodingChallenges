using System;
using System.Collections.Generic;
using System.Linq;

public record Position(int X, int Y);

public class Solution
{
    private static int _width;
    private static int _height;
    private static string[,] _grid;
    private static Position _playerPos;
    private static List<Position> _enemyPositions = new();
    private static List<Position> _previousEnemyPositions = new();
    private static readonly HashSet<Position> _enemyVisitedTiles = new();

    private static readonly Dictionary<string, string> ActionToGameCommand = new()
    {
        { "UP", "C" }, { "RIGHT", "A" }, { "DOWN", "D" }, { "LEFT", "E" }, { "STAY", "B" }
    };

    public static void Main()
    {
        _width = int.Parse(Console.ReadLine());
        _height = int.Parse(Console.ReadLine());
        var playerCount = int.Parse(Console.ReadLine());

        _grid = new string[_height, _width];
        for (var y = 0; y < _height; ++y)
            for (var x = 0; x < _width; ++x)
                _grid[y, x] = "?";
        
        var moveToCommandName = new Dictionary<Position, string>();

        while (true)
        {
            ReadInputs(playerCount);
            
            moveToCommandName.Clear();
            moveToCommandName[new Position(Modulo(_playerPos.X - 1, _width), _playerPos.Y)] = "LEFT";
            moveToCommandName[new Position(Modulo(_playerPos.X + 1, _width), _playerPos.Y)] = "RIGHT";
            moveToCommandName[new Position(_playerPos.X, Modulo(_playerPos.Y - 1, _height))] = "UP";
            moveToCommandName[new Position(_playerPos.X, Modulo(_playerPos.Y + 1, _height))] = "DOWN";

            PrintMapToError();

            var nextMove = DecideNextMove();

            string actionName = "STAY";
            if (moveToCommandName.TryGetValue(nextMove, out var foundAction))
            {
                actionName = foundAction;
            }
            Console.WriteLine(ActionToGameCommand[actionName]);
        }
    }

    private static Position DecideNextMove()
    {
        var predictedEnemyMoves = PredictEnemyMoves();
        
        if (IsForbiddenByCurrentEnemy(_playerPos))
        {
            Console.Error.WriteLine("DEBUG: DANGER! Enemy is adjacent. Triggering EVASION.");
            return FindEscapeMove();
        }

        var candidatePaths = new List<(List<Position> path, double score)>();

        var opportunitySquares = GetOpportunitySquares();
        if (opportunitySquares.Any())
        {
            var pathsToOpportunities = FindShortestSafePaths(p => opportunitySquares.Contains(p), 5, predictedEnemyMoves);
            foreach(var path in pathsToOpportunities)
            {
                candidatePaths.Add((path, EvaluatePath(path, predictedEnemyMoves, true)));
            }
        }

        var pathsToUnknowns = FindShortestSafePaths(p => _grid[p.Y, p.X] == "?", 15, predictedEnemyMoves);
        foreach(var path in pathsToUnknowns)
        {
            candidatePaths.Add((path, EvaluatePath(path, predictedEnemyMoves, false)));
        }

        if(candidatePaths.Any())
        {
            var bestChoice = candidatePaths.OrderByDescending(c => c.score).First();
            Console.Error.WriteLine($"DEBUG: Best target: {bestChoice.path.Last()}, Score: {bestChoice.score:F2}");
            return bestChoice.path[0];
        }

        var safeWanderMove = GetSafeAdjacentMoves(_playerPos, predictedEnemyMoves).FirstOrDefault();
        if (safeWanderMove != null)
        {
            Console.Error.WriteLine("DEBUG: STRATEGY: Wandering to a safe adjacent tile.");
            return safeWanderMove;
        }
        
        Console.Error.WriteLine("DEBUG: TRAPPED! No safe moves found. Staying put as last resort.");
        return _playerPos;
    }
    
    private static double EvaluatePath(List<Position> path, List<Position> predictedEnemies, bool isOpportunity)
    {
        var target = path.Last();
        
        const double opportunityWeight = 50.0;
        const double unknownTileWeight = 100.0;
        const double pathCostWeight = -2.0;
        const double dangerWeight = -200.0;
        const double opennessWeight = 0.5;

        double score = 0;

        if(isOpportunity) score += opportunityWeight;
        if(_grid[target.Y, target.X] == "?") score += unknownTileWeight;
        
        score += path.Count * pathCostWeight;
        
        if (predictedEnemies.Any())
        {
            int minPredictedDist = predictedEnemies.Min(e => GetManhattanDistance(target, e));
            if (minPredictedDist > 0)
            {
                score += dangerWeight / minPredictedDist;
            }
        }
        
        score += CalculateOpennessScore(target, predictedEnemies) * opennessWeight;
        
        return score;
    }

    private static List<Position> PredictEnemyMoves()
    {
        var predictions = new List<Position>();
        foreach (var enemy in _enemyPositions)
        {
            var pathToPlayer = FindShortestPath(enemy, p => p.Equals(_playerPos), p => _grid[p.Y, p.X] != "#");
            if (pathToPlayer != null && pathToPlayer.Any())
            {
                predictions.Add(pathToPlayer[0]);
            }
            else 
            {
                predictions.Add(enemy);
            }
        }
        return predictions;
    }

    private static int CalculateOpennessScore(Position startPos, List<Position> predictedEnemies)
    {
        var queue = new Queue<Position>();
        var visited = new HashSet<Position> { startPos };
        queue.Enqueue(startPos);
        
        int reachableCount = 0;
        int depth = 0;
        const int maxDepth = 5;

        while (queue.Any() && depth < maxDepth)
        {
            int levelSize = queue.Count;
            for(int i = 0; i < levelSize; i++)
            {
                var current = queue.Dequeue();
                reachableCount++;
                
                foreach (var neighbor in GetSafeAdjacentMoves(current, predictedEnemies))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
            depth++;
        }
        return reachableCount;
    }

    private static bool IsForbiddenByCurrentEnemy(Position pos) => _enemyPositions.Any(enemy => GetManhattanDistance(pos, enemy) <= 1);

    private static bool IsForbiddenByPredictedEnemy(Position pos, List<Position> predictedEnemies) => predictedEnemies.Any(p => GetManhattanDistance(pos, p) <= 1);
    
    private static List<List<Position>> FindShortestSafePaths(Func<Position, bool> isTarget, int maxPaths, List<Position> predictedEnemies)
    {
        var foundPaths = new List<List<Position>>();
        var queue = new Queue<List<Position>>();
        var visited = new HashSet<Position> { _playerPos };
        queue.Enqueue(new List<Position> { _playerPos });

        while (queue.Any() && foundPaths.Count < maxPaths)
        {
            var path = queue.Dequeue();
            var current = path.Last();

            if (path.Count > 1 && isTarget(current))
            {
                path.RemoveAt(0);
                foundPaths.Add(path);
                if (foundPaths.Count >= maxPaths) break;
                continue;
            }
            if (path.Count > _width) continue;

            foreach (var neighbor in GetSafeAdjacentMoves(current, predictedEnemies))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    var newPath = new List<Position>(path) { neighbor };
                    queue.Enqueue(newPath);
                }
            }
        }
        return foundPaths;
    }
    
    private static List<Position> FindShortestPath(Position start, Func<Position, bool> isTarget, Func<Position, bool> isTraversable)
    {
        var queue = new Queue<List<Position>>();
        var visited = new HashSet<Position> { start };
        queue.Enqueue(new List<Position> { start });

        while (queue.Any())
        {
            var path = queue.Dequeue();
            var current = path.Last();

            if (path.Count > 1 && isTarget(current))
            {
                path.RemoveAt(0);
                return path;
            }
            
            var neighbors = new[]
            {
                new Position(current.X, Modulo(current.Y - 1, _height)), new Position(Modulo(current.X + 1, _width), current.Y),
                new Position(current.X, Modulo(current.Y + 1, _height)), new Position(Modulo(current.X - 1, _width), current.Y)
            };

            foreach (var neighbor in neighbors.Where(isTraversable))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    var newPath = new List<Position>(path) { neighbor };
                    queue.Enqueue(newPath);
                }
            }
        }
        return null;
    }

    private static Position FindEscapeMove() => new[]
        {
            new Position(_playerPos.X, Modulo(_playerPos.Y - 1, _height)), new Position(Modulo(_playerPos.X + 1, _width), _playerPos.Y),
            new Position(_playerPos.X, Modulo(_playerPos.Y + 1, _height)), new Position(Modulo(_playerPos.X - 1, _width), _playerPos.Y)
        }
        .Where(move => _grid[move.Y, move.X] != "#")
        .OrderByDescending(move => _enemyPositions.Any() ? _enemyPositions.Min(e => GetManhattanDistance(move, e)) : int.MaxValue)
        .FirstOrDefault() ?? _playerPos;
    
    private static HashSet<Position> GetOpportunitySquares() => _previousEnemyPositions.Except(_enemyPositions).ToHashSet();
    
    private static IEnumerable<Position> GetSafeAdjacentMoves(Position pos, List<Position> predictedEnemies) => new[]
        {
            new Position(pos.X, Modulo(pos.Y - 1, _height)), new Position(Modulo(pos.X + 1, _width), pos.Y),
            new Position(pos.X, Modulo(pos.Y + 1, _height)), new Position(Modulo(pos.X - 1, _width), pos.Y)
        }.Where(p => _grid[p.Y, p.X] != "#" && !IsForbiddenByCurrentEnemy(p) && !IsForbiddenByPredictedEnemy(p, predictedEnemies));
    
    private static void ReadInputs(int playerCount)
    {
        var up = Console.ReadLine();
        var right = Console.ReadLine();
        var down = Console.ReadLine();
        var left = Console.ReadLine();
        
        _previousEnemyPositions = new List<Position>(_enemyPositions);
        _enemyPositions.Clear();

        for (var i = 0; i < playerCount; ++i)
        {
            var parts = Console.ReadLine().Split();
            var px = Modulo(int.Parse(parts[0]) - 1, _width);
            var py = Modulo(int.Parse(parts[1]) - 1, _height);
            var pos = new Position(px, py);

            if (i == playerCount - 1) { _playerPos = pos; }
            else { _enemyPositions.Add(pos); }
        }

        if (_enemyPositions.Any(e => e.Equals(_playerPos)))
        {
            Console.Error.WriteLine("!!!!!! CAPTURED !!!!!!");
        }

        _enemyVisitedTiles.UnionWith(_enemyPositions);

        foreach (var visitedPos in _enemyVisitedTiles)
            if (_grid[visitedPos.Y, visitedPos.X] == "?")
                _grid[visitedPos.Y, visitedPos.X] = "_";

        _grid[_playerPos.Y, _playerPos.X] = "_";
        _grid[Modulo(_playerPos.Y - 1, _height), _playerPos.X] = up;
        _grid[Modulo(_playerPos.Y + 1, _height), _playerPos.X] = down;
        _grid[_playerPos.Y, Modulo(_playerPos.X - 1, _width)] = left;
        _grid[_playerPos.Y, Modulo(_playerPos.X + 1, _width)] = right;
    }
    
    private static void PrintMapToError()
    {
        var predicted = PredictEnemyMoves();
        for (var y = 0; y < _height; ++y)
        {
            for (var x = 0; x < _width; ++x)
            {
                var currentPos = new Position(x, y);
                if (_playerPos != null && _playerPos.Equals(currentPos)) Console.Error.Write('+');
                else if (_enemyPositions.Any(e => e.Equals(currentPos))) Console.Error.Write('@');
                else if (IsForbiddenByCurrentEnemy(currentPos) && _grid[y,x] != "#") Console.Error.Write('x');
                else if (predicted.Contains(currentPos) && _grid[y,x] != "#") Console.Error.Write('%');
                else Console.Error.Write(_grid[y, x]);
            }
            Console.Error.WriteLine();
        }
    }
    
    private static int GetManhattanDistance(Position p1, Position p2)
    {
        int dx = Math.Abs(p1.X - p2.X);
        int dy = Math.Abs(p1.Y - p2.Y);
        return Math.Min(dx, _width - dx) + Math.Min(dy, _height - dy);
    }

    private static int Modulo(int x, int m) => (x % m + m) % m;
}