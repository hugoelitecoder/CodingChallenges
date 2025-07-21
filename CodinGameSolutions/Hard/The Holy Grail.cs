using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var line1 = Console.ReadLine().Split(' ');
        var width = int.Parse(line1[0]);
        var height = int.Parse(line1[1]);
        var pathFinder = new PathFinder(width, height);
        var runNumber = 1;
        while (true)
        {
            var lineTile = Console.ReadLine().Split(' ');
            var tileX = int.Parse(lineTile[0]);
            var tileY = int.Parse(lineTile[1]);
            if (pathFinder.Add(tileX, tileY))
            {
                Console.WriteLine(runNumber);
                return;
            }
            runNumber++;
        }
    }
}

class PathFinder
{
    private readonly int _width;
    private readonly int _height;
    private bool[,] _matrix;
    private bool[,] _reachable;

    private static readonly int[] Dx = { 0, 0, 1, -1 };
    private static readonly int[] Dy = { 1, -1, 0, 0 };

    public PathFinder(int width, int height)
    {
        _width = width;
        _height = height;
        
        _matrix = new bool[_width, _height];
        _reachable = new bool[_width, _height];

        _matrix[0, 0] = true;
        _reachable[0, 0] = true;
        
        _matrix[_width - 1, _height - 1] = true;
    }

    public bool Add(int tileX, int tileY)
    {
        _matrix[tileX, tileY] = true;
        var newTileIsConnectedToStart = false;
        if (tileX == 0 && tileY == 0)
        {
            newTileIsConnectedToStart = true;
        }
        else
        {
            for (var i = 0; i < 4; i++)
            {
                var neighborX = tileX + Dx[i];
                var neighborY = tileY + Dy[i];
                if (neighborX >= 0 && neighborX < _width &&
                    neighborY >= 0 && neighborY < _height)
                {
                    if (_reachable[neighborX, neighborY])
                    {
                        newTileIsConnectedToStart = true;
                        break;
                    }
                }
            }
        }
        if (newTileIsConnectedToStart)
        {
            return ExploreFrom(tileX, tileY);
        }
        return false;
    }

    private bool ExploreFrom(int startX, int startY)
    {
        if (_reachable[startX, startY])
        {
            if (startX == _width - 1 && startY == _height - 1) return true;
        }
        
        var q = new Queue<(int, int)>();
        _reachable[startX, startY] = true;
        q.Enqueue((startX, startY));
        while (q.Count > 0)
        {
            var (currX, currY) = q.Dequeue();
            if (currX == _width - 1 && currY == _height - 1)
            {
                return true;
            }
            for (var i = 0; i < 4; i++)
            {
                var nextX = currX + Dx[i];
                var nextY = currY + Dy[i];
                if (nextX >= 0 && nextX < _width &&
                    nextY >= 0 && nextY < _height)
                {
                    if (_matrix[nextX, nextY] && !_reachable[nextX, nextY])
                    {
                        _reachable[nextX, nextY] = true;
                        q.Enqueue((nextX, nextY));
                    }
                }
            }
        }
        return false;
    }
}

