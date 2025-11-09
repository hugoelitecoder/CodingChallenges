using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Player
{
    public static void Main(string[] args)
    {
        var totalStopwatch = Stopwatch.StartNew();
        string[] inputs = Console.ReadLine().Split(' ');
        int height = int.Parse(inputs[0]);
        int width = int.Parse(inputs[1]);

        var board = new int[height, width];
        var rules = new Dictionary<int, Rule>();

        for (int i = 0; i < height; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            for (int j = 0; j < width; j++)
            {
                int cellValue = int.Parse(inputs[j]);
                board[i, j] = cellValue;
                if (cellValue > 0)
                {
                    if (!rules.ContainsKey(cellValue)) rules[cellValue] = new Rule();
                    rules[cellValue].Cells.Add((j, i));
                }
            }
        }

        int rulesCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < rulesCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int ruleId = int.Parse(inputs[0]);
            rules[ruleId].Type = inputs[1];
            rules[ruleId].Value = int.Parse(inputs[2]);
        }

        int dominoesCount = int.Parse(Console.ReadLine());
        var dominoes = new List<(int A, int B)>();
        for (int i = 0; i < dominoesCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            dominoes.Add((A: int.Parse(inputs[0]), B: int.Parse(inputs[1])));
        }
        Console.Error.WriteLine("[DEBUG] Input parsing complete.");

        var solver = new PipsSolver(height, width, board, rules, dominoes);
        var finalSolution = solver.Solve();
        
        if (finalSolution != null)
        {
            foreach (var p in finalSolution)
            {
                Console.WriteLine($"{p.PipsA} {p.PipsB} {p.X} {p.Y} {p.Orientation}");
            }
        }

        totalStopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Total solving time: {totalStopwatch.ElapsedMilliseconds} ms");
    }
}

public class PipsSolver
{
    private readonly int _height;
    private readonly int _width;
    private readonly int[,] _board;
    private readonly Dictionary<int, Rule> _rules;
    private readonly List<(int A, int B)> _dominoes;
    private readonly int _maxPipValue;

    public PipsSolver(int height, int width, int[,] board, Dictionary<int, Rule> rules, List<(int A, int B)> dominoes)
    {
        _height = height;
        _width = width;
        _board = board;
        _rules = rules;
        _dominoes = dominoes;
        
        _maxPipValue = 0;
        foreach (var d in _dominoes)
        {
            _maxPipValue = Math.Max(_maxPipValue, Math.Max(d.A, d.B));
        }
    }

    public Placement[] Solve()
    {
        var availableCells = new List<(int x, int y)>();
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (_board[y, x] != -1) availableCells.Add((x, y));
            }
        }

        var cellToIndexMap = new Dictionary<(int x, int y), int>(availableCells.Count);
        for(int i = 0; i < availableCells.Count; i++) cellToIndexMap[availableCells[i]] = i;

        var validPlacements = GenerateValidPlacements();

        int numCellColumns = availableCells.Count;
        int numDominoColumns = _dominoes.Count;
        int numPrimaryColumns = numCellColumns + numDominoColumns;
        int maxNodes = validPlacements.Count * 3 + numPrimaryColumns + 1;
        var options = new AlgorithmXOptions(SolverStrategy.FindFirst, EarlyAbortOnZeroColumn: true, TieBreakStopAtOne: true);
        var dlxSolver = new AlgorithmXSolver<Placement>(numPrimaryColumns, numPrimaryColumns, maxNodes, _dominoes.Count, options);
        var colBuffer = new int[3];

        foreach (var p in validPlacements)
        {
            (int x, int y) cell1 = (p.X, p.Y);
            (int x, int y) cell2 = p.Orientation == 0 ? (p.X + 1, p.Y) : (p.X, p.Y + 1);
            colBuffer[0] = cellToIndexMap[cell1];
            colBuffer[1] = cellToIndexMap[cell2];
            colBuffer[2] = numCellColumns + p.OriginalDominoIndex;
            dlxSolver.AddRow(colBuffer, 3, p);
        }

        Placement[] finalSolution = null;
        var placedPips = new int[_height, _width];

        dlxSolver.Search(solution =>
        {
            foreach (var p in solution)
            {
                placedPips[p.Y, p.X] = p.PipsA;
                if (p.Orientation == 0) placedPips[p.Y, p.X + 1] = p.PipsB;
                else placedPips[p.Y + 1, p.X] = p.PipsB;
            }

            foreach (var rule in _rules.Values)
            {
                if (!CheckRule(rule, placedPips)) return false;
            }
            finalSolution = solution;
            return true;
        });

        return finalSolution;
    }

    private List<Placement> GenerateValidPlacements()
    {
        var validPlacements = new List<Placement>();
        for (int dominoIndex = 0; dominoIndex < _dominoes.Count; dominoIndex++)
        {
            var (dominoA, dominoB) = _dominoes[dominoIndex];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_board[y, x] == -1) continue;

                    if (x + 1 < _width && _board[y, x + 1] != -1)
                    {
                        var p1 = new Placement(dominoIndex, dominoA, dominoB, x, y, 0);
                        if (IsPlacementInitiallyValid(p1)) validPlacements.Add(p1);

                        if (dominoA != dominoB)
                        {
                            var p2 = new Placement(dominoIndex, dominoB, dominoA, x, y, 0);
                            if (IsPlacementInitiallyValid(p2)) validPlacements.Add(p2);
                        }
                    }

                    if (y + 1 < _height && _board[y + 1, x] != -1)
                    {
                        var p3 = new Placement(dominoIndex, dominoA, dominoB, x, y, 1);
                        if (IsPlacementInitiallyValid(p3)) validPlacements.Add(p3);

                        if (dominoA != dominoB)
                        {
                            var p4 = new Placement(dominoIndex, dominoB, dominoA, x, y, 1);
                            if (IsPlacementInitiallyValid(p4)) validPlacements.Add(p4);
                        }
                    }
                }
            }
        }
        return validPlacements;
    }

    private bool IsPlacementInitiallyValid(Placement p)
    {
        (int x1, int y1) = (p.X, p.Y);
        (int x2, int y2) = p.Orientation == 0 ? (p.X + 1, p.Y) : (p.X, p.Y + 1);
        int ruleId1 = _board[y1, x1];
        int ruleId2 = _board[y2, x2];

        if (ruleId1 > 0 && ruleId1 == ruleId2)
        {
            var rule = _rules[ruleId1];
            if (rule.Type == "==" && p.PipsA != p.PipsB) return false;
            if (rule.Type == "!=" && p.PipsA == p.PipsB) return false;
            if (!IsRegionSumPossible(rule, p.PipsA + p.PipsB, 2)) return false;
        }
        else
        {
            if (ruleId1 > 0 && !IsRegionSumPossible(_rules[ruleId1], p.PipsA, 1)) return false;
            if (ruleId2 > 0 && !IsRegionSumPossible(_rules[ruleId2], p.PipsB, 1)) return false;
        }
        return true;
    }

    private bool IsRegionSumPossible(Rule rule, int currentSum, int filledCount)
    {
        int remainingCount = rule.Cells.Count - filledCount;
        switch (rule.Type)
        {
            case "=":
                return currentSum <= rule.Value && currentSum + (remainingCount * _maxPipValue) >= rule.Value;
            case "<":
                return currentSum < rule.Value;
            case ">":
                return currentSum + (remainingCount * _maxPipValue) > rule.Value;
        }
        return true;
    }

    private bool CheckRule(Rule rule, int[,] placedPips)
    {
        var cells = rule.Cells;
        if (cells.Count == 0) return true;

        switch (rule.Type)
        {
            case "==":
                int firstPip = placedPips[cells[0].y, cells[0].x];
                for (int i = 1; i < cells.Count; i++)
                {
                    if (placedPips[cells[i].y, cells[i].x] != firstPip) return false;
                }
                return true;
            case "!=":
                for (int i = 0; i < cells.Count; i++)
                {
                    int pip_i = placedPips[cells[i].y, cells[i].x];
                    for (int j = i + 1; j < cells.Count; j++)
                    {
                        if (pip_i == placedPips[cells[j].y, cells[j].x]) return false;
                    }
                }
                return true;
            case ">":
            case "<":
            case "=":
                int sum = 0;
                foreach (var cell in cells)
                {
                    sum += placedPips[cell.y, cell.x];
                }
                if (rule.Type == ">") return sum > rule.Value;
                if (rule.Type == "<") return sum < rule.Value;
                if (rule.Type == "=") return sum == rule.Value;
                break;
        }
        return false;
    }
}

public record Placement(int OriginalDominoIndex, int PipsA, int PipsB, int X, int Y, int Orientation);
public class Rule
{
    public string Type { get; set; }
    public int Value { get; set; }
    public List<(int x, int y)> Cells { get; } = new();
}

public enum SolverStrategy { FindFirst, FindAll }
public readonly record struct AlgorithmXOptions(SolverStrategy Strategy = SolverStrategy.FindFirst, bool SortAndDedupRow = false, bool EarlyAbortOnZeroColumn = true, bool TieBreakStopAtOne = true, bool IncludeAllColumnsInHeader = false);
public class AlgorithmXSolver<T> where T : class
{
    private struct DlxNode { public int Left, Right, Up, Down, ColHeader, Size; public T RowPayload; }
    private readonly DlxNode[] _nodes;
    private readonly int _header;
    private int _nodeCount;
    private readonly T[] _solutionBuffer;
    private int _solutionDepth;
    private readonly AlgorithmXOptions _options;
    public AlgorithmXSolver(int numPrimaryColumns, int numTotalColumns, int maxNodes, int maxSolutionDepth, AlgorithmXOptions options)
    {
        _options = options;
        _nodes = new DlxNode[numTotalColumns + 1 + maxNodes];
        _solutionBuffer = new T[maxSolutionDepth];
        _header = 0;
        for (int i = 0; i <= numTotalColumns; i++) { _nodes[i] = new DlxNode { Left = i, Right = i, Up = i, Down = i, ColHeader = i, Size = 0 }; }
        _nodes[_header].Right = _header; _nodes[_header].Left = _header;
        int limit = options.IncludeAllColumnsInHeader ? numTotalColumns : numPrimaryColumns;
        for (int i = 1; i <= limit; i++) { int r = _nodes[_header].Right; _nodes[i].Right = r; _nodes[i].Left = _header; _nodes[r].Left = i; _nodes[_header].Right = i; }
        _nodeCount = numTotalColumns + 1;
    }
    public void AddRow(int[] columns, int count, T rowPayload)
    {
        if (count <= 0) return;
        if (_options.SortAndDedupRow) { Array.Sort(columns, 0, count); int writeIdx = 1; for (int i = 1; i < count; i++) if (columns[i] != columns[i - 1]) columns[writeIdx++] = columns[i]; count = writeIdx; }
        int firstNode = -1;
        for (int i = 0; i < count; i++)
        {
            int colHeaderIdx = columns[i] + 1;
            int newNodeIdx = _nodeCount++;
            ref var col = ref _nodes[colHeaderIdx];
            ref var node = ref _nodes[newNodeIdx];
            col.Size++;
            node = new DlxNode { RowPayload = rowPayload, ColHeader = colHeaderIdx, Up = col.Up, Down = colHeaderIdx };
            _nodes[col.Up].Down = newNodeIdx; col.Up = newNodeIdx;
            if (firstNode == -1) { firstNode = newNodeIdx; node.Left = newNodeIdx; node.Right = newNodeIdx; }
            else { node.Left = _nodes[firstNode].Left; node.Right = firstNode; _nodes[_nodes[firstNode].Left].Right = newNodeIdx; _nodes[firstNode].Left = newNodeIdx; }
        }
    }

    public bool Search(Func<T[], bool> onSolutionFound)
    {
        if (_nodes[_header].Right == _header) { var result = new T[_solutionDepth]; Array.Copy(_solutionBuffer, result, _solutionDepth); return onSolutionFound(result); }
        int c = ChooseColumn();
        if (_options.EarlyAbortOnZeroColumn && c == 0) return false;
        Cover(c);
        for (int rNode = _nodes[c].Down; rNode != c; rNode = _nodes[rNode].Down)
        {
            _solutionBuffer[_solutionDepth++] = _nodes[rNode].RowPayload;
            for (int jNode = _nodes[rNode].Right; jNode != rNode; jNode = _nodes[jNode].Right) Cover(_nodes[jNode].ColHeader);
            if (Search(onSolutionFound) && _options.Strategy == SolverStrategy.FindFirst) return true;
            _solutionDepth--;
            for (int jNode = _nodes[rNode].Left; jNode != rNode; jNode = _nodes[jNode].Left) Uncover(_nodes[jNode].ColHeader);
        }
        Uncover(c);
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ChooseColumn()
    {
        int minSize = int.MaxValue; int bestCol = 0;
        for (int cHeader = _nodes[_header].Right; cHeader != _header; cHeader = _nodes[cHeader].Right)
        {
            int s = _nodes[cHeader].Size;
            if (_options.EarlyAbortOnZeroColumn && s == 0) return 0;
            if (s < minSize) { minSize = s; bestCol = cHeader; if (_options.TieBreakStopAtOne && s <= 1) break; }
        }
        return bestCol;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Cover(int c)
    {
        ref var colNode = ref _nodes[c];
        _nodes[colNode.Left].Right = colNode.Right; _nodes[colNode.Right].Left = colNode.Left;
        for (int i = colNode.Down; i != c; i = _nodes[i].Down) { for (int j = _nodes[i].Right; j != i; j = _nodes[j].Right) { ref var node = ref _nodes[j]; _nodes[node.Up].Down = node.Down; _nodes[node.Down].Up = node.Up; _nodes[node.ColHeader].Size--; } }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Uncover(int c)
    {
        ref var colNode = ref _nodes[c];
        for (int i = colNode.Up; i != c; i = _nodes[i].Up) { for (int j = _nodes[i].Left; j != i; j = _nodes[j].Left) { ref var node = ref _nodes[j]; _nodes[node.ColHeader].Size++; _nodes[node.Up].Down = j; _nodes[node.Down].Up = j; } }
        _nodes[colNode.Left].Right = c; _nodes[colNode.Right].Left = c;
    }
}

