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
        string[] inputs = Console.ReadLine().Split(' ');
        var W = int.Parse(inputs[0]);
        var H = int.Parse(inputs[1]);
        var board = new char[H][];
        var startPos = (-1, -1);
        for (var i = 0; i < H; i++)
        {
            var row = Console.ReadLine();
            board[i] = row.ToCharArray();
            var bIndex = row.IndexOf('B');
            if (bIndex != -1)
            {
                startPos = (bIndex, i);
            }
        }
        var solver = new KnightPathSolver(W, H, board, startPos);
        var result = solver.FindShortestPath();
        Console.WriteLine(result);
    }
}

class KnightPathSolver
{
    private readonly int _w;
    private readonly int _h;
    private readonly char[][] _board;
    private readonly (int X, int Y) _start;
    private static readonly (int dX, int dY)[] KnightMoves =
    {
        (1, 2), (1, -2), (-1, 2), (-1, -2),
        (2, 1), (2, -1), (-2, 1), (-2, -1)
    };

    public KnightPathSolver(int w, int h, char[][] board, (int X, int Y) start)
    {
        _w = w;
        _h = h;
        _board = board;
        _start = start;
    }

    public object FindShortestPath()
    {
        if (_start.X == -1)
        {
            return "Impossible";
        }
        var q = new Queue<(int X, int Y)>();
        var visited = new HashSet<(int X, int Y)>();
        q.Enqueue(_start);
        visited.Add(_start);
        var turns = 0;
        while (q.Count > 0)
        {
            var levelSize = q.Count;
            for (var i = 0; i < levelSize; i++)
            {
                var pos = q.Dequeue();
                if (_board[pos.Y][pos.X] == 'E')
                {
                    return turns;
                }
                foreach (var move in KnightMoves)
                {
                    var nextX = pos.X + move.dX;
                    var nextY = pos.Y + move.dY;
                    var nextPos = (nextX, nextY);
                    if (IsValid(nextPos) && !visited.Contains(nextPos))
                    {
                        visited.Add(nextPos);
                        q.Enqueue(nextPos);
                    }
                }
            }
            turns++;
        }
        return "Impossible";
    }

    private bool IsValid((int X, int Y) pos)
    {
        if (pos.X < 0 || pos.X >= _w || pos.Y < 0 || pos.Y >= _h)
        {
            return false;
        }
        if (_board[pos.Y][pos.X] == '#')
        {
            return false;
        }
        return true;
    }
}
