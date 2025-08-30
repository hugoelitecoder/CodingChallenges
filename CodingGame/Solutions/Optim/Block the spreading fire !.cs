using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public record struct Point(int X, int Y);

public enum CellType { Safe, Tree, House }

public class Cell
{
    public Point Pos { get; }
    public CellType Type { get; }
    public int CutDuration { get; }
    public int FireDuration { get; }
    public int Value { get; }

    public Cell(int x, int y, CellType type, GameConfig config)
    {
        Pos = new Point(x, y);
        Type = type;
        switch (type)
        {
            case CellType.Tree:
                CutDuration = config.TreeCutDuration;
                FireDuration = config.TreeFireDuration;
                Value = config.TreeValue;
                break;
            case CellType.House:
                CutDuration = config.HouseCutDuration;
                FireDuration = config.HouseFireDuration;
                Value = config.HouseValue;
                break;
            default:
                CutDuration = int.MaxValue;
                FireDuration = int.MaxValue;
                Value = 0;
                break;
        }
    }
}

public class GameConfig
{
    public int TreeCutDuration { get; init; }
    public int TreeFireDuration { get; init; }
    public int TreeValue { get; init; }
    public int HouseCutDuration { get; init; }
    public int HouseFireDuration { get; init; }
    public int HouseValue { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}

public class TurnInput
{
    public int Cooldown { get; init; }
    public int[,] FireProgress { get; init; }
}

public class Solution
{
    private static GameConfig _config;
    private static Cell[,] _grid;
    private static bool _isFirstTurn = true;
    private const int SIMULATION_DEPTH = 30;

    public static void Main(string[] args)
    {
        ReadInitialInput();
        
        while (true)
        {
            var stopwatch = Stopwatch.StartNew();
            var turnInput = ReadTurnInput();

            if (turnInput.Cooldown > 0)
            {
                Console.WriteLine("WAIT");
            }
            else
            {
                var timeLimit = _isFirstTurn ? 4800 : 95;
                var bestAction = FindBestAction(turnInput, timeLimit);
                Console.WriteLine(bestAction);
            }
            
            stopwatch.Stop();
            Console.Error.WriteLine($"[DEBUG] Turn decision took {stopwatch.ElapsedMilliseconds} ms.");
            _isFirstTurn = false;
        }
    }
    
    public static string FindBestAction(TurnInput turnInput, long timeLimit)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var timeToBurn = CalculateTimeToBurn(turnInput.FireProgress);
        
        var candidates = new List<Cell>();
        for (var y = 0; y < _config.Height; y++)
        {
            for (var x = 0; x < _config.Width; x++)
            {
                if (turnInput.FireProgress[y, x] == -1)
                {
                    var cell = _grid[y, x];
                    if (cell.CutDuration < timeToBurn[y, x])
                    {
                        candidates.Add(cell);
                    }
                }
            }
        }
        
        Console.Error.WriteLine($"[DEBUG] Found {candidates.Count} viable candidates.");
        if (candidates.Count == 0)
        {
            return "WAIT";
        }
        
        candidates.Sort((a, b) => 
        {
            var scoreA = (double)a.Value / (a.CutDuration + 1);
            var scoreB = (double)b.Value / (b.CutDuration + 1);
            return scoreB.CompareTo(scoreA);
        });

        var bestCut = new Point(-1, -1);
        var bestScore = 0.0;
        
        var baselineLoss = Simulate(turnInput.FireProgress, null);
        Console.Error.WriteLine($"[DEBUG] Baseline loss if we wait: {baselineLoss}");
        
        var evaluatedCount = 0;
        foreach (var candidateCell in candidates)
        {
            if (stopwatch.ElapsedMilliseconds > timeLimit)
            {
                Console.Error.WriteLine($"[DEBUG] Time limit reached. Evaluated {evaluatedCount}/{candidates.Count} candidates.");
                break;
            }
            
            var lossWithCut = Simulate(turnInput.FireProgress, candidateCell.Pos);
            var valueSaved = baselineLoss - lossWithCut;
            var score = (double)valueSaved / (candidateCell.CutDuration + 1);

            if (score > bestScore)
            {
                bestScore = score;
                bestCut = candidateCell.Pos;
            }
            evaluatedCount++;
        }

        if (bestCut.X == -1)
        {
            Console.Error.WriteLine("[DEBUG] No beneficial cut found. Waiting.");
            return "WAIT";
        }

        var finalValueSaved = baselineLoss - Simulate(turnInput.FireProgress, bestCut);
        Console.Error.WriteLine($"[DEBUG] Best cut: {bestCut.X} {bestCut.Y} with score {bestScore:F2}. Estimated Value Saved: {finalValueSaved}");
        return $"{bestCut.X} {bestCut.Y}";
    }
    
    private static int Simulate(int[,] initialProgress, Point? cut)
    {
        var fireProgress = (int[,])initialProgress.Clone();
        var totalLoss = 0;

        if (cut.HasValue)
        {
            var p = cut.Value;
            totalLoss += _grid[p.Y, p.X].Value;
            fireProgress[p.Y, p.X] = -2;
        }
        
        for (var turn = 0; turn < SIMULATION_DEPTH; turn++)
        {
            var nextProgress = (int[,])fireProgress.Clone();
            var newlyIgnited = new HashSet<Point>();
            var fireIsActive = false;

            for (var y = 0; y < _config.Height; y++)
            {
                for (var x = 0; x < _config.Width; x++)
                {
                    var progress = fireProgress[y, x];
                    var cell = _grid[y, x];
                    if (progress >= 0 && progress < cell.FireDuration)
                    {
                        fireIsActive = true;
                        if (progress + 1 == cell.FireDuration)
                        {
                            totalLoss += cell.Value;
                            foreach (var n in GetNeighbors(x, y))
                            {
                                if (fireProgress[n.Y, n.X] == -1)
                                {
                                    newlyIgnited.Add(n);
                                }
                            }
                        }
                        nextProgress[y, x]++;
                    }
                }
            }

            foreach (var p in newlyIgnited)
            {
                if (nextProgress[p.Y, p.X] == -1)
                {
                    nextProgress[p.Y, p.X] = 0;
                }
            }
            
            fireProgress = nextProgress;
            if (!fireIsActive && newlyIgnited.Count == 0) break;
        }
        return totalLoss;
    }

    private static int[,] CalculateTimeToBurn(int[,] fireProgress)
    {
        var timeToBurn = new int[_config.Height, _config.Width];
        var pq = new PriorityQueue<Point, int>();

        for (var y = 0; y < _config.Height; y++)
        {
            for (var x = 0; x < _config.Width; x++)
            {
                timeToBurn[y, x] = int.MaxValue;
            }
        }
        
        for (var y = 0; y < _config.Height; y++)
        {
            for (var x = 0; x < _config.Width; x++)
            {
                if (fireProgress[y, x] >= 0)
                {
                    var cellOnFire = _grid[y, x];
                    var turnsUntilSpread = cellOnFire.FireDuration - fireProgress[y, x];
                    foreach (var n in GetNeighbors(x, y))
                    {
                        if (fireProgress[n.Y, n.X] == -1 && turnsUntilSpread < timeToBurn[n.Y, n.X])
                        {
                            timeToBurn[n.Y, n.X] = turnsUntilSpread;
                            pq.Enqueue(n, turnsUntilSpread);
                        }
                    }
                }
            }
        }
        
        while (pq.Count > 0)
        {
            var currentPos = pq.Dequeue();
            var timeToReachCurrent = timeToBurn[currentPos.Y, currentPos.X];
            var currentCell = _grid[currentPos.Y, currentPos.X];
            var timeToSpreadFromCurrent = timeToReachCurrent + currentCell.FireDuration;

            foreach (var neighbor in GetNeighbors(currentPos.X, currentPos.Y))
            {
                if (fireProgress[neighbor.Y, neighbor.X] == -1 && timeToSpreadFromCurrent < timeToBurn[neighbor.Y, neighbor.X])
                {
                    timeToBurn[neighbor.Y, neighbor.X] = timeToSpreadFromCurrent;
                    pq.Enqueue(neighbor, timeToSpreadFromCurrent);
                }
            }
        }
        return timeToBurn;
    }

    private static IEnumerable<Point> GetNeighbors(int x, int y)
    {
        if (y > 0) yield return new Point(x, y - 1);
        if (y < _config.Height - 1) yield return new Point(x, y + 1);
        if (x > 0) yield return new Point(x - 1, y);
        if (x < _config.Width - 1) yield return new Point(x + 1, y);
    }
    
    private static void ReadInitialInput()
    {
        var treeInputs = Console.ReadLine().Split(' ');
        var houseInputs = Console.ReadLine().Split(' ');
        var mapSizeInputs = Console.ReadLine().Split(' ');
        Console.ReadLine();
        
        _config = new GameConfig
        {
            TreeCutDuration = int.Parse(treeInputs[0]),
            TreeFireDuration = int.Parse(treeInputs[1]),
            TreeValue = int.Parse(treeInputs[2]),
            HouseCutDuration = int.Parse(houseInputs[0]),
            HouseFireDuration = int.Parse(houseInputs[1]),
            HouseValue = int.Parse(houseInputs[2]),
            Width = int.Parse(mapSizeInputs[0]),
            Height = int.Parse(mapSizeInputs[1]),
        };
        
        _grid = new Cell[_config.Height, _config.Width];
        var totalValue = 0;
        
        for (var i = 0; i < _config.Height; i++)
        {
            var gridLine = Console.ReadLine();
            for (var j = 0; j < _config.Width; j++)
            {
                CellType type;
                switch (gridLine[j])
                {
                    case '.': type = CellType.Tree; break;
                    case 'X': type = CellType.House; break;
                    default: type = CellType.Safe; break;
                }
                var cell = new Cell(j, i, type, _config);
                _grid[i, j] = cell;
                totalValue += cell.Value;
            }
        }
        Console.Error.WriteLine($"[DEBUG] Initial setup complete. Total map value: {totalValue}");
    }
    
    private static TurnInput ReadTurnInput()
    {
        var cooldown = int.Parse(Console.ReadLine());
        var fireProgress = new int[_config.Height, _config.Width];
        
        for (var i = 0; i < _config.Height; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            for (var j = 0; j < _config.Width; j++)
            {
                fireProgress[i, j] = int.Parse(inputs[j]);
            }
        }
        
        return new TurnInput { Cooldown = cooldown, FireProgress = fireProgress };
    }
}