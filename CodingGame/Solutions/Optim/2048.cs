using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Solution
{
    private const int GridSize = 4;
    private const int BeamWidth = 80;
    private const int SearchDepth = 8;
    private const ulong PrngMod = 50515093UL;
    private static readonly Dictionary<int, int> _log2Lookup = new Dictionary<int, int>();

    public static void Main(string[] args)
    {
        MoveTables.Init();
        for (var i = 1; i <= 17; i++)
        {
            _log2Lookup[1 << i] = i;
        }
        while (true)
        {
            var sw = Stopwatch.StartNew();
            var (seed, score, initialBoard) = ReadTurnInput();
            var debugBoard = new ulong[1];
            debugBoard[0] = initialBoard;
            PrintDebugBoard(debugBoard, "Input board:");
            var bestSequence = FindBestMoveSequence(initialBoard, score, seed);
            sw.Stop();
            Console.Error.WriteLine($"[DEBUG] Turn took {sw.ElapsedMilliseconds}ms. Chosen sequence: {bestSequence}");
            Console.WriteLine(bestSequence);
        }
    }

    public static string FindBestMoveSequence(ulong initialBoard, int initialScore, ulong initialSeed)
    {
        var initialNode = new SearchNode
        {
            Board = initialBoard,
            Score = initialScore,
            Seed = initialSeed,
            Path = "",
            HeuristicScore = Heuristics.Evaluate(initialBoard, initialScore)
        };
        var beam = new List<SearchNode> { initialNode };
        var nextBeamCandidates = new PriorityQueue<SearchNode, double>();
        for (var d = 0; d < SearchDepth; d++)
        {
            nextBeamCandidates.Clear();
            foreach (var node in beam)
            {
                for (var dir = 0; dir < 4; dir++)
                {
                    var (newBoard, scoreGained, moved) = BoardUtils.Move(node.Board, dir);
                    if (!moved) continue;
                    var currentSeed = node.Seed;
                    var newPath = node.Path + "UDLR"[dir];
                    var newScore = node.Score + scoreGained;
                    var emptyCellIndices = BoardUtils.GetEmptyCellIndices(newBoard);
                    if (emptyCellIndices.Count > 0)
                    {
                        var spawnListIndex = (int)(currentSeed % (ulong)emptyCellIndices.Count);
                        var cellBoardIndex = emptyCellIndices[spawnListIndex];
                        var spawnPower = (currentSeed & 0x10) == 0 ? 1 : 2;
                        var spawnedBoard = newBoard | ((ulong)spawnPower << (cellBoardIndex * 4));
                        var nextSeed = (currentSeed * currentSeed) % PrngMod;
                        var heuristic = Heuristics.Evaluate(spawnedBoard, newScore);
                        nextBeamCandidates.Enqueue(new SearchNode { Board = spawnedBoard, Score = newScore, Seed = nextSeed, Path = newPath, HeuristicScore = heuristic }, -heuristic);
                    }
                    else
                    {
                        var heuristic = Heuristics.Evaluate(newBoard, newScore);
                        nextBeamCandidates.Enqueue(new SearchNode { Board = newBoard, Score = newScore, Seed = currentSeed, Path = newPath, HeuristicScore = heuristic }, -heuristic);
                    }
                }
            }
            if (nextBeamCandidates.Count == 0) break;
            beam.Clear();
            var count = Math.Min(BeamWidth, nextBeamCandidates.Count);
            for (var i = 0; i < count; i++)
            {
                beam.Add(nextBeamCandidates.Dequeue());
            }
        }
        if (beam.Count > 0)
        {
            var bestNode = beam.OrderByDescending(n => n.HeuristicScore).First();
            PrintDebugBoard(new[] { bestNode }, "Best candidate found:");
            return bestNode.Path;
        }
        return "U";
    }

    private static (ulong seed, int score, ulong board) ReadTurnInput()
    {
        var seed = ulong.Parse(Console.ReadLine());
        var score = int.Parse(Console.ReadLine());
        ulong board = 0;
        for (var r = 0; r < GridSize; r++)
        {
            var inputs = Console.ReadLine().Split(' ');
            for (var c = 0; c < GridSize; c++)
            {
                var val = int.Parse(inputs[c]);
                if (val > 0)
                {
                    var power = _log2Lookup[val];
                    var index = r * GridSize + c;
                    board |= (ulong)power << (index * 4);
                }
            }
        }
        return (seed, score, board);
    }

    private static void PrintDebugBoard(IEnumerable<SearchNode> nodes, string message)
    {
        Console.Error.WriteLine($"[DEBUG] {message}");
        foreach (var node in nodes)
        {
            Console.Error.WriteLine($"[DEBUG] Path: {node.Path}, H: {node.HeuristicScore:F2}, S: {node.Score}");
            for (var r = 0; r < GridSize; r++)
            {
                var line = "";
                for (var c = 0; c < GridSize; c++)
                {
                    var p = BoardUtils.GetTile(node.Board, r, c);
                    line += (p == 0 ? "." : (1 << p).ToString()).PadLeft(5);
                }
                Console.Error.WriteLine($"[DEBUG] {line}");
            }
        }
    }

    private static void PrintDebugBoard(IEnumerable<ulong> boards, string message)
    {
        var nodes = boards.Select(b => new SearchNode { Board = b });
        PrintDebugBoard(nodes, message);
    }
}

public class SearchNode
{
    public ulong Board;
    public int Score;
    public ulong Seed;
    public string Path;
    public double HeuristicScore;
}

public static class BoardUtils
{
    private const int GridSize = 4;
    public static (ulong, int, bool) Move(ulong board, int direction)
    {
        ulong newBoard;
        int scoreGained;
        switch (direction)
        {
            case 0: // Up
                var transposedUp = Transpose(board);
                (newBoard, scoreGained) = MoveLeft(transposedUp);
                newBoard = Transpose(newBoard);
                break;
            case 1: // Down
                var transposedDown = Transpose(board);
                (newBoard, scoreGained) = MoveRight(transposedDown);
                newBoard = Transpose(newBoard);
                break;
            case 2: // Left
                (newBoard, scoreGained) = MoveLeft(board);
                break;
            default: // Right
                (newBoard, scoreGained) = MoveRight(board);
                break;
        }
        return (newBoard, scoreGained, newBoard != board);
    }

    private static (ulong, int) MoveLeft(ulong board)
    {
        ulong newBoard = 0;
        var totalScore = 0;
        for (var i = 0; i < GridSize; i++)
        {
            var row = (ushort)((board >> (i * 16)) & 0xFFFF);
            var (newRow, score) = MoveTables.GetLeftMove(row);
            newBoard |= (ulong)newRow << (i * 16);
            totalScore += score;
        }
        return (newBoard, totalScore);
    }

    private static (ulong, int) MoveRight(ulong board)
    {
        ulong newBoard = 0;
        var totalScore = 0;
        for (var i = 0; i < GridSize; i++)
        {
            var row = (ushort)((board >> (i * 16)) & 0xFFFF);
            var reversedRow = ReverseRow(row);
            var (movedReversedRow, score) = MoveTables.GetLeftMove(reversedRow);
            newBoard |= (ulong)ReverseRow(movedReversedRow) << (i * 16);
            totalScore += score;
        }
        return (newBoard, totalScore);
    }

    public static int GetTile(ulong board, int r, int c) => (int)((board >> ((r * GridSize + c) * 4)) & 0xF);

    public static List<int> GetEmptyCellIndices(ulong board)
    {
        var indices = new List<int>(16);
        for (var i = 0; i < GridSize * GridSize; i++)
        {
            if (((board >> (i * 4)) & 0xF) == 0)
            {
                indices.Add(i);
            }
        }
        return indices;
    }

    private static ushort ReverseRow(ushort row) => (ushort)(((row & 0xF000) >> 12) | ((row & 0x0F00) >> 4) | ((row & 0x00F0) << 4) | ((row & 0x000F) << 12));

    private static ulong Transpose(ulong board)
    {
        ulong transposedBoard = 0;
        for (var r = 0; r < GridSize; r++)
        {
            for (var c = 0; c < GridSize; c++)
            {
                var tile = (board >> ((r * GridSize + c) * 4)) & 0xF;
                if (tile != 0)
                {
                    transposedBoard |= tile << ((c * GridSize + r) * 4);
                }
            }
        }
        return transposedBoard;
    }
}

public static class MoveTables
{
    private const int GridSize = 4;
    private static (ushort, int)[] _leftMoveLookup;

    public static void Init()
    {
        _leftMoveLookup = new (ushort, int)[65536];
        for (uint i = 0; i < 65536; i++)
        {
            _leftMoveLookup[i] = PrecomputeMove((ushort)i);
        }
    }

    public static (ushort, int) GetLeftMove(ushort row) => _leftMoveLookup[row];

    private static (ushort, int) PrecomputeMove(ushort row)
    {
        var line = new int[GridSize];
        for (var i = 0; i < GridSize; i++) line[i] = (row >> (i * 4)) & 0xF;
        var packed = new int[GridSize];
        var current = 0;
        for (var i = 0; i < GridSize; i++)
        {
            if (line[i] != 0) packed[current++] = line[i];
        }
        var score = 0;
        var merged = new int[GridSize];
        current = 0;
        for (var i = 0; i < GridSize && packed[i] != 0; i++)
        {
            if (i + 1 < GridSize && packed[i] == packed[i + 1])
            {
                var val = packed[i] + 1;
                merged[current] = val;
                score += 1 << val;
                i++;
            }
            else merged[current] = packed[i];
            current++;
        }
        ushort resultRow = 0;
        for (var i = 0; i < GridSize; i++) resultRow |= (ushort)(merged[i] << (i * 4));
        return (resultRow, score);
    }
}

public static class Heuristics
{
    private const int GridSize = 4;
    private static readonly double EmptyWeight = 2.7;
    private static readonly double MonoWeight = 1.0;
    private static readonly double SmoothWeight = 0.1;
    private static readonly double MaxTileWeight = 1.0;
    private static readonly double ScoreWeight = 0.5;
    private static readonly double[] MonotonicityGridPenalty = { 10, 8, 7, 6.5, 5, 4, 3.5, 3, -0.5, -1.5, -2, -2.5, -4, -5, -6, -7 };

    public static double Evaluate(ulong board, int score)
    {
        var tiles = new int[16];
        var emptyCells = 0;
        var maxTile = 0;
        for (var i = 0; i < 16; i++)
        {
            tiles[i] = (int)((board >> (i * 4)) & 0xF);
            if (tiles[i] == 0) emptyCells++;
            if (tiles[i] > maxTile) maxTile = tiles[i];
        }
        double monoScore1 = 0, monoScore2 = 0, monoScore3 = 0, monoScore4 = 0;
        double smoothScore = 0;
        for (var r = 0; r < GridSize; r++)
        {
            for (var c = 0; c < GridSize; c++)
            {
                var idx = r * GridSize + c;
                monoScore1 += MonotonicityGridPenalty[idx] * tiles[idx];
                monoScore2 += MonotonicityGridPenalty[idx] * tiles[15 - idx];
                monoScore3 += MonotonicityGridPenalty[idx] * tiles[r * GridSize + (3 - c)];
                monoScore4 += MonotonicityGridPenalty[idx] * tiles[(3 - r) * GridSize + c];
                var tilePower = tiles[idx];
                if (tilePower > 0)
                {
                    if (c < GridSize - 1)
                    {
                        var rightPower = tiles[idx + 1];
                        if (rightPower > 0) smoothScore -= Math.Abs(tilePower - rightPower);
                    }
                    if (r < GridSize - 1)
                    {
                        var downPower = tiles[idx + 4];
                        if (downPower > 0) smoothScore -= Math.Abs(tilePower - downPower);
                    }
                }
            }
        }
        var monoScore = Math.Max(Math.Max(monoScore1, monoScore2), Math.Max(monoScore3, monoScore4));
        return score * ScoreWeight
               + emptyCells * EmptyWeight
               + monoScore * MonoWeight
               + smoothScore * SmoothWeight
               + maxTile * MaxTileWeight;
    }
}