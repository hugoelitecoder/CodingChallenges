using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Player
{
    static readonly Dictionary<char, (int x, int y, int dx, int dy)[]> Shapes = new() {
        {'F', new[]{(0,0,0,1),(1,0,0,1),(0,0,1,0),(1,0,1,0)}}, {'H', new[]{(0,0,1,0),(1,0,0,1),(1,0,1,0),(1,1,1,0)}},
        {'J', new[]{(0,1,1,0),(1,1,1,0),(1,0,1,0),(2,0,0,1)}}, {'L', new[]{(0,0,1,0),(1,0,1,0),(2,0,1,0),(3,0,0,1)}},
        {'N', new[]{(0,0,1,0),(1,0,0,1),(1,1,1,0),(2,1,1,0)}}, {'O', new[]{(0,0,0,1),(0,0,1,0),(0,1,1,0),(1,0,0,1)}},
        {'P', new[]{(0,0,0,1),(0,1,1,0),(1,0,0,1),(1,0,1,0)}}, {'R', new[]{(0,0,0,1),(0,1,1,0),(1,1,0,1),(1,1,1,0)}},
        {'T', new[]{(0,0,0,1),(0,1,0,1),(0,1,1,0),(1,1,1,0)}}, {'U', new[]{(0,0,1,0),(1,0,0,1),(1,1,0,1),(0,2,1,0)}},
        {'I', new[]{(0,0,1,0),(1,0,1,0),(2,0,1,0),(3,0,1,0)}}, {'V', new[]{(0,2,1,0),(1,2,1,0),(2,0,0,1),(2,1,0,1)}},
        {'W', new[]{(0,1,0,1),(0,1,1,0),(1,0,0,1),(1,0,1,0)}}, {'X', new[]{(0,1,1,0),(1,0,0,1),(1,1,0,1),(1,1,1,0)}},
        {'Y', new[]{(0,0,1,0),(1,0,0,1),(1,0,1,0),(2,0,1,0)}}, {'Z', new[]{(0,0,0,1),(0,1,1,0),(1,1,1,0),(2,1,0,1)}},
    };
    static readonly (char id, int f, int r, int R, int C)[] PrecomputedY = new (char, int, int, int, int)[] {
      ('F',0,1,4,3),('I',0,1,5,1),('H',1,0,2,4),('N',1,0,2,3),('O',0,0,4,0),
      ('W',0,0,2,0),('P',0,0,1,0),('V',0,2,0,0),('U',0,0,0,1),('L',0,1,0,2),
      ('J',0,0,0,4),('R',0,3,0,3),('Z',0,1,2,1),('T',0,3,1,2),('X',0,0,3,1)
    };
    class Placement { public char id; public int f, r, R, C; public ulong Mask, DblH, DblV; }
    const int ROWS = 6, COLS = 6; const int H_EDGES = ROWS * (COLS - 1), V_EDGES = (ROWS - 1) * COLS; const int EDGE_COUNT = H_EDGES + V_EDGES;
    static readonly List<Placement>[] Placements = new List<Placement>[26];
    static bool FirstTurn = true, GreedyMode = false; static List<Placement> SolutionSequence; static int NextIndex; static readonly long maxSearchTimeMs = 4900;
    static Player()
    {
        for (int i = 0; i < 26; i++) Placements[i] = new List<Placement>();
        foreach (var kv in Shapes)
        {
            char id = kv.Key; var segs = kv.Value; var list = Placements[id - 'A'];
            int maxX = 0, maxY = 0; foreach (var s in segs) { maxX = Math.Max(maxX, s.x + s.dx); maxY = Math.Max(maxY, s.y + s.dy); }
            for (int f = 0; f < 2; f++) for (int r = 0; r < 4; r++)
                {
                    var trans = Transform(segs, f, r, maxX, maxY);
                    int w = 0, h = 0; foreach (var s in trans) { w = Math.Max(w, s.x + s.dx); h = Math.Max(h, s.y + s.dy); }
                    for (int R = 0; R + w < ROWS; R++) for (int C = 0; C + h < COLS; C++)
                        {
                            Span<int> hCount = stackalloc int[(ROWS + 1) * (COLS + 1)]; Span<int> vCount = stackalloc int[(ROWS + 1) * (COLS + 1)];
                            ulong em = 0, dblH = 0, dblV = 0;
                            foreach (var s in trans)
                            {
                                em |= 1UL << EdgeIndex(s.x + R, s.y + C, s.dx, s.dy);
                                int v0 = (R + s.x) * (COLS + 1) + (C + s.y); int v1 = (R + s.x + s.dx) * (COLS + 1) + (C + s.y + s.dy);
                                if (s.dx == 0) { if (++hCount[v0] == 2) dblH |= 1UL << v0; if (++hCount[v1] == 2) dblH |= 1UL << v1; }
                                else { if (++vCount[v0] == 2) dblV |= 1UL << v0; if (++vCount[v1] == 2) dblV |= 1UL << v1; }
                            }
                            list.Add(new Placement { id = id, f = f, r = r, R = R, C = C, Mask = em, DblH = dblH, DblV = dblV });
                        }
                }
        }
    }
    static (int x, int y, int dx, int dy)[] Transform((int x, int y, int dx, int dy)[] src, int flip, int rot, int maxX, int maxY) { var dst = new (int x, int y, int dx, int dy)[src.Length]; int i = 0; foreach (var s in src) { int x = s.x, y = s.y, dx = s.dx, dy = s.dy; if (flip == 1) y = maxY - y - dy; int cx = maxX, cy = maxY; for (int k = 0; k < rot; k++) { int nx = y, ny = cx - x - dx; (dx, dy, x, y) = (dy, dx, nx, ny); (cx, cy) = (cy, cx); } dst[i++] = (x, y, dx, dy); } return dst; }
    static int EdgeIndex(int x, int y, int dx, int dy) { if (dx == 0 && dy == 1) return x * (COLS - 1) + y; if (dx == 1 && dy == 0) return H_EDGES + x * COLS + y; throw new ArgumentException(); }

    public static void Main(string[] args)
    {
        while (true)
        {
            int m = int.Parse(Console.ReadLine());
            var remain = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToList();
            int n = int.Parse(Console.ReadLine());
            var sw = new Stopwatch();

            ulong occupied = 0;
            for (int i = 0; i < n; i++)
            {
                var parts = Console.ReadLine().Split(' ');
                char id = parts[0][0]; int f = int.Parse(parts[1]), r = int.Parse(parts[2]);
                int R = int.Parse(parts[3]), C = int.Parse(parts[4]);
                foreach (var s in Transform(Shapes[id], f, r, Shapes[id].Max(x => x.x + x.dx), Shapes[id].Max(x => x.y + x.dy)))
                    occupied |= 1UL << EdgeIndex(s.x + R, s.y + C, s.dx, s.dy);
            }

            if (FirstTurn)
            {
                FirstTurn = false;
                sw.Restart();

                if (n == 0 && remain.Count == 15)
                {
                    var missing = Shapes.Keys.FirstOrDefault(k => !remain.Contains(k));
                    if (missing == 'Y')
                    {
                        SolutionSequence = PrecomputedY
                            .Select(t => new Placement { id = t.id, f = t.f, r = t.r, R = t.R, C = t.C }).ToList();
                        GreedyMode = false;
                        sw.Stop();
                        Console.Error.WriteLine($"[DEBUG] Using precomputed solution for missing Y");
                        NextIndex = 0;
                        goto Output;
                    }
                }

                var edgeToColMap = new Dictionary<int, int>();
                for (int e = 0; e < EDGE_COUNT; e++)
                    if ((occupied & (1UL << e)) == 0)
                        edgeToColMap[e] = edgeToColMap.Count;

                int numEdgeCols = edgeToColMap.Count;
                int numShapeCols = remain.Count;
                int numVertexCols = (ROWS + 1) * (COLS + 1);

                int numPrimaryCols = numEdgeCols + numShapeCols;
                int numTotalCols = numPrimaryCols + numVertexCols;
                int maxNodes = 1200000;
                int maxSolutionDepth = numShapeCols;

                var solverOptions = new AlgorithmXOptions(Strategy: SolverStrategy.FindFirst, SortAndDedupRow: true);
                var solver = new AlgorithmXSolver<Placement>(numPrimaryCols, numTotalCols, maxNodes, maxSolutionDepth, solverOptions);

                var colBuf = new int[60 + 15 + 49];
                for (int si = 0; si < numShapeCols; si++)
                {
                    char pieceId = remain[si];
                    foreach (var p in Placements[pieceId - 'A'])
                    {
                        if ((p.Mask & occupied) != 0) continue;
                        int colCount = 0;
                        ulong edgeMask = p.Mask;
                        while (edgeMask != 0)
                        {
                            int e = BitOperations.TrailingZeroCount(edgeMask);
                            edgeMask &= edgeMask - 1;
                            if (edgeToColMap.TryGetValue(e, out var col))
                            {
                                colBuf[colCount++] = col;
                            }
                        }
                        colBuf[colCount++] = numEdgeCols + si;
                        ulong bends = p.DblH | p.DblV;
                        while (bends != 0)
                        {
                            int v = BitOperations.TrailingZeroCount(bends);
                            bends &= bends - 1;
                            colBuf[colCount++] = numPrimaryCols + v;
                        }
                        solver.AddRow(colBuf, colCount, p);
                    }
                }

                Placement[] solution = null; bool timedOut = false;
                var solveTask = Task.Run(() => solver.Solve());
                if (solveTask.Wait(TimeSpan.FromMilliseconds(maxSearchTimeMs))) { solution = solveTask.Result; }
                else { timedOut = true; }

                sw.Stop();
                if (timedOut)
                {
                    GreedyMode = true;
                    Console.Error.WriteLine($"[DEBUG] Solver timed out after {sw.Elapsed.TotalMilliseconds:F2} ms. Switching to greedy mode.");
                }
                else if (solution != null)
                {
                    SolutionSequence = solution.ToList();
                    Console.Error.WriteLine($"[DEBUG] Exact solution found by solver in {sw.Elapsed.TotalMilliseconds:F2} ms.");
                }
                else
                {
                    GreedyMode = true;
                    Console.Error.WriteLine($"[DEBUG] No solution found by solver in {sw.Elapsed.TotalMilliseconds:F2} ms. Switching to greedy mode.");
                }
                NextIndex = 0;
            }

        Output:
            if (GreedyMode)
            {
                Placement pick = null;
                foreach (char fid in remain)
                {
                    foreach (var p in Placements[fid - 'A'])
                        if ((p.Mask & occupied) == 0) { pick = p; goto FoundGreedy; }
                }
            FoundGreedy:
                if (pick == null) throw new Exception("Greedy mode could not find any valid move.");
                Console.WriteLine($"{pick.id} {pick.f} {pick.r} {pick.R} {pick.C}");
            }
            else
            {
                if (SolutionSequence == null || NextIndex >= SolutionSequence.Count)
                {
                    GreedyMode = true;
                    Console.Error.WriteLine("[DEBUG] Solution sequence exhausted. Switching to greedy mode.");
                    goto Output;
                }
                var sol = SolutionSequence[NextIndex++];
                Console.WriteLine($"{sol.id} {sol.f} {sol.r} {sol.R} {sol.C}");
            }
        }
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