using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Solution
{
    private static List<Move> _solutionPath;
    private static int _moveIndex = 0;

    public static void Main(string[] args)
    {
        var maxTurns = int.Parse(Console.ReadLine());
        var deadCount = int.Parse(Console.ReadLine());
        var deadCells = new HashSet<Pos>();
        for (var i = 0; i < deadCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            deadCells.Add(new Pos(int.Parse(inputs[0]), int.Parse(inputs[1])));
        }

        var solver = new BoardSolver(maxTurns, deadCells);

        while (true)
        {
            var cellCount = int.Parse(Console.ReadLine());
            var cells = new List<Cell>();
            for (var i = 0; i < cellCount; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                cells.Add(new Cell(int.Parse(inputs[0]), new Pos(int.Parse(inputs[1]), int.Parse(inputs[2]))));
            }

            if (_solutionPath == null)
            {
                Console.Error.WriteLine($"[DEBUG] Max Turns: {maxTurns}");
                var stopwatch = Stopwatch.StartNew();
                _solutionPath = solver.FindSolutionPath(new BoardState(cells));
                stopwatch.Stop();
                Console.Error.WriteLine($"[DEBUG] BFS finished in {stopwatch.ElapsedMilliseconds} ms.");

                if (_solutionPath == null) {
                    Console.Error.WriteLine("[DEBUG] No solution found! Using empty path as fallback.");
                    _solutionPath = new List<Move>(); 
                } else {
                    Console.Error.WriteLine($"[DEBUG] Solution found with {_solutionPath.Count} moves.");
                }
            }

            if (_moveIndex < _solutionPath.Count)
            {
                var nextMove = _solutionPath[_moveIndex++];
                Console.WriteLine($"{nextMove.MoleculeId} {nextMove.Dir}");
            }
            else
            {
                Console.Error.WriteLine("[DEBUG] Path exhausted or no solution found. Sending default move.");
                Console.WriteLine("0 LEFT");
            }
        }
    }
}

public readonly record struct Cell(int Id, Pos Pos) : IComparable<Cell>
{
    public int CompareTo(Cell other)
    {
        int idCmp = Id.CompareTo(other.Id);
        if (idCmp != 0) return idCmp;
        int xCmp = Pos.X.CompareTo(other.Pos.X);
        if (xCmp != 0) return xCmp;
        return Pos.Y.CompareTo(other.Pos.Y);
    }
}

public class BoardSolver
{
    private readonly int _maxTurns;
    private readonly HashSet<Pos> _deadCells;

    private static readonly Direction[] Directions = { Direction.LEFT, Direction.UP, Direction.RIGHT, Direction.DOWN };
    private static readonly Pos[] Deltas = {
        new(-1, 0), // LEFT
        new(0, -1), // UP
        new(1, 0),  // RIGHT
        new(0, 1)   // DOWN
    };

    public BoardSolver(int maxTurns, HashSet<Pos> deadCells)
    {
        _maxTurns = maxTurns;
        _deadCells = deadCells;
    }

    public List<Move> FindSolutionPath(BoardState initialState)
    {
        var queue = new Queue<(BoardState state, int depth)>();
        var visited = new HashSet<BoardState> { initialState };
        var predecessors = new Dictionary<BoardState, (BoardState parent, Move move)>();

        queue.Enqueue((initialState, 0));

        while (queue.Count > 0)
        {
            var (currentState, currentDepth) = queue.Dequeue();

            if (currentState.IsWinState())
            {
                return ReconstructPath(currentState, predecessors, initialState);
            }
            if (currentDepth >= _maxTurns) continue;

            var moleculeIds = currentState.GetMoleculeIds();
            foreach (var moleculeId in moleculeIds)
            {
                foreach (Direction dir in Directions)
                {
                    var move = new Move(moleculeId, dir);
                    var (success, nextState) = TryApplyMove(currentState, move);
                    if (success && visited.Add(nextState))
                    {
                        predecessors[nextState] = (currentState, move);
                        queue.Enqueue((nextState, currentDepth + 1));
                    }
                }
            }
        }
        return null;
    }

    private List<Move> ReconstructPath(BoardState goal, Dictionary<BoardState, (BoardState parent, Move move)> predecessors, BoardState initial)
    {
        var path = new LinkedList<Move>();
        var current = goal;
        while (!current.Equals(initial))
        {
            var (parent, move) = predecessors[current];
            path.AddFirst(move);
            current = parent;
        }
        return path.ToList();
    }

    private bool IsInBounds(Pos p)
    {
        if (p.Equals(Pos.Exit)) return true;
        return Math.Abs(p.X - 3) + Math.Abs(p.Y - 3) <= 3;
    }

    private long CalculateChainReactionMask(BoardState state, int initialMoleculeId, Pos delta)
    {
        var occupationMap = state.GetOccupationMap();
        var cellsById = state.GetCellsById();
        
        if (initialMoleculeId >= 64 || !cellsById.Contains(initialMoleculeId)) return 0;

        long moleculesInChainMask = 1L << initialMoleculeId;
        var processQueue = new Queue<int>();
        processQueue.Enqueue(initialMoleculeId);

        while (processQueue.Count > 0)
        {
            var currentId = processQueue.Dequeue();
            foreach (var cell in cellsById[currentId])
            {
                var frontierPos = new Pos(cell.Pos.X + delta.X, cell.Pos.Y + delta.Y);
                if (occupationMap.TryGetValue(frontierPos, out var hitId))
                {
                    long hitBit = 1L << hitId;
                    if ((moleculesInChainMask & hitBit) == 0)
                    {
                        moleculesInChainMask |= hitBit;
                        processQueue.Enqueue(hitId);
                    }
                }
            }
        }
        return moleculesInChainMask;
    }
    
    private (bool, BoardState) TryApplyMove(BoardState currentState, Move move)
    {
        var delta = Deltas[(int)move.Dir];
        long moleculesToMoveMask = CalculateChainReactionMask(currentState, move.MoleculeId, delta);
        if (moleculesToMoveMask == 0) return (false, null);

        var staticCells = new List<Cell>();
        var movedCellsWithNewPos = new List<Cell>();
        
        foreach (var cell in currentState.AllCells)
        {
            if ((moleculesToMoveMask & (1L << cell.Id)) != 0)
            {
                var newPos = new Pos(cell.Pos.X + delta.X, cell.Pos.Y + delta.Y);
                if (!IsInBounds(newPos) || _deadCells.Contains(newPos))
                {
                    return (false, null);
                }
                movedCellsWithNewPos.Add(new Cell(cell.Id, newPos));
            }
            else
            {
                staticCells.Add(cell);
            }
        }

        var nextState = BoardState.CreateFromPartitions(staticCells, movedCellsWithNewPos);
        return (true, nextState);
    }
}

public class BoardState : IEquatable<BoardState>
{
    public readonly IReadOnlyList<Cell> AllCells;
    private readonly int _hashCode;
    
    private ILookup<int, Cell> _cellsById;
    private IReadOnlyDictionary<Pos, int> _occupationMap;
    private IReadOnlyList<int> _moleculeIds;

    public BoardState(IEnumerable<Cell> cells)
    {
        AllCells = cells.OrderBy(c => c).ToList();
        _hashCode = ComputeHashCode();
    }
    
    private BoardState(List<Cell> sortedCells)
    {
        AllCells = sortedCells;
        _hashCode = ComputeHashCode();
    }
    
    public static BoardState CreateFromPartitions(List<Cell> staticCells, List<Cell> movedCells)
    {
        movedCells.Sort();

        var finalCells = new List<Cell>(staticCells.Count + movedCells.Count);
        int staticIdx = 0, movedIdx = 0;

        while (staticIdx < staticCells.Count || movedIdx < movedCells.Count)
        {
            if (staticIdx < staticCells.Count && (movedIdx >= movedCells.Count || staticCells[staticIdx].CompareTo(movedCells[movedIdx]) <= 0))
            {
                finalCells.Add(staticCells[staticIdx++]);
            }
            else
            {
                finalCells.Add(movedCells[movedIdx++]);
            }
        }
        return new BoardState(finalCells);
    }
    
    public ILookup<int, Cell> GetCellsById() => _cellsById ??= AllCells.ToLookup(c => c.Id);
    public IReadOnlyDictionary<Pos, int> GetOccupationMap() => _occupationMap ??= AllCells.ToDictionary(c => c.Pos, c => c.Id);

    private IReadOnlyList<int> ComputeMoleculeIds()
    {
        if (AllCells.Count == 0) return Array.Empty<int>();
        var ids = new List<int>();
        int lastId = -1;
        foreach(var cell in AllCells)
        {
            if(cell.Id != lastId)
            {
                ids.Add(cell.Id);
                lastId = cell.Id;
            }
        }
        return ids;
    }
    public IReadOnlyList<int> GetMoleculeIds() => _moleculeIds ??= ComputeMoleculeIds();
    
    public bool IsWinState() => AllCells.Any(c => c.Id == 0 && c.Pos.Equals(Pos.Exit));
    
    private int ComputeHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var cell in AllCells)
            {
                hash = hash * 31 + cell.GetHashCode();
            }
            return hash;
        }
    }
    
    public override int GetHashCode() => _hashCode;
    public override bool Equals(object obj) => Equals(obj as BoardState);
    public bool Equals(BoardState other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_hashCode != other._hashCode || AllCells.Count != other.AllCells.Count) return false;
        return AllCells.SequenceEqual(other.AllCells);
    }
}

public readonly record struct Pos(int X, int Y)
{
    public static readonly Pos Exit = new(-1, 3);
}

public readonly record struct Move(int MoleculeId, Direction Dir);

public enum Direction { LEFT, UP, RIGHT, DOWN }