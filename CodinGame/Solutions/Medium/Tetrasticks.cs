using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

class Player
{
    private const int ROWS = 6;
    private const int COLS = 6;
    private const int H_EDGES = ROWS * (COLS - 1);
    private const int V_EDGES = (ROWS - 1) * COLS;
    private const int EDGE_COUNT = H_EDGES + V_EDGES;
    private static readonly long MAX_SEARCH_TIME_MS = 4900;

    private class Placement
    {
        public char Id;
        public int Flip, Rotation, Row, Col;
        public ulong Mask, DoubleHorizontalMask, DoubleVerticalMask;
    }

    private static readonly Dictionary<char, (int x, int y, int dx, int dy)[]> Shapes = new() {
        {'F', new[]{(0,0,0,1),(1,0,0,1),(0,0,1,0),(1,0,1,0)}}, {'H', new[]{(0,0,1,0),(1,0,0,1),(1,0,1,0),(1,1,1,0)}},
        {'J', new[]{(0,1,1,0),(1,1,1,0),(1,0,1,0),(2,0,0,1)}}, {'L', new[]{(0,0,1,0),(1,0,1,0),(2,0,1,0),(3,0,0,1)}},
        {'N', new[]{(0,0,1,0),(1,0,0,1),(1,1,1,0),(2,1,1,0)}}, {'O', new[]{(0,0,0,1),(0,0,1,0),(0,1,1,0),(1,0,0,1)}},
        {'P', new[]{(0,0,0,1),(0,1,1,0),(1,0,0,1),(1,0,1,0)}}, {'R', new[]{(0,0,0,1),(0,1,1,0),(1,1,0,1),(1,1,1,0)}},
        {'T', new[]{(0,0,0,1),(0,1,0,1),(0,1,1,0),(1,1,1,0)}}, {'U', new[]{(0,0,1,0),(1,0,0,1),(1,1,0,1),(0,2,1,0)}},
        {'I', new[]{(0,0,1,0),(1,0,1,0),(2,0,1,0),(3,0,1,0)}}, {'V', new[]{(0,2,1,0),(1,2,1,0),(2,0,0,1),(2,1,0,1)}},
        {'W', new[]{(0,1,0,1),(0,1,1,0),(1,0,0,1),(1,0,1,0)}}, {'X', new[]{(0,1,1,0),(1,0,0,1),(1,1,0,1),(1,1,1,0)}},
        {'Y', new[]{(0,0,1,0),(1,0,0,1),(1,0,1,0),(2,0,1,0)}}, {'Z', new[]{(0,0,0,1),(0,1,1,0),(1,1,1,0),(2,1,0,1)}},
    };

    private static readonly List<Placement>[] AllPlacements = new List<Placement>[26];
    private static bool _isFirstTurn = true;
    private static bool _useGreedyMode = false;
    private static List<Placement> _solutionSequence;
    private static int _nextSolutionIndex;

    static Player()
    {
        for (int i = 0; i < 26; i++) AllPlacements[i] = new List<Placement>();
        PrecomputeAllPlacements();
    }

    public static void Main(string[] args)
    {
        while (true)
        {
            int m = int.Parse(Console.ReadLine()); // Unused input
            var remainingPieces = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToList();
            int n = int.Parse(Console.ReadLine());

            ulong occupiedEdges = 0;
            for (int i = 0; i < n; i++)
            {
                var parts = Console.ReadLine().Split(' ');
                char id = parts[0][0];
                int flip = int.Parse(parts[1]), rotation = int.Parse(parts[2]);
                int row = int.Parse(parts[3]), col = int.Parse(parts[4]);
                var pieceSegments = Shapes[id];
                int maxX = pieceSegments.Max(s => s.x + s.dx);
                int maxY = pieceSegments.Max(s => s.y + s.dy);

                foreach (var s in Transform(pieceSegments, flip, rotation, maxX, maxY))
                    occupiedEdges |= 1UL << EdgeIndex(s.x + row, s.y + col, s.dx, s.dy);
            }

            if (_isFirstTurn)
            {
                _isFirstTurn = false;
                var sw = Stopwatch.StartNew();

                _solutionSequence = SolvePuzzle(remainingPieces, occupiedEdges);
                sw.Stop();

                if (_solutionSequence != null)
                {
                    Console.Error.WriteLine($"[DEBUG] Exact solution found by solver in {sw.Elapsed.TotalMilliseconds:F2} ms.");
                }
                else
                {
                    _useGreedyMode = true;
                    Console.Error.WriteLine($"[DEBUG] Solver failed or timed out after {sw.Elapsed.TotalMilliseconds:F2} ms. Switching to greedy mode.");
                }
                _nextSolutionIndex = 0;
            }

            if (_useGreedyMode)
            {
                var move = FindGreedyMove(remainingPieces, occupiedEdges);
                Console.WriteLine($"{move.Id} {move.Flip} {move.Rotation} {move.Row} {move.Col}");
            }
            else
            {
                if (_solutionSequence == null || _nextSolutionIndex >= _solutionSequence.Count)
                {
                    _useGreedyMode = true;
                    Console.Error.WriteLine("[DEBUG] Solution sequence exhausted. Switching to greedy fallback.");
                    var move = FindGreedyMove(remainingPieces, occupiedEdges);
                    Console.WriteLine($"{move.Id} {move.Flip} {move.Rotation} {move.Row} {move.Col}");
                }
                else
                {
                    var move = _solutionSequence[_nextSolutionIndex++];
                    Console.WriteLine($"{move.Id} {move.Flip} {move.Rotation} {move.Row} {move.Col}");
                }
            }
        }
    }

    private static List<Placement> SolvePuzzle(List<char> remainingPieces, ulong occupiedEdges)
    {
        var edgeToColMap = new Dictionary<int, int>();
        for (int e = 0; e < EDGE_COUNT; e++)
            if ((occupiedEdges & (1UL << e)) == 0)
                edgeToColMap[e] = edgeToColMap.Count;

        int numEdgeCols = edgeToColMap.Count;
        int numShapeCols = remainingPieces.Count;
        int numVertexCols = (ROWS + 1) * (COLS + 1);
        int numPrimaryCols = numEdgeCols + numShapeCols;
        int numTotalCols = numPrimaryCols + numVertexCols;
        int maxNodes = 1200000;
        int maxSolutionDepth = numShapeCols;

        var solverOptions = new AlgorithmXOptions(Strategy: SolverStrategy.FindFirst, SortAndDedupRow: true);
        var solver = new AlgorithmXSolver<Placement>(numPrimaryCols, numTotalCols, maxNodes, maxSolutionDepth, solverOptions);

        var colBuffer = new int[EDGE_COUNT + 15 + ((ROWS + 1) * (COLS + 1))];
        for (int i = 0; i < numShapeCols; i++)
        {
            char pieceId = remainingPieces[i];
            foreach (var p in AllPlacements[pieceId - 'A'])
            {
                if ((p.Mask & occupiedEdges) != 0) continue;

                int colCount = 0;
                ulong edgeMask = p.Mask;
                while (edgeMask != 0)
                {
                    int e = BitOperations.TrailingZeroCount(edgeMask);
                    edgeMask &= edgeMask - 1;
                    if (edgeToColMap.TryGetValue(e, out var col))
                    {
                        colBuffer[colCount++] = col;
                    }
                }
                colBuffer[colCount++] = numEdgeCols + i;

                ulong bends = p.DoubleHorizontalMask | p.DoubleVerticalMask;
                while (bends != 0)
                {
                    int v = BitOperations.TrailingZeroCount(bends);
                    bends &= bends - 1;
                    colBuffer[colCount++] = numPrimaryCols + v;
                }
                solver.AddRow(colBuffer, colCount, p);
            }
        }

        Placement[] solution = null;
        var solveTask = Task.Run(() => solver.Solve());
        if (solveTask.Wait(TimeSpan.FromMilliseconds(MAX_SEARCH_TIME_MS)))
        {
            solution = solveTask.Result;
        }

        return solution?.ToList();
    }

    private static Placement FindGreedyMove(List<char> remainingPieces, ulong occupiedEdges)
    {
        foreach (char pieceId in remainingPieces)
        {
            foreach (var p in AllPlacements[pieceId - 'A'])
            {
                if ((p.Mask & occupiedEdges) == 0) return p;
            }
        }
        throw new Exception("Greedy mode could not find any valid move.");
    }

    private static void PrecomputeAllPlacements()
    {
        var uniqueOrientations = new HashSet<(int x, int y, int dx, int dy)[]>(new SegmentArrayComparer());

        foreach (var (id, segments) in Shapes)
        {
            var placementsForPiece = AllPlacements[id - 'A'];
            int maxX = segments.Max(s => s.x + s.dx);
            int maxY = segments.Max(s => s.y + s.dy);
            uniqueOrientations.Clear();

            for (int flip = 0; flip < 2; flip++) for (int rotation = 0; rotation < 4; rotation++)
                {
                    var transformed = Transform(segments, flip, rotation, maxX, maxY);

                    int minX = int.MaxValue, minY = int.MaxValue;
                    foreach (var s in transformed) { minX = Math.Min(minX, s.x); minY = Math.Min(minY, s.y); }

                    var canonicalSegs = transformed
                        .Select(s => (x: s.x - minX, y: s.y - minY, dx: s.dx, dy: s.dy))
                        .OrderBy(s => s.x).ThenBy(s => s.y).ThenBy(s => s.dx).ThenBy(s => s.dy)
                        .ToArray();

                    if (!uniqueOrientations.Add(canonicalSegs)) continue; // Symmetry breaking

                    int w = 0, h = 0;
                    foreach (var s in transformed) { w = Math.Max(w, s.x + s.dx); h = Math.Max(h, s.y + s.dy); }

                    for (int row = 0; row + w < ROWS; row++) for (int col = 0; col + h < COLS; col++)
                        {
                            Span<int> hCounts = stackalloc int[(ROWS + 1) * (COLS + 1)];
                            Span<int> vCounts = stackalloc int[(ROWS + 1) * (COLS + 1)];
                            ulong edgeMask = 0, dblH = 0, dblV = 0;

                            foreach (var s in transformed)
                            {
                                edgeMask |= 1UL << EdgeIndex(s.x + row, s.y + col, s.dx, s.dy);
                                int v0 = (row + s.x) * (COLS + 1) + (col + s.y);
                                int v1 = (row + s.x + s.dx) * (COLS + 1) + (col + s.y + s.dy);

                                if (s.dx == 0) // Vertical edge
                                {
                                    if (++hCounts[v0] == 2) dblH |= 1UL << v0;
                                    if (++hCounts[v1] == 2) dblH |= 1UL << v1;
                                }
                                else // Horizontal edge
                                {
                                    if (++vCounts[v0] == 2) dblV |= 1UL << v0;
                                    if (++vCounts[v1] == 2) dblV |= 1UL << v1;
                                }
                            }
                            placementsForPiece.Add(new Placement
                            {
                                Id = id,
                                Flip = flip,
                                Rotation = rotation,
                                Row = row,
                                Col = col,
                                Mask = edgeMask,
                                DoubleHorizontalMask = dblH,
                                DoubleVerticalMask = dblV
                            });
                        }
                }
        }
    }

    private static (int x, int y, int dx, int dy)[] Transform((int x, int y, int dx, int dy)[] src, int flip, int rot, int maxX, int maxY)
    {
        var dst = new (int x, int y, int dx, int dy)[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            var s = src[i];
            int x = s.x, y = s.y, dx = s.dx, dy = s.dy;

            if (flip == 1)
            {
                y = maxY - y - dy;
            }

            int currentMaxX = maxX, currentMaxY = maxY;
            for (int k = 0; k < rot; k++)
            {
                int nextX = y;
                int nextY = currentMaxX - x - dx;
                (dx, dy) = (dy, dx);
                x = nextX;
                y = nextY;
                (currentMaxX, currentMaxY) = (currentMaxY, currentMaxX);
            }
            dst[i] = (x, y, dx, dy);
        }
        return dst;
    }

    private static int EdgeIndex(int x, int y, int dx, int dy)
    {
        if (dx == 0 && dy == 1) return x * (COLS - 1) + y;
        if (dx == 1 && dy == 0) return H_EDGES + x * COLS + y;
        throw new ArgumentException("Invalid edge segment definition.");
    }

    private class SegmentArrayComparer : IEqualityComparer<(int x, int y, int dx, int dy)[]>
    {
        public bool Equals((int x, int y, int dx, int dy)[] s1, (int x, int y, int dx, int dy)[] s2)
        {
            if (s1.Length != s2.Length) return false;
            for (int i = 0; i < s1.Length; i++) if (s1[i] != s2[i]) return false;
            return true;
        }
        public int GetHashCode((int x, int y, int dx, int dy)[] segments)
        {
            var hash = new HashCode();
            foreach (var seg in segments) hash.Add(seg);
            return hash.ToHashCode();
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
        for (int i = 0; i <= numTotalColumns; i++)
        {
            _nodes[i] = new DlxNode { Left = i, Right = i, Up = i, Down = i, ColHeader = i, Size = 0 };
        }
        _nodes[_header].Right = _header; _nodes[_header].Left = _header;
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
        if (_options.SortAndDedupRow)
        {
            Array.Sort(columns, 0, count);
            int writeIdx = 1;
            for (int i = 1; i < count; i++) if (columns[i] != columns[i - 1]) columns[writeIdx++] = columns[i];
            count = writeIdx;
        }
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
            if (firstNode == -1)
            {
                firstNode = newNodeIdx; node.Left = newNodeIdx; node.Right = newNodeIdx;
            }
            else
            {
                node.Left = _nodes[firstNode].Left; node.Right = firstNode;
                _nodes[_nodes[firstNode].Left].Right = newNodeIdx; _nodes[firstNode].Left = newNodeIdx;
            }
        }
    }
    public T[] Solve()
    {
        T[] firstSolution = null;
        if (_options.Strategy == SolverStrategy.FindFirst)
        {
            Search(sol => { firstSolution = sol; return true; });
        }
        else
        {
            Search(sol => { if (firstSolution == null) firstSolution = sol; return false; });
        }
        return firstSolution;
    }
    private bool Search(Func<T[], bool> onSolutionFound)
    {
        if (_nodes[_header].Right == _header)
        {
            var result = new T[_solutionDepth];
            Array.Copy(_solutionBuffer, result, _solutionDepth);
            return onSolutionFound(result);
        }
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
        for (int i = colNode.Down; i != c; i = _nodes[i].Down)
        {
            for (int j = _nodes[i].Right; j != i; j = _nodes[j].Right)
            {
                ref var node = ref _nodes[j];
                _nodes[node.Up].Down = node.Down; _nodes[node.Down].Up = node.Up;
                _nodes[node.ColHeader].Size--;
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Uncover(int c)
    {
        ref var colNode = ref _nodes[c];
        for (int i = colNode.Up; i != c; i = _nodes[i].Up)
        {
            for (int j = _nodes[i].Left; j != i; j = _nodes[j].Left)
            {
                ref var node = ref _nodes[j];
                _nodes[node.ColHeader].Size++;
                _nodes[node.Up].Down = j; _nodes[node.Down].Up = j;
            }
        }
        _nodes[colNode.Left].Right = c; _nodes[colNode.Right].Left = c;
    }
}