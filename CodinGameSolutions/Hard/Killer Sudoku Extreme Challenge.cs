using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

class Solution
{
    private static readonly Dictionary<(int, int), List<List<int>>> ComboCache = new();
    private static readonly Dictionary<string, List<List<int>>> PermCache = new();

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
        long totalTime=0;
        for (int i = 0; i < numPuzzles; i++)
        {
            var sw = Stopwatch.StartNew();
            var killerSudokuSolver = new KillerSudokuSolver(puzzleLayouts[i], puzzleSums[i], ComboCache, PermCache);
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
    private readonly List<Cage> _cages;
    private readonly List<Placement> _placements = new List<Placement>();
    private readonly int _numColumns;
    private readonly Dictionary<(int, int), List<List<int>>> _comboCache;
    private readonly Dictionary<string, List<List<int>>> _permCache;

    public KillerSudokuSolver(string layout, string sums, 
                              Dictionary<(int, int), List<List<int>>> comboCache,
                              Dictionary<string, List<List<int>>> permCache)
    {
        _cages = ParseCages(layout, sums);
        _numColumns = _cages.Count + 3 * 81;
        _comboCache = comboCache;
        _permCache = permCache;
    }

    public int[,] Solve()
    {
        PrecomputePlacements();
        var totalNodes = _placements.Sum(p => 1 + p.Cage.Cells.Count * 3);

        var solver = new AlgorithmXSolver<Placement>(_numColumns, _numColumns, totalNodes, _cages.Count);
        
        foreach (var placement in _placements)
        {
            solver.AddRow(GetColumnIndices(placement), placement);
        }
        
        var solutionPlacements = solver.EnumerateSolutions().FirstOrDefault();
        return solutionPlacements != null ? DecodeSolution(solutionPlacements) : null;
    }
    
    private List<int> GetColumnIndices(Placement p)
    {
        var cage = p.Cage;
        var perm = p.Permutation;
        var indices = new List<int> { cage.Id };
        var cageOffset = _cages.Count;
        var rowValOffset = cageOffset;
        var colValOffset = cageOffset + 81;
        var regionValOffset = cageOffset + 162;

        for (var i = 0; i < cage.Cells.Count; i++)
        {
            var r = cage.Cells[i].r;
            var c = cage.Cells[i].c;
            var v = perm[i];
            var b = (r / 3) * 3 + c / 3;
            indices.Add(rowValOffset + r * 9 + (v - 1));
            indices.Add(colValOffset + c * 9 + (v - 1));
            indices.Add(regionValOffset + b * 9 + (v - 1));
        }
        return indices;
    }

    private int[,] DecodeSolution(Placement[] solutionPlacements)
    {
        var grid = new int[9, 9];
        if (solutionPlacements == null) return grid;
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

    private void PrecomputePlacements()
    {
        foreach (var cage in _cages)
        {
            var combinations = FindCombinations(cage.Sum, cage.Cells.Count);
            foreach (var combo in combinations)
            {
                var permutations = GetPermutations(combo);
                foreach (var perm in permutations)
                {
                    _placements.Add(new Placement(cage, perm));
                }
            }
        }
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
        for(var i = 0; i < cages.Count; i++) cages[i].Id = i;
        return cages;
    }

    private List<List<int>> FindCombinations(int targetSum, int k)
    {
        var key = (targetSum, k);
        if (_comboCache.TryGetValue(key, out var cachedResult))
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
        _comboCache[key] = results;
        return results;
    }

    private List<List<int>> GetPermutations(List<int> list)
    {
        var key = string.Join(",", list);
        if (_permCache.TryGetValue(key, out var cachedResult))
        {
            return cachedResult;
        }
        
        var results = new List<List<int>>();
        void Permute(int[] arr, int k)
        {
            if (k == arr.Length)
            {
                results.Add(new List<int>(arr));
                return;
            }
            for (var i = k; i < arr.Length; i++)
            {
                (arr[k], arr[i]) = (arr[i], arr[k]);
                Permute(arr, k + 1);
                (arr[k], arr[i]) = (arr[i], arr[k]);
            }
        }
        Permute(list.ToArray(), 0);
        _permCache[key] = results;
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
    private int _nodeCount;
    private readonly T[] _solution;
    private int _solutionDepth;

    public AlgorithmXSolver(int numPrimaryColumns, int numTotalColumns, int maxNodes, int maxSolutionDepth)
    {
        var poolSize = numTotalColumns + 1 + maxNodes;
        _nodes = new DlxNode[poolSize];
        _solution = new T[maxSolutionDepth];
        
        for (int i = 0; i <= numTotalColumns; i++)
        {
            _nodes[i] = new DlxNode { Left = i, Right = i, Up = i, Down = i, ColHeader = i, Size = 0 };
        }

        _nodes[0].Right = 0;
        _nodes[0].Left = 0;
        for (int i = 1; i <= numPrimaryColumns; i++)
        {
            _nodes[i].Right = _nodes[0].Right;
            _nodes[i].Left = 0;
            _nodes[_nodes[0].Right].Left = i;
            _nodes[0].Right = i;
        }
        _nodeCount = numTotalColumns + 1;
    }

    public void AddRow(List<int> columns, T rowPayload)
    {
        if (columns.Count == 0) return;
        int firstNode = -1;
        foreach (int c_idx in columns)
        {
            int headerIdx = c_idx + 1;
            _nodes[headerIdx].Size++;
            _nodes[_nodeCount].RowPayload = rowPayload;
            _nodes[_nodeCount].ColHeader = headerIdx;
            _nodes[_nodeCount].Up = _nodes[headerIdx].Up;
            _nodes[_nodeCount].Down = headerIdx;
            _nodes[_nodes[headerIdx].Up].Down = _nodeCount;
            _nodes[headerIdx].Up = _nodeCount;
            if (firstNode == -1)
            {
                firstNode = _nodeCount;
                _nodes[_nodeCount].Left = _nodeCount;
                _nodes[_nodeCount].Right = _nodeCount;
            }
            else
            {
                _nodes[_nodeCount].Left = _nodes[firstNode].Left;
                _nodes[_nodeCount].Right = firstNode;
                _nodes[_nodes[firstNode].Left].Right = _nodeCount;
                _nodes[firstNode].Left = _nodeCount;
            }
            _nodeCount++;
        }
    }
    
    public IEnumerable<T[]> EnumerateSolutions()
    {
        if (_nodes[0].Right == 0)
        {
            var result = new T[_solutionDepth];
            Array.Copy(_solution, result, _solutionDepth);
            yield return result;
            yield break;
        }

        int c = ChooseColumn();
        Cover(c);

        for (int r_node = _nodes[c].Down; r_node != c; r_node = _nodes[r_node].Down)
        {
            _solution[_solutionDepth] = _nodes[r_node].RowPayload;
            _solutionDepth++;
            for (int j_node = _nodes[r_node].Right; j_node != r_node; j_node = _nodes[j_node].Right)
            {
                Cover(_nodes[j_node].ColHeader);
            }

            foreach (var sol in EnumerateSolutions())
            {
                yield return sol;
            }
            
            _solutionDepth--;
            for (int j_node = _nodes[r_node].Left; j_node != r_node; j_node = _nodes[j_node].Left)
            {
                Uncover(_nodes[j_node].ColHeader);
            }
        }
        Uncover(c);
    }

    private int ChooseColumn()
    {
        int minSize = int.MaxValue;
        int bestCol = 0;
        for (int c_header = _nodes[0].Right; c_header != 0; c_header = _nodes[c_header].Right)
        {
            if (_nodes[c_header].Size < minSize)
            {
                minSize = _nodes[c_header].Size;
                bestCol = c_header;
            }
        }
        return bestCol;
    }

    private void Cover(int c)
    {
        _nodes[_nodes[c].Left].Right = _nodes[c].Right;
        _nodes[_nodes[c].Right].Left = _nodes[c].Left;
        
        for (int i = _nodes[c].Down; i != c; i = _nodes[i].Down)
        {
            for (int j = _nodes[i].Right; j != i; j = _nodes[j].Right)
            {
                _nodes[_nodes[j].Up].Down = _nodes[j].Down;
                _nodes[_nodes[j].Down].Up = _nodes[j].Up;
                _nodes[_nodes[j].ColHeader].Size--;
            }
        }
    }

    private void Uncover(int c)
    {
        for (int i = _nodes[c].Up; i != c; i = _nodes[i].Up)
        {
            for (int j = _nodes[i].Left; j != i; j = _nodes[j].Left)
            {
                _nodes[_nodes[j].ColHeader].Size++;
                _nodes[_nodes[j].Up].Down = j;
                _nodes[_nodes[j].Down].Up = j;
            }
        }
        _nodes[_nodes[c].Left].Right = c;
        _nodes[_nodes[c].Right].Left = c;
    }
}