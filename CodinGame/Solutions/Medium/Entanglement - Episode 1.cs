using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
public class Solution
{
    public static void Main(string[] args)
    {
        var game = new EntanglementGame();
        var turnCounter = 0;
        var totalTimer = new Stopwatch();
        while (true)
        {
            var initialLine = Console.ReadLine();
            if (string.IsNullOrEmpty(initialLine)) break;
            turnCounter++;
            var turnTimer = Stopwatch.StartNew();
            totalTimer.Start();
            var tileConnections = new int[12];
            game.ParseConnection(initialLine, tileConnections);
            for (var i = 0; i < 5; i++) game.ParseConnection(Console.ReadLine(), tileConnections);
            var result = game.ProcessTurn(tileConnections);
            Console.WriteLine(result.CurrentTotalScore);
            Console.WriteLine(result.PathDescription);
            turnTimer.Stop();
            totalTimer.Stop();
            Console.Error.WriteLine($"[DEBUG] --- TURN {turnCounter} STATS ---");
            Console.Error.WriteLine($"[DEBUG] Turn Score: {result.SegmentsThisTurn}");
            Console.Error.WriteLine($"[DEBUG] Total Score: {result.CurrentTotalScore}");
            Console.Error.WriteLine($"[DEBUG] Total Path Segments: {game.TotalSegmentsTraversed}");
            Console.Error.WriteLine($"[DEBUG] Turn Time: {turnTimer.ElapsedMilliseconds}ms");
            Console.Error.WriteLine($"[DEBUG] Avg Turn Time: {totalTimer.ElapsedMilliseconds / turnCounter}ms");
            Console.Error.WriteLine($"[DEBUG] Next Placement: ({game.NextEmptyCell.X}, {game.NextEmptyCell.Y})");
        }
    }
}
public record HexCoordinate(int X, int Y);
public record PathSegment(HexCoordinate Location, int EntryEntrance, int ExitEntrance)
{
    public override string ToString() => $"{Location.X} {Location.Y} {Math.Min(EntryEntrance, ExitEntrance)} {Math.Max(EntryEntrance, ExitEntrance)}";
}
public class TurnSummary
{
    public int CurrentTotalScore { get; set; }
    public int SegmentsThisTurn { get; set; }
    public string PathDescription { get; set; } = string.Empty;
}
public class EntanglementGame
{
    public HexCoordinate NextEmptyCell { get; private set; } = new HexCoordinate(0, -1);
    public int TotalSegmentsTraversed { get; private set; } = 0;
    private int _entryEntranceForNextTile = 0;
    private int _accumulatedScore = 0;
    private readonly Dictionary<HexCoordinate, int[]> _placedTiles = new Dictionary<HexCoordinate, int[]>();
    private static readonly int[] _oppositeEntranceMap = { 7, 6, 9, 8, 11, 10, 1, 0, 3, 2, 5, 4 };
    private static readonly int[] _deltaX = { 0, 0, 1, 1, 1, 1, 0, 0, -1, -1, -1, -1 };
    private static readonly int[] _deltaY = { 1, 1, 1, 1, 0, 0, -1, -1, -1, -1, 0, 0 };
    private static readonly HashSet<HexCoordinate> _validPlayArea = new HashSet<HexCoordinate>
    {
        new(0,-3),new(0,-2),new(0,-1),new(0,1),new(0,2),new(0,3),new(1,-2),new(1,-1),new(1,0),new(1,1),new(1,2),new(1,3),new(2,-1),new(2,0),new(2,1),new(2,2),new(2,3),new(3,0),new(3,1),new(3,2),new(3,3),
        new(-1,2),new(-1,1),new(-1,0),new(-1,-1),new(-1,-2),new(-1,-3),new(-2,1),new(-2,0),new(-2,-1),new(-2,-2),new(-2,-3),new(-3,0),new(-3,-1),new(-3,-2),new(-3,-3)
    };
    public TurnSummary ProcessTurn(int[] tileConnections)
    {
        var currentPosition = NextEmptyCell;
        var currentEntry = _entryEntranceForNextTile;
        _placedTiles[currentPosition] = (int[])tileConnections.Clone();
        var segmentsVisitedThisTurn = new List<PathSegment>();
        var segmentsPassedInSequence = 0;
        while (true)
        {
            var tileInternalPaths = _placedTiles[currentPosition];
            var exitEntrance = tileInternalPaths[currentEntry];
            segmentsVisitedThisTurn.Add(new PathSegment(currentPosition, currentEntry, exitEntrance));
            _accumulatedScore += ++segmentsPassedInSequence;
            TotalSegmentsTraversed++;
            var neighborPosition = new HexCoordinate(currentPosition.X + _deltaX[exitEntrance], currentPosition.Y + _deltaY[exitEntrance]);
            var neighborEntry = _oppositeEntranceMap[exitEntrance];
            if ((neighborPosition.X == 0 && neighborPosition.Y == 0) || !_validPlayArea.Contains(neighborPosition))
            {
                NextEmptyCell = neighborPosition;
                _entryEntranceForNextTile = neighborEntry;
                break;
            }
            if (!_placedTiles.ContainsKey(neighborPosition))
            {
                NextEmptyCell = neighborPosition;
                _entryEntranceForNextTile = neighborEntry;
                break;
            }
            currentPosition = neighborPosition;
            currentEntry = neighborEntry;
        }
        return new TurnSummary 
        { 
            CurrentTotalScore = _accumulatedScore,
            SegmentsThisTurn = segmentsPassedInSequence,
            PathDescription = string.Join("; ", segmentsVisitedThisTurn) 
        };
    }
    public void ParseConnection(string line, int[] connections)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var entranceA = int.Parse(parts[0]);
        var entranceB = int.Parse(parts[1]);
        connections[entranceA] = entranceB;
        connections[entranceB] = entranceA;
    }
}