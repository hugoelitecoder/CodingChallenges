using System;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    static void Main(string[] args)
    {
        var initialTiles = new int[12];
        var tileIndex = 0;
        for (var i = 0; i < 3; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            for (var j = 0; j < 4; j++)
            {
                initialTiles[tileIndex++] = int.Parse(inputs[j]);
            }
        }
        
        var initialBoard = new Board(initialTiles);
        var solver = new SlidePuzzleSolver();
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var stats = solver.Solve(initialBoard);
        stopwatch.Stop();
        
        Console.Error.WriteLine($"Solver finished in: {stopwatch.ElapsedMilliseconds}ms");
        Console.Error.WriteLine($"Iterations (nodes expanded): {stats.Iterations}");
        Console.Error.WriteLine($"Max open set size: {stats.MaxOpenSetSize}");
        
        if (stats.Path != null)
        {
            Console.Error.WriteLine($"Path length: {stats.Path.Count} moves");
            foreach (var move in stats.Path)
            {
                Console.WriteLine($"{move.Row} {move.Col}");
            }
        }
        else
        {
            Console.Error.WriteLine("No solution found or solution exceeds 50 moves.");
        }
    }
}

readonly struct Board : IEquatable<Board>
{
    private const int Cols = 4;
    private readonly ulong _state;

    public Board(IReadOnlyList<int> tiles)
    {
        var s = 0UL;
        for (var i = 0; i < 12; i++)
        {
            s |= (ulong)tiles[i] << (i * 4);
        }
        _state = s;
    }

    private Board(ulong state)
    {
        _state = state;
    }

    public int GetTile(int row, int col)
    {
        var index = row * Cols + col;
        return (int)((_state >> (index * 4)) & 0xF);
    }
    
    public Position GetZeroPosition()
    {
        for (var i = 0; i < 12; i++)
        {
            if (((_state >> (i * 4)) & 0xF) == 0)
            {
                return new Position(i / Cols, i % Cols);
            }
        }
        return new Position(-1, -1);
    }

    public Board CreateBoardByMovingTile(int tileRow, int tileCol)
    {
        var zeroPosition = GetZeroPosition();
        var tileIndex = tileRow * Cols + tileCol;
        var zeroIndex = zeroPosition.Row * Cols + zeroPosition.Col;
        var tileValue = GetTile(tileRow, tileCol);
        var newState = _state;
        newState &= ~(0xFUL << (tileIndex * 4));
        newState |= (ulong)tileValue << (zeroIndex * 4);
        return new Board(newState);
    }

    public bool Equals(Board other)
    {
        return _state == other._state;
    }

    public override bool Equals(object obj)
    {
        return obj is Board other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _state.GetHashCode();
    }
}

class Node
{
    public Board State { get; }
    public Node Parent { get; }
    public Position Move { get; }
    public int GScore { get; }
    public int FScore { get; }

    public Node(Board state, Node parent, Position move, int gScore, int hScore)
    {
        State = state;
        Parent = parent;
        Move = move;
        GScore = gScore;
        FScore = gScore + hScore;
    }
}

class Position
{
    public int Row { get; }
    public int Col { get; }

    public Position(int row, int col)
    {
        Row = row;
        Col = col;
    }
}

class SlidePuzzleSolver
{
    private const int Rows = 3;
    private const int Cols = 4;
    private const int MaxMoves = 50;

    private static readonly Board GoalBoard;
    private static readonly Dictionary<int, Position> GoalPositions = new();

    public struct SolverStats
    {
        public List<Position> Path;
        public int Iterations;
        public int MaxOpenSetSize;
    }
    
    static SlidePuzzleSolver()
    {
        var goalTiles = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        GoalBoard = new Board(goalTiles);
        for (var i = 0; i < goalTiles.Length; i++)
        {
            var value = goalTiles[i];
            if (value != 0)
            {
                GoalPositions[value] = new Position(i / Cols, i % Cols);
            }
        }
    }

    public SolverStats Solve(Board initialBoard)
    {
        var openSet = new PriorityQueue<Node, int>();
        var gScores = new Dictionary<Board, int>();
        var iterations = 0;
        var maxOpenSetSize = 0;
        var hScore = CalculateHeuristic(initialBoard);
        var startNode = new Node(initialBoard, null, new Position(-1, -1), 0, hScore);
        openSet.Enqueue(startNode, startNode.FScore);
        gScores[initialBoard] = 0;
        
        while (openSet.Count > 0)
        {
            maxOpenSetSize = Math.Max(maxOpenSetSize, openSet.Count);
            var currentNode = openSet.Dequeue();
            iterations++;
            if (currentNode.State.Equals(GoalBoard))
            {
                return new SolverStats { Path = ReconstructPath(currentNode), Iterations = iterations, MaxOpenSetSize = maxOpenSetSize };
            }
            if (gScores.TryGetValue(currentNode.State, out var currentG) && currentG < currentNode.GScore)
            {
                continue;
            }
            var zeroPosition = currentNode.State.GetZeroPosition();
            var dr = new[] { -1, 1, 0, 0 };
            var dc = new[] { 0, 0, -1, 1 };
            for (var i = 0; i < 4; i++)
            {
                var tileToMoveRow = zeroPosition.Row + dr[i];
                var tileToMoveCol = zeroPosition.Col + dc[i];
                if (tileToMoveRow >= 0 && tileToMoveRow < Rows && tileToMoveCol >= 0 && tileToMoveCol < Cols)
                {
                    var tentativeGScore = currentNode.GScore + 1;
                    if (tentativeGScore > MaxMoves) continue;
                    var move = new Position(tileToMoveRow, tileToMoveCol);
                    var neighborBoard = currentNode.State.CreateBoardByMovingTile(move.Row, move.Col);
                    if (tentativeGScore < gScores.GetValueOrDefault(neighborBoard, int.MaxValue))
                    {
                        gScores[neighborBoard] = tentativeGScore;
                        var neighborHScore = CalculateHeuristic(neighborBoard);
                        var neighborNode = new Node(neighborBoard, currentNode, move, tentativeGScore, neighborHScore);
                        openSet.Enqueue(neighborNode, neighborNode.FScore);
                    }
                }
            }
        }
        return new SolverStats { Path = null, Iterations = iterations, MaxOpenSetSize = maxOpenSetSize };
    }

    private int CalculateHeuristic(Board board)
    {
        var distance = 0;
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Cols; c++)
            {
                var value = board.GetTile(r, c);
                if (value == 0) continue;
                var goalPos = GoalPositions[value];
                distance += Math.Abs(r - goalPos.Row) + Math.Abs(c - goalPos.Col);
            }
        }
        return distance;
    }

    private List<Position> ReconstructPath(Node endNode)
    {
        var path = new List<Position>();
        var current = endNode;
        while (current.Parent != null)
        {
            path.Add(current.Move);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }
}
