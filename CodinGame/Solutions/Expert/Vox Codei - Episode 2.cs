using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    static void Main(string[] args)
    {
        var dimensionInputs = Console.ReadLine().Split(' ');
        int width = int.Parse(dimensionInputs[0]);
        int height = int.Parse(dimensionInputs[1]);

        var game = new VoxCodei2Game(width, height);
        int turn = 0;

        while (true)
        {
            turn++;
            string roundInfo = Console.ReadLine();
            var gridLines = new string[height];
            for (int y = 0; y < height; y++)
            {
                gridLines[y] = Console.ReadLine();
            }

            game.Update(turn, roundInfo, gridLines);

            var debugGrid = game.GenerateDebugMap();
            PrintDebugMap(debugGrid, turn, height, width);

            string action = game.ComputeAction();

            if (turn == 4)
            {
                PrintPlanDetails(game.CalculatedPlan, game.PlanCalculationTimeMs);
            }

            Console.WriteLine(action);
        }
    }

    private static void PrintPlanDetails(IEnumerable<BombPlan> plan, long timeMs)
    {
        if (plan == null) return;

        Console.Error.WriteLine($"[DEBUG] Plan calculated in {timeMs} ms.");
        Console.Error.WriteLine($"[DEBUG] Bombing plan has {plan.Count()} step(s):");
        foreach (var step in plan)
        {
            int placementTurn = step.Turn - VoxCodei2Game.BombDelay;
            Console.Error.WriteLine($"[DEBUG]  - Place bomb at ({step.Point.X},{step.Point.Y}) on turn {placementTurn} to explode on turn {step.Turn}.");
        }
    }

    private static void PrintDebugMap(char[,] debugGrid, int turn, int height, int width)
    {
        Console.Error.WriteLine($"[DEBUG] Turn {turn}");
        for (int y = 0; y < height; y++)
        {
            var row = new StringBuilder();
            for (int x = 0; x < width; x++)
            {
                row.Append(debugGrid[x, y]);
            }
            Console.Error.WriteLine($"[DEBUG] {row}");
        }
    }
}

public readonly record struct Point(int X, int Y)
{
    public static Point FromPos(int position, int width) => new(position % width, position / width);
    public int ToPos(int width) => Y * width + X;
    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    public static Point operator *(Point p, int scalar) => new(p.X * scalar, p.Y * scalar);
}

public readonly record struct BombPlan(int Turn, Point Point);
public readonly record struct NodeMovement(int Position, int Speed);
public readonly record struct BombingInfo(Point Point, int Hits);

public struct AreaScanInfo
{
    public Point Point { get; }
    public int Hits { get; set; }
    public bool HasNode { get; set; }

    public AreaScanInfo(Point point, int hits, bool hasNode)
    {
        Point = point;
        Hits = hits;
        HasNode = hasNode;
    }
}

public class GridHistory
{
    private readonly Dictionary<int, char[]> grids = new();
    public void Add(int turn, char[] grid) => grids[turn] = grid;
    public char[] Get(int turn) => grids.GetValueOrDefault(turn);
    public char[] Turn1 => Get(1);
    public char[] Turn2 => Get(2);
    public char[] Turn3 => Get(3);
}

public class GameState
{
    public int BombCount, TurnCount, Turn;
    public char[] CurrentGrid;
    public readonly GridHistory InitialGrids = new();
    public readonly List<Node> Nodes = new();
    public readonly List<Area> Areas = new();
}

public readonly struct Node
{
    public int Id { get; }
    public Point[] Path { get; }

    public Node(int id, Point[] path)
    {
        Id = (int)Math.Pow(2, id);
        Path = path;
    }
}

public class Area
{
    public int Target { get; }
    public bool[] Cover { get; }
    public List<BombingInfo>[] Infos { get; }

    public Area(int target, bool[] cover, List<BombingInfo>[] infos)
    {
        Target = target;
        Cover = cover;
        Infos = infos;
    }
}

public class VoxCodei2Game
{
    private const int MaxSearchDepth = 50;
    public const char WallChar = '#', NodeChar = '@';
    public const int BombDelay = 3, BombRange = 3;
    private static readonly Point[] Directions = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

    private readonly int width, height, gridSize;
    private readonly GameState State = new();
    private List<BombPlan> actions = null;
    private bool[] destroyedNodes;

    private int SolverTarget, SolverBombsLeft;
    private Area SolverArea;
    private HashSet<int>[] SolverDone;
    private readonly HashSet<int> SolverFailures = new();
    private readonly HashSet<int> SolverExcludedTurns = new();

    public IEnumerable<BombPlan> CalculatedPlan { get; private set; }
    public long PlanCalculationTimeMs { get; private set; } = -1;

    public VoxCodei2Game(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.gridSize = width * height;
    }

    public void Update(int turn, string roundInfo, string[] gridLines)
    {
        State.Turn = turn;
        State.CurrentGrid = ParseGrid(gridLines);

        if (actions != null)
        {
            foreach (var plan in actions.Where(p => p.Turn == State.Turn))
            {
                var explosionPoints = new HashSet<Point>(BombHits(plan.Point));
                for (int i = 0; i < State.Nodes.Count; i++)
                {
                    if (destroyedNodes[i]) continue;
                    if (State.Turn < State.Nodes[i].Path.Length)
                    {
                        Point nodePos = State.Nodes[i].Path[State.Turn];
                        if (explosionPoints.Contains(nodePos))
                        {
                            destroyedNodes[i] = true;
                        }
                    }
                }
            }
        }

        if (turn <= 3)
        {
            State.InitialGrids.Add(turn, State.CurrentGrid);
        }

        if (turn == 1)
        {
            var inputs = roundInfo.Split(' ');
            State.TurnCount = int.Parse(inputs[0]) + 1;
            State.BombCount = int.Parse(inputs[1]);
        }
    }

    public char[,] GenerateDebugMap()
    {
        var debugGrid = new char[width, height];
        var baseGrid = State.CurrentGrid ?? State.InitialGrids.Turn1;
        if (baseGrid == null) return debugGrid;

        for (int i = 0; i < gridSize; i++)
        {
            debugGrid[i % width, i / width] = baseGrid[i];
        }

        if (State.Nodes.Any())
        {
            for (int i = 0; i < gridSize; i++)
            {
                if (debugGrid[i % width, i / width] == NodeChar)
                {
                    debugGrid[i % width, i / width] = '.';
                }
            }

            for (int i = 0; i < State.Nodes.Count; i++)
            {
                if (destroyedNodes[i]) continue;

                var node = State.Nodes[i];
                if (State.Turn < node.Path.Length)
                {
                    Point nodePos = node.Path[State.Turn];
                    char nodeChar = i < 10 ? (char)('0' + i) : (char)('A' + i - 10);
                    debugGrid[nodePos.X, nodePos.Y] = nodeChar;
                }
            }

            if (actions != null)
            {
                foreach (var plan in actions)
                {
                    int placementTurn = plan.Turn - BombDelay;
                    if (State.Turn >= placementTurn && State.Turn < plan.Turn)
                    {
                        debugGrid[plan.Point.X, plan.Point.Y] = 'B';
                    }
                }

                foreach (var plan in actions.Where(p => p.Turn == State.Turn))
                {
                    foreach (var explosionPoint in BombHits(plan.Point))
                    {
                        debugGrid[explosionPoint.X, explosionPoint.Y] = 'X';
                    }
                }
            }
        }

        return debugGrid;
    }

    public string ComputeAction()
    {
        if (State.Turn == 3)
        {
            IdentifyNodes();
            ScanAreas();
        }
        else if (State.Turn == 4)
        {
            var stopwatch = Stopwatch.StartNew();
            actions = OptimizeBombing().SelectMany(actionList => actionList).OrderBy(action => action.Turn).ToList();
            stopwatch.Stop();

            PlanCalculationTimeMs = stopwatch.ElapsedMilliseconds;
            CalculatedPlan = actions;
        }

        var action = actions?.Find(action => action.Turn - BombDelay == State.Turn) ?? default;
        if (action != default)
            return $"{action.Point.X} {action.Point.Y}";

        return "WAIT";
    }

    private char[] ParseGrid(string[] gridLines)
    {
        if (gridLines == null) return null;
        var grid = new char[gridSize];
        for (int y = 0; y < height; y++)
            gridLines[y].ToCharArray().CopyTo(grid, y * width);
        return grid;
    }

    private IEnumerable<Point> GetNeighbors(Point point)
    {
        foreach (var direction in Directions)
        {
            var neighbor = point + direction;
            if (neighbor.X >= 0 && neighbor.X < width && neighbor.Y >= 0 && neighbor.Y < height)
            {
                yield return neighbor;
            }
        }
    }

    private IEnumerable<Point> BombHits(Point point)
    {
        yield return point;
        foreach (var direction in Directions)
        {
            for (int distance = 1; distance <= BombRange; distance++)
            {
                var nextPoint = point + direction * distance;
                if (nextPoint.X < 0 || nextPoint.X >= width || nextPoint.Y < 0 || nextPoint.Y >= height || State.InitialGrids.Turn1[nextPoint.ToPos(width)] == WallChar)
                {
                    break;
                }
                yield return nextPoint;
            }
        }
    }

    private void IdentifyNodes()
    {
        var staticGrid = State.InitialGrids.Turn1;
        static int countNodes(char[] grid) => grid.Count(c => c == NodeChar);
        NodeMovement nextTurn(int position, int speed)
        {
            if (speed == 1 && (position % width == width - 1 || staticGrid[position + 1] == WallChar) ||
                speed == -1 && (position % width == 0 || staticGrid[position - 1] == WallChar) ||
                speed == width && (position >= width * (height - 1) || staticGrid[position + width] == WallChar) ||
                speed == -width && (position < width || staticGrid[position - width] == WallChar))
                speed = -speed;
            return new NodeMovement(position + speed, speed);
        }

        var offsets = new int[5] { 0, 1, -1, width, -width };
        bool noDuplicates = countNodes(staticGrid) >= Math.Max(countNodes(State.InitialGrids.Turn2), countNodes(State.InitialGrids.Turn3));

        for (int position = 0; position < gridSize; position++)
        {
            if (staticGrid[position] != NodeChar) continue;

            foreach (var movement1 in offsets.Select(speed => nextTurn(position, speed)))
            {
                if (State.InitialGrids.Turn2[movement1.Position] != NodeChar) continue;

                var movement2 = nextTurn(movement1.Position, movement1.Speed);
                if (State.InitialGrids.Turn3[movement2.Position] != NodeChar) continue;

                var path = new Point[State.TurnCount];
                path[1] = Point.FromPos(position, width);
                path[2] = Point.FromPos(movement1.Position, width);

                int currentPos = movement2.Position;
                int currentSpeed = movement2.Speed;
                for (int i = 3; i < State.TurnCount; i++)
                {
                    path[i] = Point.FromPos(currentPos, width);
                    var nextMovement = nextTurn(currentPos, currentSpeed);
                    currentPos = nextMovement.Position;
                    currentSpeed = nextMovement.Speed;
                }

                State.Nodes.Add(new Node(State.Nodes.Count, path));
                if (noDuplicates) break;
            }
        }
        destroyedNodes = new bool[State.Nodes.Count];
    }

    private void ScanAreas()
    {
        for (int position = 0; position < gridSize; position++)
            if (State.InitialGrids.Turn1[position] != WallChar && !State.Areas.Any(area => area.Cover[position]))
            {
                var area = ScanArea(Point.FromPos(position, width));
                if (area != null) State.Areas.Add(area);
            }
    }

    private Area ScanArea(Point startPoint)
    {
        var cover = new bool[gridSize];
        bool containsNode = false;
        var queue = new Queue<Point>(gridSize);

        queue.Enqueue(startPoint);
        cover[startPoint.ToPos(width)] = true;

        while (queue.Count > 0)
        {
            var currentPoint = queue.Dequeue();
            foreach (var neighbor in GetNeighbors(currentPoint))
            {
                int neighborPosition = neighbor.ToPos(width);
                if (State.InitialGrids.Turn1[neighborPosition] != WallChar && !cover[neighborPosition])
                {
                    containsNode |= State.InitialGrids.Turn1[neighborPosition] == NodeChar;
                    queue.Enqueue(neighbor);
                    cover[neighborPosition] = true;
                }
            }
        }
        if (!containsNode) return null;

        int target = 0;
        var infos = new Dictionary<Point, AreaScanInfo>[State.TurnCount];
        for (int turn = 0; turn < State.TurnCount; turn++)
            infos[turn] = new Dictionary<Point, AreaScanInfo>();

        foreach (var node in State.Nodes)
        {
            if (State.Turn >= node.Path.Length || !cover[node.Path[State.Turn].ToPos(width)]) continue;
            target |= node.Id;
            for (int turn = State.Turn; turn < State.TurnCount; turn++)
            {
                Point nodeLocation = node.Path[turn];
                if (turn < State.TurnCount - BombDelay)
                {
                    if (infos[turn + BombDelay].TryGetValue(nodeLocation, out var info))
                    {
                        info.HasNode = true;
                        infos[turn + BombDelay][nodeLocation] = info;
                    }
                    else
                    {
                        infos[turn + BombDelay][nodeLocation] = new AreaScanInfo(nodeLocation, 0, true);
                    }
                }

                if (!infos[turn].ContainsKey(nodeLocation))
                    infos[turn][nodeLocation] = new AreaScanInfo(nodeLocation, node.Id, false);

                foreach (Point hitLocation in BombHits(nodeLocation))
                {
                    if (infos[turn].TryGetValue(hitLocation, out var info))
                    {
                        if ((info.Hits & node.Id) == 0)
                        {
                            info.Hits |= node.Id;
                            infos[turn][hitLocation] = info;
                        }
                    }
                    else
                    {
                        infos[turn][hitLocation] = new AreaScanInfo(hitLocation, node.Id, false);
                    }
                }
            }
        }

        var cleanedUpInfos = new List<BombingInfo>[State.TurnCount];
        for (int i = 0; i < infos.Length; i++)
            cleanedUpInfos[i] = infos[i].Values
                .Where(info => !info.HasNode && info.Hits != 0)
                .Select(info => new BombingInfo(info.Point, info.Hits))
                .OrderByDescending(info => info.Hits)
                .Take(MaxSearchDepth)
                .ToList();

        return new Area(target, cover, cleanedUpInfos);
    }

    private List<List<BombPlan>> OptimizeBombing()
    {
        SolverBombsLeft = State.BombCount;
        SolverDone = Enumerable.Range(0, State.TurnCount).Select(_ => new HashSet<int>()).ToArray();
        var sequences = new List<List<BombPlan>>(State.Areas.Count);
        foreach (var area in State.Areas)
        {
            var sequence = FindSequenceForArea(area);
            SolverBombsLeft -= sequence.Count;
            foreach (var action in sequence) _ = SolverExcludedTurns.Add(action.Turn);
            sequences.Add(sequence);
        }
        return sequences;
    }

    private List<BombPlan> FindSequenceForArea(Area area)
    {
        SolverArea = area;
        SolverTarget = 0;
        int firstTurn = State.Turn + BombDelay;

        foreach (var node in State.Nodes)
        {
            if (firstTurn < node.Path.Length && area.Cover[node.Path[firstTurn].ToPos(width)])
            {
                SolverTarget |= node.Id;
            }
        }

        for (int bombCount = 1; bombCount <= SolverBombsLeft; bombCount++)
        {
            SolverFailures.Clear();
            var sequence = FindSequenceRecursive(firstTurn, bombCount, 0);
            if (sequence != null)
            {
                sequence.Reverse();
                return sequence;
            }
        }
        throw new InvalidOperationException("No sequence found");
    }

    private List<BombPlan> FindSequenceRecursive(int turn, int bombsLeft, int currentCoverage)
    {
        if (SolverExcludedTurns.Contains(turn))
            return turn != State.TurnCount - 1 ? FindSequenceRecursive(turn + 1, bombsLeft, currentCoverage) : null;
        int hash = HashCode.Combine(turn, bombsLeft, currentCoverage);
        if (SolverFailures.Contains(hash)) return null;

        SolverDone[turn].Clear();
        bool followUp = bombsLeft > 1 && turn != State.TurnCount - 1;
        foreach (var bombingInfo in SolverArea.Infos[turn])
        {
            if (!SolverDone[turn].Add(bombingInfo.Hits)) continue;
            int nextCoverage = currentCoverage | bombingInfo.Hits;
            if (nextCoverage == currentCoverage) continue;
            if (nextCoverage == SolverTarget) return new List<BombPlan> { new(turn, bombingInfo.Point) };
            if (followUp)
            {
                var sequence = FindSequenceRecursive(turn + 1, bombsLeft - 1, nextCoverage);
                if (sequence != null)
                {
                    sequence.Add(new BombPlan(turn, bombingInfo.Point));
                    return sequence;
                }
            }
        }
        var resultOfPassing = turn != State.TurnCount - 1 ? FindSequenceRecursive(turn + 1, bombsLeft, currentCoverage) : null;
        if (resultOfPassing == null) _ = SolverFailures.Add(hash);
        return resultOfPassing;
    }
}