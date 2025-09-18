using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

class Solution
{
    static void Main(string[] args)
    {
        int numPuzzles = int.Parse(Console.ReadLine());
        var puzzleLayouts = new string[numPuzzles];
        for (int i = 0; i < numPuzzles; i++)
        {
            puzzleLayouts[i] = Console.ReadLine();
        }
        var puzzleSums = new string[numPuzzles];
        for (int i = 0; i < numPuzzles; i++)
        {
            puzzleSums[i] = Console.ReadLine();
        }

        var totalGrid = new int[9, 9];
        long totalTime = 0;
        for (int i = 0; i < numPuzzles; i++)
        {
            var sw = Stopwatch.StartNew();
            var killerSudokuSolver = new KillerSudokuSolver(puzzleLayouts[i], puzzleSums[i]);
            var solvedGrid = killerSudokuSolver.Solve();
            sw.Stop();
            totalTime += sw.ElapsedMilliseconds;
            Console.Error.WriteLine($"[DEBUG] Puzzle {i} solved in {sw.ElapsedMilliseconds} ms.");
            if (solvedGrid != null)
            {
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        totalGrid[r, c] += solvedGrid[r, c];
                    }
                }
            }
        }
        Console.Error.WriteLine($"[DEBUG] ALL Puzzles solved in {totalTime} ms.");

        var sb = new StringBuilder();
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                sb.Append(totalGrid[r, c]);
                if (c < 8) sb.Append(" ");
            }
            if (r < 8) sb.AppendLine();
        }
        Console.WriteLine(sb.ToString());
    }
}

public class KillerSudokuSolver
{
    private static readonly Dictionary<(int, int), List<List<int>>> ComboCache = new();
    private readonly List<Cage> _cages;
    private readonly List<Placement> _placements = new List<Placement>();
    private readonly int _numColumns;

    public KillerSudokuSolver(string layout, string sums)
    {
        _cages = ParseCages(layout, sums);
        _numColumns = _cages.Count + 3 * 81;
    }

    public int[,] Solve()
    {
        var candidates = BuildAndFilterCandidates();
        GeneratePlacements(candidates);

        var totalNodes = _placements.Sum(p => p.Cage.Cells.Count * 3 + 1);
        var solverOptions = new AlgorithmXOptions(Strategy: SolverStrategy.FindFirst, SortAndDedupRow: true);
        var solver = new AlgorithmXSolver<Placement>(_numColumns, _numColumns, totalNodes, _cages.Count, solverOptions);

        var columnsBuffer = new int[1 + 9 * 3];
        foreach (var placement in _placements)
        {
            int count = GetColumnIndices(placement, columnsBuffer);
            solver.AddRow(columnsBuffer, count, placement);
        }

        var solutionPlacements = solver.Solve();
        return DecodeSolution(solutionPlacements);
    }

    private Dictionary<(int, int), HashSet<int>> BuildAndFilterCandidates()
    {
        var candidates = new Dictionary<(int, int), HashSet<int>>();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                candidates[(r, c)] = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        foreach (var cage in _cages)
        {
            var combinations = FindCombinations(cage.Sum, cage.Cells.Count);
            var possibleValues = new HashSet<int>();
            foreach (var combo in combinations) possibleValues.UnionWith(combo);
            foreach (var cell in cage.Cells) candidates[cell].IntersectWith(possibleValues);
        }

        bool changed;
        do
        {
            changed = false;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (candidates[(r, c)].Count == 1)
                    {
                        if (EliminateFromPeers(candidates, r, c, candidates[(r, c)].First()))
                            changed = true;
                    }
                }
            }
            if (changed) continue;
            for (int i = 0; i < 9; i++)
            {
                if (FindHiddenSinglesInRow(candidates, i)) changed = true;
                if (FindHiddenSinglesInCol(candidates, i)) changed = true;
                if (FindHiddenSinglesInBox(candidates, i)) changed = true;
            }
        } while (changed);
        return candidates;
    }

    private bool EliminateFromPeers(Dictionary<(int, int), HashSet<int>> candidates, int r, int c, int value)
    {
        bool changed = false;
        for (int i = 0; i < 9; i++)
        {
            if (i != c && candidates[(r, i)].Remove(value)) changed = true;
            if (i != r && candidates[(i, c)].Remove(value)) changed = true;
        }
        int startRow = (r / 3) * 3;
        int startCol = (c / 3) * 3;
        for (int r_ = startRow; r_ < startRow + 3; r_++)
        {
            for (int c_ = startCol; c_ < startCol + 3; c_++)
            {
                if ((r_ != r || c_ != c) && candidates[(r_, c_)].Remove(value))
                    changed = true;
            }
        }
        return changed;
    }

    private bool FindHiddenSinglesInUnit(Dictionary<(int, int), HashSet<int>> candidates, IEnumerable<(int, int)> unitCells)
    {
        bool changed = false;
        var valuePlacements = new List<(int, int)>[10];
        for (int i = 1; i <= 9; i++) valuePlacements[i] = new List<(int, int)>();
        foreach (var cell in unitCells)
        {
            foreach (var val in candidates[cell])
            {
                valuePlacements[val].Add(cell);
            }
        }
        for (int val = 1; val <= 9; val++)
        {
            if (valuePlacements[val].Count == 1)
            {
                var cell = valuePlacements[val][0];
                if (candidates[cell].Count > 1)
                {
                    candidates[cell] = new HashSet<int> { val };
                    changed = true;
                }
            }
        }
        return changed;
    }

    private bool FindHiddenSinglesInRow(Dictionary<(int, int), HashSet<int>> candidates, int r)
    {
        return FindHiddenSinglesInUnit(candidates, Enumerable.Range(0, 9).Select(c => (r, c)));
    }
    private bool FindHiddenSinglesInCol(Dictionary<(int, int), HashSet<int>> candidates, int c)
    {
        return FindHiddenSinglesInUnit(candidates, Enumerable.Range(0, 9).Select(r => (r, c)));
    }
    private bool FindHiddenSinglesInBox(Dictionary<(int, int), HashSet<int>> candidates, int b)
    {
        int startRow = (b / 3) * 3;
        int startCol = (b % 3) * 3;
        return FindHiddenSinglesInUnit(candidates, Enumerable.Range(0, 9).Select(i => (startRow + i / 3, startCol + i % 3)));
    }

    private void GeneratePlacements(Dictionary<(int, int), HashSet<int>> candidates)
    {
        foreach (var cage in _cages)
        {
            var combinations = FindCombinations(cage.Sum, cage.Cells.Count);
            foreach (var combo in combinations)
            {
                GenerateValidPermutationsForCage(cage, combo, candidates);
            }
        }
    }

    private void GenerateValidPermutationsForCage(Cage cage, List<int> combo, Dictionary<(int, int), HashSet<int>> candidates)
    {
        var currentPerm = new int[cage.Cells.Count];
        var usedComboIndices = new bool[combo.Count];
        var occupiedInPeers = new bool[3, 9, 9];
        PermuteAndPlace(0, cage, combo, candidates, currentPerm, usedComboIndices, occupiedInPeers);
    }

    private void PermuteAndPlace(int cellIndex, Cage cage, List<int> combo, Dictionary<(int, int), HashSet<int>> candidates, int[] currentPerm, bool[] usedComboIndices, bool[,,] occupiedInPeers)
    {
        if (cellIndex == cage.Cells.Count)
        {
            _placements.Add(new Placement(cage, new List<int>(currentPerm)));
            return;
        }

        var cell = cage.Cells[cellIndex];
        int r = cell.r, c = cell.c, b = (r / 3) * 3 + c / 3;

        for (int i = 0; i < combo.Count; i++)
        {
            if (!usedComboIndices[i])
            {
                int valueToPlace = combo[i];
                if (candidates[cell].Contains(valueToPlace))
                {
                    if (occupiedInPeers[0, r, valueToPlace - 1] ||
                        occupiedInPeers[1, c, valueToPlace - 1] ||
                        occupiedInPeers[2, b, valueToPlace - 1])
                    {
                        continue;
                    }
                    usedComboIndices[i] = true;
                    currentPerm[cellIndex] = valueToPlace;
                    occupiedInPeers[0, r, valueToPlace - 1] = true;
                    occupiedInPeers[1, c, valueToPlace - 1] = true;
                    occupiedInPeers[2, b, valueToPlace - 1] = true;
                    PermuteAndPlace(cellIndex + 1, cage, combo, candidates, currentPerm, usedComboIndices, occupiedInPeers);
                    occupiedInPeers[0, r, valueToPlace - 1] = false;
                    occupiedInPeers[1, c, valueToPlace - 1] = false;
                    occupiedInPeers[2, b, valueToPlace - 1] = false;
                    usedComboIndices[i] = false;
                }
            }
        }
    }

    private int GetColumnIndices(Placement p, int[] buffer)
    {
        var cage = p.Cage;
        var perm = p.Permutation;
        buffer[0] = cage.Id;
        int count = 1;
        var cageOffset = _cages.Count;
        var rowValOffset = cageOffset;
        var colValOffset = cageOffset + 81;
        var regionValOffset = cageOffset + 162;

        for (var i = 0; i < cage.Cells.Count; i++)
        {
            var r = cage.Cells[i].r;
            var c = cage.Cells[i].c;
            var v = perm[i] - 1;
            var b = (r / 3) * 3 + c / 3;
            buffer[count++] = rowValOffset + r * 9 + v;
            buffer[count++] = colValOffset + c * 9 + v;
            buffer[count++] = regionValOffset + b * 9 + v;
        }
        return count;
    }

    private int[,] DecodeSolution(Placement[] solutionPlacements)
    {
        if (solutionPlacements == null) return null;
        var grid = new int[9, 9];
        foreach (var p in solutionPlacements)
        {
            for (var i = 0; i < p.Cage.Cells.Count; i++)
            {
                var r = p.Cage.Cells[i].r;
                var c = p.Cage.Cells[i].c;
                grid[r, c] = p.Permutation[i];
            }
        }
        return grid;
    }

    private static List<Cage> ParseCages(string layout, string sums)
    {
        var cageMap = new Dictionary<char, Cage>();
        for (var i = 0; i < layout.Length; i++)
        {
            var r = i / 9;
            var c = i % 9;
            var id = layout[i];
            if (!cageMap.ContainsKey(id)) cageMap[id] = new Cage(id);
            cageMap[id].Cells.Add((r, c));
        }

        foreach (var sumPair in sums.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = sumPair.Split('=');
            if (parts.Length < 2) continue;
            var id = parts[0][0];
            var sum = int.Parse(parts[1]);
            if (cageMap.ContainsKey(id)) cageMap[id].Sum = sum;
        }

        var cages = cageMap.Values.OrderBy(c => c.CharId).ToList();
        for (var i = 0; i < cages.Count; i++) cages[i].Id = i;
        return cages;
    }

    private List<List<int>> FindCombinations(int targetSum, int k)
    {
        var key = (targetSum, k);
        if (ComboCache.TryGetValue(key, out var cachedResult))
        {
            return cachedResult;
        }

        var results = new List<List<int>>();
        void Find(int start, int currentSum, List<int> current)
        {
            if (current.Count == k)
            {
                if (currentSum == targetSum) results.Add(new List<int>(current));
                return;
            }
            if (current.Count >= k || currentSum >= targetSum) return;

            for (var i = start; i <= 9; i++)
            {
                current.Add(i);
                Find(i + 1, currentSum + i, current);
                current.RemoveAt(current.Count - 1);
            }
        }
        Find(1, 0, new List<int>());
        ComboCache[key] = results;
        return results;
    }
}

public class Cage
{
    public int Id { get; set; }
    public char CharId { get; }
    public int Sum { get; set; }
    public List<(int r, int c)> Cells { get; } = new List<(int, int)>();
    public Cage(char charId) { CharId = charId; }
}

public class Placement
{
    public Cage Cage { get; }
    public List<int> Permutation { get; }
    public Placement(Cage cage, List<int> permutation)
    {
        Cage = cage;
        Permutation = permutation;
    }
}

public enum SolverStrategy { FindFirst, FindAll }

public readonly record struct AlgorithmXOptions(
    SolverStrategy Strategy = SolverStrategy.FindFirst,
    bool SortAndDedupRow = false,
    bool EarlyAbortOnZeroColumn = true,
    bool TieBreakStopAtOne = true,
    bool IncludeAllColumnsInHeader = false
);

public class AlgorithmXSolver<T> where T : class
{
    private struct DlxNode
    {
        public int Left, Right, Up, Down;
        public int ColHeader;
        public T RowPayload;
        public int Size;
    }

    private readonly DlxNode[] _nodes;
    private readonly int _header;
    private int _nodeCount;
    private readonly T[] _solutionBuffer;
    private int _solutionDepth;
    private readonly bool _earlyAbortOnZeroColumn;
    private readonly bool _tieBreakStopAtOne;
    private readonly bool _sortAndDedupRow;
    private readonly SolverStrategy _strategy;

    public AlgorithmXSolver(int numPrimaryColumns, int numTotalColumns, int maxNodes, int maxSolutionDepth, AlgorithmXOptions options = default)
    {
        _strategy = options.Strategy;
        _sortAndDedupRow = options.SortAndDedupRow;
        _earlyAbortOnZeroColumn = options.EarlyAbortOnZeroColumn;
        _tieBreakStopAtOne = options.TieBreakStopAtOne;
        var poolSize = numTotalColumns + 1 + maxNodes;
        _nodes = new DlxNode[poolSize];
        _solutionBuffer = new T[maxSolutionDepth];
        _header = 0;
        for (int i = 0; i <= numTotalColumns; i++)
        {
            _nodes[i].Left = i; _nodes[i].Right = i;
            _nodes[i].Up = i; _nodes[i].Down = i;
            _nodes[i].ColHeader = i; _nodes[i].Size = 0;
        }
        _nodes[_header].Right = _header;
        _nodes[_header].Left = _header;
        int limit = options.IncludeAllColumnsInHeader ? numTotalColumns : numPrimaryColumns;
        for (int i = 1; i <= limit; i++)
        {
            int r = _nodes[_header].Right;
            _nodes[i].Right = r; _nodes[i].Left = _header;
            _nodes[r].Left = i; _nodes[_header].Right = i;
        }
        _nodeCount = numTotalColumns + 1;
    }

    public void AddRow(int[] columns, int count, T rowPayload)
    {
        if (count <= 0) return;
        if (_sortAndDedupRow)
        {
            Array.Sort(columns, 0, count);
            int w = 1;
            for (int i = 1; i < count; i++)
                if (columns[i] != columns[i - 1]) columns[w++] = columns[i];
            count = w;
        }
        int firstNode = -1;
        for (int ci = 0; ci < count; ci++)
        {
            int colHeaderNodeIndex = columns[ci] + 1;
            int newNodeIndex = _nodeCount++;
            ref var col = ref _nodes[colHeaderNodeIndex];
            ref var node = ref _nodes[newNodeIndex];
            col.Size++;
            node.RowPayload = rowPayload; node.ColHeader = colHeaderNodeIndex;
            node.Up = col.Up; node.Down = colHeaderNodeIndex;
            _nodes[col.Up].Down = newNodeIndex; col.Up = newNodeIndex;
            if (firstNode == -1)
            {
                firstNode = newNodeIndex; node.Left = newNodeIndex; node.Right = newNodeIndex;
            }
            else
            {
                node.Left = _nodes[firstNode].Left; node.Right = firstNode;
                _nodes[_nodes[firstNode].Left].Right = newNodeIndex;
                _nodes[firstNode].Left = newNodeIndex;
            }
        }
    }

    public T[] Solve()
    {
        T[] firstSolution = null;
        if (_strategy == SolverStrategy.FindFirst)
        {
            SearchFindFirst(sol => { firstSolution = sol; });
        }
        else
        {
            firstSolution = EnumerateSolutions().FirstOrDefault();
        }
        return firstSolution;
    }

    public IEnumerable<T[]> EnumerateSolutions()
    {
        var solutions = new List<T[]>();
        if (_strategy == SolverStrategy.FindFirst)
        {
            SearchFindFirst(sol => solutions.Add(sol));
        }
        else
        {
            SearchFindAll(sol => solutions.Add(sol));
        }
        return solutions;
    }

    private bool SearchFindFirst(Action<T[]> onSolutionFound)
    {
        if (_nodes[_header].Right == _header)
        {
            var result = new T[_solutionDepth];
            Array.Copy(_solutionBuffer, result, _solutionDepth);
            onSolutionFound(result);
            return true;
        }
        int c = ChooseColumn();
        if (_earlyAbortOnZeroColumn && c == 0) return false;
        Cover(c);
        for (int r_node = _nodes[c].Down; r_node != c; r_node = _nodes[r_node].Down)
        {
            _solutionBuffer[_solutionDepth++] = _nodes[r_node].RowPayload;
            for (int j_node = _nodes[r_node].Right; j_node != r_node; j_node = _nodes[j_node].Right)
                Cover(_nodes[j_node].ColHeader);
            if (SearchFindFirst(onSolutionFound)) return true;
            _solutionDepth--;
            for (int j_node = _nodes[r_node].Left; j_node != r_node; j_node = _nodes[j_node].Left)
                Uncover(_nodes[j_node].ColHeader);
        }
        Uncover(c);
        return false;
    }

    private void SearchFindAll(Action<T[]> onSolutionFound)
    {
        if (_nodes[_header].Right == _header)
        {
            var result = new T[_solutionDepth];
            Array.Copy(_solutionBuffer, result, _solutionDepth);
            onSolutionFound(result);
            return;
        }
        int c = ChooseColumn();
        if (_earlyAbortOnZeroColumn && c == 0) return;
        Cover(c);
        for (int r_node = _nodes[c].Down; r_node != c; r_node = _nodes[r_node].Down)
        {
            _solutionBuffer[_solutionDepth++] = _nodes[r_node].RowPayload;
            for (int j_node = _nodes[r_node].Right; j_node != r_node; j_node = _nodes[j_node].Right)
                Cover(_nodes[j_node].ColHeader);
            SearchFindAll(onSolutionFound);
            _solutionDepth--;
            for (int j_node = _nodes[r_node].Left; j_node != r_node; j_node = _nodes[j_node].Left)
                Uncover(_nodes[j_node].ColHeader);
        }
        Uncover(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ChooseColumn()
    {
        int minSize = int.MaxValue;
        int bestCol = 0;
        for (int c_header = _nodes[_header].Right; c_header != _header; c_header = _nodes[c_header].Right)
        {
            int s = _nodes[c_header].Size;
            if (_earlyAbortOnZeroColumn && s == 0) return 0;
            if (s < minSize)
            {
                minSize = s;
                bestCol = c_header;
                if (_tieBreakStopAtOne && s <= 1) break;
            }
        }
        return bestCol;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Cover(int c)
    {
        ref var rc = ref _nodes[c];
        _nodes[rc.Left].Right = rc.Right;
        _nodes[rc.Right].Left = rc.Left;
        for (int i = rc.Down; i != c; i = _nodes[i].Down)
        {
            for (int j = _nodes[i].Right; j != i; j = _nodes[j].Right)
            {
                ref var nodeJ = ref _nodes[j];
                _nodes[nodeJ.Up].Down = nodeJ.Down;
                _nodes[nodeJ.Down].Up = nodeJ.Up;
                _nodes[nodeJ.ColHeader].Size--;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Uncover(int c)
    {
        ref var rc = ref _nodes[c];
        for (int i = rc.Up; i != c; i = _nodes[i].Up)
        {
            for (int j = _nodes[i].Left; j != i; j = _nodes[j].Left)
            {
                ref var nodeJ = ref _nodes[j];
                _nodes[nodeJ.ColHeader].Size++;
                _nodes[nodeJ.Up].Down = j;
                _nodes[nodeJ.Down].Up = j;
            }
        }
        _nodes[rc.Left].Right = c;
        _nodes[rc.Right].Left = c;
    }
}