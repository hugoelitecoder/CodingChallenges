using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var inputs = Console.ReadLine().Split(' ');
        var h = int.Parse(inputs[0]);
        var w = int.Parse(inputs[1]);
        
        var map = new char[h, w];
        Point2D start = null;
        Point2D end = null;
        
        for (var i = 0; i < h; i++)
        {
            var row = Console.ReadLine();
            for (var j = 0; j < w; j++)
            {
                map[i, j] = row[j];
                if (row[j] == 'B')
                {
                    start = new Point2D(j, i);
                }
                else if (row[j] == 'M')
                {
                    end = new Point2D(j, i);
                }
            }
        }

        var pathfinder = new Pathfinder(map, h, w, start, end);
        var distance = pathfinder.FindShortestDistance();
        var leagues = distance == 1 ? "league" : "leagues";
        Console.WriteLine($"{distance} {leagues}");
    }
}

public record Point2D(int X, int Y);

public class Pathfinder
{
    private readonly char[,] _map;
    private readonly int _h;
    private readonly int _w;
    private readonly Point2D _start;
    private readonly Point2D _end;

    private static readonly int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
    private static readonly int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

    public Pathfinder(char[,] map, int h, int w, Point2D start, Point2D end)
    {
        _map = map;
        _h = h;
        _w = w;
        _start = start;
        _end = end;
    }

    public int FindShortestDistance()
    {
        var queue = new Queue<(Point2D Pos, int Dist)>();
        var visited = new HashSet<Point2D>();
        
        queue.Enqueue((_start, 0));
        visited.Add(_start);

        while (queue.Count > 0)
        {
            var (currentPos, currentDist) = queue.Dequeue();

            if (currentPos.Equals(_end))
            {
                return currentDist;
            }

            for (var i = 0; i < 8; i++)
            {
                var moveX = dx[i];
                var moveY = dy[i];
                var nextPos = new Point2D(currentPos.X + moveX, currentPos.Y + moveY);

                if (visited.Contains(nextPos))
                {
                    continue;
                }

                if (IsMountain(nextPos))
                {
                    continue;
                }
                
                var isDiagonal = moveX != 0 && moveY != 0;
                if (isDiagonal)
                {
                    var corner1 = new Point2D(currentPos.X + moveX, currentPos.Y);
                    var corner2 = new Point2D(currentPos.X, currentPos.Y + moveY);
                    if (IsMountain(corner1) && IsMountain(corner2))
                    {
                        continue;
                    }
                }

                visited.Add(nextPos);
                queue.Enqueue((nextPos, currentDist + 1));
            }
        }
        
        return -1;
    }

    private bool IsMountain(Point2D p)
    {
        if (p.Y >= 0 && p.Y < _h && p.X >= 0 && p.X < _w)
        {
            return _map[p.Y, p.X] == '^';
        }
        return false;
    }
}