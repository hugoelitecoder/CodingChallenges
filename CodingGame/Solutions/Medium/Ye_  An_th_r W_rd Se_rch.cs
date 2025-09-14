using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

class Program
{
    static void Main()
    {
        var dims = Console.ReadLine()!.Split();
        int height = int.Parse(dims[0]), width = int.Parse(dims[1]);
        var grid = new char[height][];
        for (int i = 0; i < height; i++) grid[i] = Console.ReadLine()!.ToCharArray();
        var words = Console.ReadLine()!.Split();

        var answer = new char[height][];
        for (int i = 0; i < height; i++) answer[i] = new string(' ', width).ToCharArray();

        var solver = new CrossWordPuzzleSolver(words, grid);
        var sw = Stopwatch.StartNew();

        var solution = solver.Solve();
        if (solution != null)
        {
            foreach (var placement in solution)
            {
                var word = placement.Direction == "backward" ? new string(placement.Word.Reverse().ToArray()) : placement.Word;
                var (dr, dc) = CrossWordPuzzleSolver.DELTAS[placement.Orientation];
                int r = placement.Row, c = placement.Col;
                foreach (var ch in word)
                {
                    answer[r][c] = ch;
                    r += dr; c += dc;
                }
            }
        }
        sw.Stop();

        foreach (var row in answer)
            Console.WriteLine(new string(row));

        Console.Error.WriteLine($"[DEBUG] Solved in {sw.ElapsedMilliseconds} ms");
    }
}

public record Placement(string Word, int Row, int Col, string Orientation, string Direction);

class CrossWordPuzzleSolver : ISolverObserver<Placement>
{
    private readonly AlgorithmXSolver<Placement> _solver;
    private bool _solutionIsValid = true;
    private readonly Dictionary<(int, int), List<char>> _locationColors;

    public static readonly Dictionary<string, (int dr, int dc)> DELTAS = new()
    {
        ["horizontal"] = (0, 1),
        ["vertical"] = (1, 0),
        ["up_diag"] = (-1, 1),
        ["down_diag"] = (1, 1)
    };

    public CrossWordPuzzleSolver(string[] words, char[][] grid)
    {
        int height = grid.Length;
        int width = grid[0].Length;
        _locationColors = new Dictionary<(int, int), List<char>>();
        for (int r = 0; r < height; r++)
            for (int c = 0; c < width; c++)
                _locationColors[(r, c)] = new List<char>();

        var wordToCol = words.Select((w, i) => (w, i)).ToDictionary(p => p.w, p => p.i);
        int maxNodes = 0;
        var rowsToAdd = new List<(Placement payload, int[] cols)>();

        foreach (var word in words)
        {
            foreach (var orientation in DELTAS.Keys)
            {
                var (dr, dc) = DELTAS[orientation];
                for (int row = 0; row < height; row++)
                    for (int col = 0; col < width; col++)
                    {
                        if (row + dr * (word.Length - 1) < 0 || row + dr * (word.Length - 1) >= height ||
                            col + dc * (word.Length - 1) < 0 || col + dc * (word.Length - 1) >= width)
                            continue;

                        foreach (var fb in new[] { "forward", "backward" })
                        {
                            var wstr = fb == "forward" ? word : new string(word.Reverse().ToArray());
                            bool ok = true;
                            int rr = row, cc = col;
                            foreach (var ch in wstr)
                            {
                                if (grid[rr][cc] != '.' && grid[rr][cc] != ch) { ok = false; break; }
                                rr += dr; cc += dc;
                            }
                            if (ok)
                            {
                                var payload = new Placement(word, row, col, orientation, fb);
                                rowsToAdd.Add((payload, new[] { wordToCol[word] }));
                                maxNodes++;
                            }
                        }
                    }
            }
        }

        _solver = new AlgorithmXSolver<Placement>(words.Length, words.Length, maxNodes, words.Length, observer: this);

        foreach (var row in rowsToAdd)
        {
            _solver.AddRow(row.cols, row.cols.Length, row.payload);
        }
    }

    public Placement[] Solve() => _solver.Solve();
    public bool IsSolutionValid() => _solutionIsValid;

    public void OnRowSelected(Placement placement)
    {
        _solutionIsValid = true;
        var word = placement.Direction == "backward" ? new string(placement.Word.Reverse().ToArray()) : placement.Word;
        var (dr, dc) = DELTAS[placement.Orientation];
        int r = placement.Row, c = placement.Col;
        foreach (var ch in word)
        {
            var lst = _locationColors[(r, c)];
            if (lst.Count > 0 && lst.Last() != ch)
            {
                _solutionIsValid = false;
            }
            lst.Add(ch);
            r += dr; c += dc;
        }
    }

    public void OnRowDeselected(Placement placement)
    {
        var word = placement.Direction == "backward" ? new string(placement.Word.Reverse().ToArray()) : placement.Word;
        var (dr, dc) = DELTAS[placement.Orientation];
        int r = placement.Row, c = placement.Col;
        for (int i = 0; i < word.Length; i++)
        {
            _locationColors[(r, c)].RemoveAt(_locationColors[(r, c)].Count - 1);
            r += dr; c += dc;
        }
    }
}

#region Algorithm X

public enum SolverStrategy { FindFirst, FindAll }

public readonly record struct AlgorithmXOptions(
    SolverStrategy Strategy = SolverStrategy.FindFirst,
    bool SortAndDedupRow = false,
    bool EarlyAbortOnZeroColumn = true,
    bool TieBreakStopAtOne = true,
    bool IncludeAllColumnsInHeader = false
);

public interface ISolverObserver<T> where T : class
{
    void OnRowSelected(T rowPayload);
    void OnRowDeselected(T rowPayload);
    bool IsSolutionValid();
}

public class AlgorithmXSolver<T> where T : class
{
    private struct DlxNode { public int Left, Right, Up, Down, ColHeader; public T RowPayload; public int Size; }

    private readonly DlxNode[] _nodes;
    private readonly int _header;
    private int _nodeCount;
    private readonly T[] _solutionBuffer;
    private int _solutionDepth;
    private readonly ISolverObserver<T> _observer;
    private readonly bool _earlyAbortOnZeroColumn, _tieBreakStopAtOne, _sortAndDedupRow;

    public AlgorithmXSolver(int numPrimaryColumns, int numTotalColumns, int maxNodes, int maxSolutionDepth, AlgorithmXOptions options = default, ISolverObserver<T> observer = null)
    {
        _observer = observer;
        _sortAndDedupRow = options.SortAndDedupRow;
        _earlyAbortOnZeroColumn = options.EarlyAbortOnZeroColumn;
        _tieBreakStopAtOne = options.TieBreakStopAtOne;
        var poolSize = numTotalColumns + 1 + maxNodes;
        _nodes = new DlxNode[poolSize];
        _solutionBuffer = new T[maxSolutionDepth];
        _header = 0;
        for (int i = 0; i <= numTotalColumns; i++)
        {
            _nodes[i].Left = i; _nodes[i].Right = i; _nodes[i].Up = i; _nodes[i].Down = i;
            _nodes[i].ColHeader = i; _nodes[i].Size = 0;
        }
        _nodes[_header].Right = _header; _nodes[_header].Left = _header;
        int limit = options.IncludeAllColumnsInHeader ? numTotalColumns : numPrimaryColumns;
        for (int i = 1; i <= limit; i++)
        {
            int r = _nodes[_header].Right;
            _nodes[i].Right = r; _nodes[i].Left = _header; _nodes[r].Left = i; _nodes[_header].Right = i;
        }
        _nodeCount = numTotalColumns + 1;
    }

    public void AddRow(int[] columns, int count, T rowPayload)
    {
        if (count <= 0) return;
        if (_sortAndDedupRow) { Array.Sort(columns, 0, count); int w = 1; for (int i = 1; i < count; i++) if (columns[i] != columns[i - 1]) columns[w++] = columns[i]; count = w; }
        int firstNode = -1;
        for (int ci = 0; ci < count; ci++)
        {
            int colHeaderNodeIndex = columns[ci] + 1;
            int newNodeIndex = _nodeCount++;
            ref var col = ref _nodes[colHeaderNodeIndex]; ref var node = ref _nodes[newNodeIndex];
            col.Size++;
            node.RowPayload = rowPayload; node.ColHeader = colHeaderNodeIndex;
            node.Up = col.Up; node.Down = colHeaderNodeIndex; _nodes[col.Up].Down = newNodeIndex; col.Up = newNodeIndex;
            if (firstNode == -1) { firstNode = newNodeIndex; node.Left = newNodeIndex; node.Right = newNodeIndex; }
            else { node.Left = _nodes[firstNode].Left; node.Right = firstNode; _nodes[_nodes[firstNode].Left].Right = newNodeIndex; _nodes[firstNode].Left = newNodeIndex; }
        }
    }

    public T[] Solve() { T[] firstSolution = null; SearchFindFirst(sol => { firstSolution = sol; }); return firstSolution; }

    private bool SearchFindFirst(Action<T[]> onSolutionFound)
    {
        if (_nodes[_header].Right == _header)
        {
            var result = new T[_solutionDepth]; Array.Copy(_solutionBuffer, result, _solutionDepth);
            onSolutionFound(result); return true;
        }
        int c = ChooseColumn(); if (_earlyAbortOnZeroColumn && c == 0) return false;
        Cover(c);
        for (int r_node = _nodes[c].Down; r_node != c; r_node = _nodes[r_node].Down)
        {
            var payload = _nodes[r_node].RowPayload;
            _solutionBuffer[_solutionDepth++] = payload;
            _observer?.OnRowSelected(payload);

            for (int j_node = _nodes[r_node].Right; j_node != r_node; j_node = _nodes[j_node].Right) Cover(_nodes[j_node].ColHeader);

            if ((_observer == null || _observer.IsSolutionValid()) && SearchFindFirst(onSolutionFound)) return true;

            _solutionDepth--;
            _observer?.OnRowDeselected(payload);

            for (int j_node = _nodes[r_node].Left; j_node != r_node; j_node = _nodes[j_node].Left) Uncover(_nodes[j_node].ColHeader);
        }
        Uncover(c); return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ChooseColumn()
    {
        int minSize = int.MaxValue, bestCol = 0;
        for (int c_header = _nodes[_header].Right; c_header != _header; c_header = _nodes[c_header].Right)
        {
            int s = _nodes[c_header].Size; if (_earlyAbortOnZeroColumn && s == 0) return 0;
            if (s < minSize) { minSize = s; bestCol = c_header; if (_tieBreakStopAtOne && s <= 1) break; }
        }
        return bestCol;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Cover(int c)
    {
        ref var rc = ref _nodes[c]; _nodes[rc.Left].Right = rc.Right; _nodes[rc.Right].Left = rc.Left;
        for (int i = rc.Down; i != c; i = _nodes[i].Down) for (int j = _nodes[i].Right; j != i; j = _nodes[j].Right)
            { ref var nodeJ = ref _nodes[j]; _nodes[nodeJ.Up].Down = nodeJ.Down; _nodes[nodeJ.Down].Up = nodeJ.Up; _nodes[nodeJ.ColHeader].Size--; }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Uncover(int c)
    {
        ref var rc = ref _nodes[c];
        for (int i = rc.Up; i != c; i = _nodes[i].Up) for (int j = _nodes[i].Left; j != i; j = _nodes[j].Left)
            { ref var nodeJ = ref _nodes[j]; _nodes[nodeJ.ColHeader].Size++; _nodes[nodeJ.Up].Down = j; _nodes[nodeJ.Down].Up = j; }
        _nodes[rc.Left].Right = c; _nodes[rc.Right].Left = c;
    }
}

#endregion