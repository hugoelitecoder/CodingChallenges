using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public class Solution
{
    public static void Main()
    {
        var size = int.Parse(Console.ReadLine());
        var grid = new int[size * size];
        var offset = 0;
        for (var i = 0; i < size; i++)
        {
            var line = Console.ReadLine();
            for (var x = 0; x < size; x++)
            {
                grid[offset + x] = line[x] == '.' ? 0 : (int)char.GetNumericValue(line[x]);
            }
            offset += size;
        }

        var clock = Stopwatch.StartNew();

        var solver = new NurikabeSolver(size, grid);
        var finalStatus = solver.Solve();

        Console.Error.WriteLine($"Solved in {clock.ElapsedMilliseconds}ms");

        var finalGrid = new string[finalStatus.Length];
        for (var i = 0; i < finalGrid.Length; i++)
        {
            finalGrid[i] = finalStatus[i] == (int)GridCellState.Water ? "~" : grid[finalStatus[i]].ToString();
        }
        for (var y = 0; y < size; y++)
        {
            Console.WriteLine(string.Join("", finalGrid.AsSpan(size * y, size).ToArray()));
        }
    }
}

public enum GridCellState { Water = -2, Unassigned = -1 }

public class PuzzleContext
{
    public int Size { get; }
    public IReadOnlyList<Island> AllIslands { get; }
    private readonly int[][] _neighbors;

    public PuzzleContext(int size, IReadOnlyList<Island> allIslands, int[][] neighbors)
    {
        Size = size;
        AllIslands = allIslands;
        _neighbors = neighbors;
    }

    public int[] GetNeighbors(int pos) => _neighbors[pos];

    public bool IsSeaValid(ReadOnlySpan<int> status)
    {
        var total = 0;
        for (var i = 0; i < status.Length; i++)
        {
            if (status[i] == (int)GridCellState.Water)
            {
                if (i % Size < Size - 1 && i / Size < Size - 1 &&
                    status[i + 1] == (int)GridCellState.Water && status[i + Size] == (int)GridCellState.Water && status[i + Size + 1] == (int)GridCellState.Water) return false;
                total++;
            }
        }
        if (total == 0) return true;
        var waterStart = status.IndexOf((int)GridCellState.Water);
        if (waterStart == -1) return true;
        Span<int> queue = stackalloc int[status.Length];
        queue[0] = waterStart;
        var idx = 0;
        var end = 1;
        var count = 1;
        Span<bool> visited = stackalloc bool[status.Length];
        visited[queue[0]] = true;
        while (idx < end)
        {
            var cur = queue[idx++];
            foreach (var n in _neighbors[cur])
            {
                if (status[n] < 0 && !visited[n])
                {
                    visited[n] = true;
                    queue[end++] = n;
                    if (status[n] == (int)GridCellState.Water) count++;
                }
            }
        }
        return total == count;
    }
}


public struct ScenarioShape
{
    public Island Island { get; }
    public HashSet<int> Shape { get; }

    public ScenarioShape(Island island, HashSet<int> shape)
    {
        Island = island;
        Shape = shape;
    }
}

public class Island
{
    private readonly PuzzleContext _context;
    public readonly int RootPosition;
    public readonly int RequiredSize;
    public bool IsComplete { get; private set; }
    public HashSet<int> CoreCells { get; } = new HashSet<int>();
    private HashSet<int> CoreBoundary = new HashSet<int>();
    public HashSet<int> CandidateCells { get; } = new HashSet<int>();
    public HashSet<int> CandidateBoundary { get; private set; } = new HashSet<int>();
    public List<HashSet<int>> PossibleShapes { get; private set; }

    public Island(PuzzleContext context, int pos, int val, Span<int> status)
    {
        _context = context;
        RootPosition = pos;
        RequiredSize = val;
        IsComplete = false;
        if (RequiredSize == 1)
        {
            CoreCells.Add(RootPosition);
            Finalize(status);
        }
    }

    public void CalculateCandidateCells(Span<int> status)
    {
        if (IsComplete) return;
        CoreCells.Add(RootPosition);
        if (RequiredSize > 1)
        {
            var idx = 0;
            var end = 1;
            Span<int> dists = stackalloc int[status.Length];
            dists.Fill(int.MaxValue);
            dists[RootPosition] = 1;
            Span<int> queue = stackalloc int[status.Length];
            queue[0] = RootPosition;
            while (idx < end)
            {
                var cur = queue[idx++];
                if (dists[cur] >= RequiredSize) continue;
                foreach (var n in _context.GetNeighbors(cur))
                {
                    if (dists[n] > dists[cur] + 1 && IsValidExpansionCell(n, status))
                    {
                        CandidateCells.Add(n);
                        queue[end++] = n;
                        dists[n] = dists[cur] + 1;
                    }
                }
            }
        }
        CoreBoundary = FindBoundary(CoreCells, status);
        CandidateBoundary = FindBoundary(CandidateCells, status);
    }

    public void AddCellToCore(int pos, Span<int> status)
    {
        if (IsComplete || CoreCells.Contains(pos)) return;
        CoreCells.Add(pos);
        status[pos] = RootPosition;
        CandidateCells.Remove(pos);
        CoreBoundary = FindBoundary(CoreCells, status);
        CandidateBoundary = FindBoundary(CandidateCells, status);
        if (CoreCells.Count == RequiredSize)
        {
            Finalize(status);
        }
    }

    public void ApplyDeductions(Span<int> status)
    {
        if (IsComplete) return;
        foreach (var i in CandidateCells.ToList())
        {
            if (!IsValidExpansionCell(i, status) || _context.AllIslands.Any(other => other != this && (other.CoreCells.Contains(i) || other.CoreBoundary.Contains(i))))
            {
                CandidateCells.Remove(i);
            }
        }
        foreach (var n in CandidateCells.ToList())
        {
            if (!_context.GetNeighbors(n).Any(n2 => CandidateCells.Contains(n2) || CoreCells.Contains(n2)))
            {
                CandidateCells.Remove(n);
            }
        }

        var exits = new List<int>();
        foreach (var n in CoreBoundary)
        {
            if (status[n] == (int)GridCellState.Unassigned)
            {
                exits.Add(n);
            }
        }
        if (exits.Count == 1)
        {
            AddCellToCore(exits[0], status);
        }

        if (IsComplete) return;
        Span<int> copy = stackalloc int[status.Length];
        foreach (var n in CandidateCells.ToList())
        {
            status.CopyTo(copy);
            copy[n] = RootPosition;
            if (!_context.IsSeaValid(copy))
            {
                CandidateCells.Remove(n);
                if (!_context.AllIslands.Any(island => island.CandidateCells.Contains(n)))
                {
                    status[n] = (int)GridCellState.Water;
                }
            }
        }
        if (IsComplete) return;
        var group = new HashSet<int>(CoreCells);
        group.UnionWith(CandidateCells);
        foreach (var n in CandidateCells.ToList())
        {
            group.Remove(n);
            if (FindContiguousGroup(RootPosition, group).Count < RequiredSize)
            {
                AddCellToCore(n, status);
            }
            group.Add(n);
        }
        if (IsComplete) return;
        if (CoreCells.Count + CandidateCells.Count == RequiredSize)
        {
            foreach (var n in CandidateCells)
            {
                status[n] = RootPosition;
            }
            CoreCells.UnionWith(CandidateCells);
            Finalize(status);
        }
        else
        {
            CandidateBoundary = FindBoundary(CandidateCells, status);
            CoreBoundary = FindBoundary(CoreCells, status);
        }
    }

    public void GeneratePossibleShapes(Span<int> status)
    {
        PossibleShapes = new List<HashSet<int>> { CoreCells };
        for (var i = CoreCells.Count; i < RequiredSize; i++)
        {
            var nextShapes = PossibleShapes;
            PossibleShapes = new List<HashSet<int>>();
            foreach (var shape in nextShapes)
            {
                foreach (var pos in shape)
                {
                    foreach (var n in _context.GetNeighbors(pos))
                    {
                        if (CandidateCells.Contains(n) && !shape.Contains(n) &&
                            !PossibleShapes.Any(other => other.Contains(n) && other.IsSupersetOf(shape)))
                        {
                            PossibleShapes.Add(new HashSet<int>(shape) { n });
                        }
                    }
                }
            }
        }
        Span<int> copy = stackalloc int[status.Length];
        foreach (var shape in PossibleShapes.ToList())
        {
            if (FindContiguousGroup(RootPosition, shape).Count != RequiredSize)
            {
                PossibleShapes.Remove(shape);
            }
            else
            {
                status.CopyTo(copy);
                foreach (var n in shape) copy[n] = RootPosition;
                if (!_context.IsSeaValid(copy)) PossibleShapes.Remove(shape);
            }
        }
    }

    private bool IsValidExpansionCell(int n, Span<int> status)
    {
        if (status[n] != (int)GridCellState.Unassigned && status[n] != RootPosition) return false;
        foreach (var n2 in _context.GetNeighbors(n))
        {
            if (status[n2] >= 0 && status[n2] != RootPosition) return false;
        }
        return true;
    }

    private HashSet<int> FindBoundary(HashSet<int> source, Span<int> status)
    {
        var res = new HashSet<int>();
        foreach (var n in source)
        {
            foreach (var n2 in _context.GetNeighbors(n))
            {
                if (status[n2] == (int)GridCellState.Unassigned) res.Add(n2);
            }
        }
        return res;
    }

    private HashSet<int> FindContiguousGroup(int pos, HashSet<int> group)
    {
        var res = new HashSet<int> { pos };
        var queue = new Queue<int>();
        queue.Enqueue(pos);
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            foreach (var n in _context.GetNeighbors(cur))
            {
                if (group.Contains(n) && !res.Contains(n))
                {
                    res.Add(n);
                    queue.Enqueue(n);
                }
            }
        }
        return res;
    }

    private void Finalize(Span<int> status)
    {
        if (IsComplete) return;
        IsComplete = true;
        CoreBoundary = FindBoundary(CoreCells, status);
        foreach (var n in CoreBoundary)
        {
            if (status[n] == (int)GridCellState.Unassigned) status[n] = (int)GridCellState.Water;
        }
        CandidateCells.Clear();
        CandidateBoundary.Clear();
    }
}

public class IslandCluster
{
    private readonly PuzzleContext _context;
    public readonly List<Island> Islands = new List<Island>(16);
    public readonly List<List<ScenarioShape>> Scenarios = new List<List<ScenarioShape>>();

    public IslandCluster(PuzzleContext context, Island start)
    {
        _context = context;
        var i = 0;
        Islands.Add(start);
        while (i < Islands.Count)
        {
            var n = Islands[i++];
            foreach (var other in _context.AllIslands)
            {
                if (!other.IsComplete && !Islands.Contains(other) && (n.CandidateCells.Overlaps(other.CandidateCells) || n.CandidateBoundary.Overlaps(other.CandidateCells)))
                {
                    Islands.Add(other);
                }
            }
        }
    }

    public void GenerateAllScenarios(Span<int> status)
    {
        foreach (var island in Islands) island.GeneratePossibleShapes(status);
        GenerateScenariosRecursively(status, 0, new List<ScenarioShape>());
    }

    private bool IsScenarioValid(Span<int> status, List<ScenarioShape> scenario)
    {
        Span<int> copy = stackalloc int[status.Length];
        status.CopyTo(copy);
        foreach (var item in scenario)
        {
            foreach (var n in item.Shape) copy[n] = item.Island.RootPosition;
        }
        return _context.IsSeaValid(copy);
    }

    private void GenerateScenariosRecursively(Span<int> status, int idx, List<ScenarioShape> scenario)
    {
        foreach (var shape in Islands[idx].PossibleShapes)
        {
            if (!shape.Any(n => scenario.Any(sc => sc.Shape.Contains(n) || _context.GetNeighbors(n).Any(n2 => sc.Shape.Contains(n2)))))
            {
                var newScenario = new List<ScenarioShape>(scenario) { new ScenarioShape(Islands[idx], shape) };
                if (IsScenarioValid(status, newScenario))
                {
                    if (idx == Islands.Count - 1) Scenarios.Add(newScenario);
                    else GenerateScenariosRecursively(status, idx + 1, newScenario);
                }
            }
        }
    }
}

public class NurikabeSolver
{
    private readonly int _size;
    private readonly int[] _grid;
    private readonly PuzzleContext _context;
    private readonly List<Island> _allIslands = new List<Island>();

    public NurikabeSolver(int size, int[] grid)
    {
        _size = size;
        _grid = grid;
        var neighbors = new int[_size * _size][];
        for (var i = 0; i < _size * _size; i++) neighbors[i] = GetOrthogonalNeighbors(i);
        _context = new PuzzleContext(_size, _allIslands, neighbors);
    }

    public int[] Solve()
    {
        Span<int> status = stackalloc int[_size * _size];
        for (var i = 0; i < status.Length; i++) status[i] = _grid[i] > 0 ? i : (int)GridCellState.Unassigned;
        for (var i = 0; i < _grid.Length; i++)
        {
            if (_grid[i] > 0) _allIslands.Add(new Island(_context, i, _grid[i], status));
        }

        SeparateInitialIslands(status);
        var unassignedCount = CountUnassignedCells(status);
        int backupCount;
        do
        {
            backupCount = unassignedCount;
            ApplyLogicalRules(status, false);
            unassignedCount = CountUnassignedCells(status);
        } while (unassignedCount != backupCount);

        foreach (var island in _allIslands) island.CalculateCandidateCells(status);

        unassignedCount = CountUnassignedCells(status);
        do
        {
            backupCount = unassignedCount;
            ApplyLogicalRules(status, true);
            unassignedCount = CountUnassignedCells(status);
        } while (unassignedCount != backupCount);

        if (_allIslands.Any(n => !n.IsComplete))
        {
            var clusters = new List<IslandCluster>();
            foreach (var island in _allIslands)
            {
                if (!island.IsComplete && !clusters.Any(a => a.Islands.Contains(island)))
                {
                    clusters.Add(new IslandCluster(_context, island));
                }
            }
            foreach (var cluster in clusters) cluster.GenerateAllScenarios(status);
            SolveByScenarioAnalysis(status, clusters);
        }
        return status.ToArray();
    }

    private int CountUnassignedCells(Span<int> status)
    {
        var count = 0;
        for (var i = 0; i < status.Length; i++)
        {
            if (status[i] == (int)GridCellState.Unassigned) count++;
        }
        return count;
    }

    private int[] GetOrthogonalNeighbors(int pos)
    {
        var res = new List<int>(4);
        if (pos % _size > 0) res.Add(pos - 1);
        if (pos % _size < _size - 1) res.Add(pos + 1);
        if (pos >= _size) res.Add(pos - _size);
        if (pos < _size * (_size - 1)) res.Add(pos + _size);
        return res.ToArray();
    }

    private int[] GetDistantOrthogonalNeighbors(int pos)
    {
        var res = new List<int>(4);
        if (pos % _size > 1) res.Add(pos - 2);
        if (pos % _size < _size - 2) res.Add(pos + 2);
        if (pos >= 2 * _size) res.Add(pos - 2 * _size);
        if (pos < _size * (_size - 2)) res.Add(pos + 2 * _size);
        return res.ToArray();
    }

    private int[] GetDiagonalNeighbors(int pos)
    {
        var res = new List<int>(4);
        if (pos > _size && pos % _size > 0) res.Add(pos - _size - 1);
        if (pos > _size && pos % _size < _size - 1) res.Add(pos - _size + 1);
        if (pos < _size * (_size - 1) && pos % _size > 0) res.Add(pos + _size - 1);
        if (pos < _size * (_size - 1) && pos % _size < _size - 1) res.Add(pos + _size + 1);
        return res.ToArray();
    }

    private void SeparateInitialIslands(Span<int> status)
    {
        foreach (var island in _allIslands)
        {
            if (!island.IsComplete)
            {
                foreach (var n in GetDistantOrthogonalNeighbors(island.RootPosition))
                {
                    if (status[n] > 0) status[(island.RootPosition + n) / 2] = (int)GridCellState.Water;
                }
                foreach (var n in GetDiagonalNeighbors(island.RootPosition))
                {
                    if (status[n] > 0)
                    {
                        status[island.RootPosition % _size + _size * (n / _size)] = (int)GridCellState.Water;
                        status[n % _size + _size * (island.RootPosition / _size)] = (int)GridCellState.Water;
                    }
                }
            }
        }
    }

    private void ApplyLogicalRules(Span<int> status, bool includeIslandRules)
    {
        if (includeIslandRules)
        {
            for (var i = 0; i < status.Length; i++)
            {
                if (status[i] == (int)GridCellState.Unassigned)
                {
                    var isSurroundedByWater = true;
                    foreach (var n in _context.GetNeighbors(i))
                    {
                        if (status[n] != (int)GridCellState.Water)
                        {
                            isSurroundedByWater = false;
                            break;
                        }
                    }
                    if (isSurroundedByWater || _allIslands.All(n => n.IsComplete || !n.CandidateCells.Contains(i))) status[i] = (int)GridCellState.Water;
                }
            }
        }
        var visited = new HashSet<int>();
        var exitGroups = new List<HashSet<int>>();
        Span<int> queue = stackalloc int[status.Length];
        for (var i = 0; i < status.Length; i++)
        {
            if (status[i] == (int)GridCellState.Water && !visited.Contains(i))
            {
                exitGroups.Add(new HashSet<int>());
                queue[0] = i;
                var idx = 0;
                var end = 1;
                visited.Add(i);
                while (idx < end)
                {
                    var cur = queue[idx++];
                    foreach (var n in _context.GetNeighbors(cur))
                    {
                        if (status[n] == (int)GridCellState.Water && !visited.Contains(n))
                        {
                            queue[end++] = n;
                            visited.Add(n);
                        }
                        else if (status[n] == (int)GridCellState.Unassigned) exitGroups[^1].Add(n);
                    }
                }
            }
        }
        if (exitGroups.Count > 1)
        {
            foreach (var exits in exitGroups)
            {
                if (exits.Count == 1) status[exits.First()] = (int)GridCellState.Water;
            }
        }
        if (includeIslandRules)
        {
            for (var i = 0; i < status.Length; i++)
            {
                if (status[i] < 0 && i % _size < _size - 1 && i / _size < _size - 1)
                {
                    if (status[i] == (int)GridCellState.Unassigned)
                    {
                        var isThreeQuartersWater = true;
                        foreach (var j in new int[3] { i + 1, i + _size, i + _size + 1 }) isThreeQuartersWater &= status[j] == (int)GridCellState.Water;
                        if (isThreeQuartersWater)
                        {
                            var candidates = _allIslands.Where(n => !n.IsComplete && n.CandidateCells.Contains(i)).ToList();
                            if (candidates.Count == 1) candidates[0].AddCellToCore(i, status);
                        }
                    }
                    else
                    {
                        var waterCount = 0;
                        var unassignedCell = (int)GridCellState.Unassigned;
                        foreach (var j in new int[3] { i + 1, i + _size, i + _size + 1 })
                        {
                            if (status[j] == (int)GridCellState.Water) waterCount++;
                            else if (status[j] == (int)GridCellState.Unassigned) unassignedCell = j;
                        }
                        if (waterCount == 2 && unassignedCell != (int)GridCellState.Unassigned)
                        {
                            var candidates = _allIslands.Where(n => !n.IsComplete && n.CandidateCells.Contains(unassignedCell)).ToList();
                            if (candidates.Count == 1) candidates[0].AddCellToCore(unassignedCell, status);
                        }
                    }
                }
            }
            foreach (var island in _allIslands)
            {
                if (!island.IsComplete) island.ApplyDeductions(status);
            }
        }
    }

    private IEnumerable<List<ScenarioShape>> EnumerateScenarios(List<IslandCluster> clusters, int idx)
    {
        foreach (var scenario in clusters[idx].Scenarios)
        {
            if (idx == clusters.Count - 1) yield return new List<ScenarioShape>(scenario);
            else
            {
                foreach (var next in EnumerateScenarios(clusters, idx + 1))
                {
                    next.AddRange(scenario);
                    yield return next;
                }
            }
        }
    }

    private void SolveByScenarioAnalysis(Span<int> status, List<IslandCluster> clusters)
    {
        Span<int> copy = stackalloc int[status.Length];
        foreach (var scenario in EnumerateScenarios(clusters, 0))
        {
            status.CopyTo(copy);
            foreach (var item in scenario)
            {
                foreach (var n in item.Shape) copy[n] = item.Island.RootPosition;
            }
            for (var i = 0; i < copy.Length; i++)
            {
                if (copy[i] == (int)GridCellState.Unassigned) copy[i] = (int)GridCellState.Water;
            }
            if (_context.IsSeaValid(copy))
            {
                copy.CopyTo(status);
                return;
            }
        }
    }
}