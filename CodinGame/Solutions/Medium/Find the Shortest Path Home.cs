using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        string route = Console.ReadLine();
        var pathfinder = new HomePathFinder();
        var solutions = pathfinder.FindReturnPaths(route);
        solutions.Sort(StringComparer.Ordinal);
        foreach (var solution in solutions)
        {
            Console.WriteLine(solution);
        }
    }
}

public class HomePathFinder
{
    public List<string> FindReturnPaths(string route)
    {
        var usedSegments = new HashSet<(Point, Point)>();
        var currentPos = new Point(0, 0);
        foreach (var moveChar in route)
        {
            var prevPos = currentPos;
            currentPos = GetNextPosition(currentPos, moveChar);
            usedSegments.Add(GetCanonicalSegment(prevPos, currentPos));
        }
        var endPointOfRoute = currentPos;
        var startPointOfRoute = new Point(0, 0);
        if (endPointOfRoute.Equals(startPointOfRoute))
        {
            return new List<string>();
        }
        return FindShortestPathsBFS(endPointOfRoute, startPointOfRoute, usedSegments);
    }

    private List<string> FindShortestPathsBFS(Point startNode, Point targetNode, ISet<(Point, Point)> forbiddenSegments)
    {
        var solutions = new List<string>();
        var minPathLength = int.MaxValue;
        var queue = new Queue<(Point Position, string Path)>();
        queue.Enqueue((startNode, ""));
        var distances = new Dictionary<Point, int> { [startNode] = 0 };
        var returnMoves = new[] { 'E', 'S', 'W' };
        while (queue.Count > 0)
        {
            var (currentPos, currentPath) = queue.Dequeue();
            if (currentPath.Length >= minPathLength)
            {
                continue;
            }
            foreach (var moveChar in returnMoves)
            {
                var nextPos = GetNextPosition(currentPos, moveChar);
                var newPath = currentPath + moveChar;
                var segment = GetCanonicalSegment(currentPos, nextPos);
                if (forbiddenSegments.Contains(segment))
                {
                    continue;
                }
                if (nextPos.Equals(targetNode))
                {
                    if (newPath.Length < minPathLength)
                    {
                        minPathLength = newPath.Length;
                        solutions.Clear();
                    }
                    solutions.Add(newPath);
                    continue;
                }
                if (distances.TryGetValue(nextPos, out var existingDist) && existingDist < newPath.Length)
                {
                    continue;
                }
                distances[nextPos] = newPath.Length;
                queue.Enqueue((nextPos, newPath));
            }
        }
        return solutions;
    }

    private Point GetNextPosition(Point p, char direction)
    {
        return direction switch
        {
            'N' => p with { Y = p.Y + 1 },
            'S' => p with { Y = p.Y - 1 },
            'E' => p with { X = p.X + 1 },
            'W' => p with { X = p.X - 1 },
            _ => p,
        };
    }

    private (Point, Point) GetCanonicalSegment(Point p1, Point p2)
    {
        if (p1.X < p2.X || (p1.X == p2.X && p1.Y < p2.Y))
        {
            return (p1, p2);
        }
        return (p2, p1);
    }

    private record struct Point(int X, int Y);
}