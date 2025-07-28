using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        var baseCells = new Dictionary<char, List<(int r, int c)>>();
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                char ch = Polyominoes.ShapeMap[r][c];
                if (ch == '.') continue;
                if (!baseCells.ContainsKey(ch))
                    baseCells[ch] = new List<(int r, int c)>();
                baseCells[ch].Add((r, c));
            }

        var shapes = new Dictionary<char, List<List<(int r, int c)>>>();
        foreach (var kv in baseCells)
        {
            char id = kv.Key;
            var pts0 = kv.Value;
            int minr = pts0.Min(p => p.r), minc = pts0.Min(p => p.c);
            var norm = pts0.Select(p => (r: p.r - minr, c: p.c - minc)).ToList();

            var orients = new List<List<(int r, int c)>>();
            foreach (bool flip in new[] { false, true })
                foreach (int rot in new[] { 0, 90, 180, 270 })
                {
                    var pts = norm.Select(p => Polyominoes.Transform(p, flip, rot)).ToList();
                    int r0 = pts.Min(p => p.r), c0 = pts.Min(p => p.c);
                    var finalPts = pts.Select(p => (r: p.r - r0, c: p.c - c0)).OrderBy(p => p.r).ThenBy(p => p.c).ToList();
                    if (!orients.Any(o2 => o2.SequenceEqual(finalPts)))
                        orients.Add(finalPts);
                }
            shapes[id] = orients;
        }

        string line;
        while ((line = Console.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            char[] pieceIds = line.Trim().ToCharArray();
            var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
            int H = dims[0], W = dims[1];
            var board = new char[H, W];
            for (int r = 0; r < H; r++)
            {
                var row = Console.ReadLine();
                for (int c = 0; c < W; c++)
                    board[r, c] = row[c];
            }

            var distinctPieceIds = pieceIds.Distinct().ToList();

            var oCells = new List<(int r, int c)>();
            for (int r = 0; r < H; r++)
                for (int c = 0; c < W; c++)
                    if (board[r, c] == 'O')
                        oCells.Add((r, c));

            int cellCount = oCells.Count;
            var cellToCol = new Dictionary<(int r, int c), int>();
            for (int i = 0; i < cellCount; i++)
                cellToCol[oCells[i]] = i;

            var pieceToCol = new Dictionary<char, int>();
            for (int i = 0; i < distinctPieceIds.Count; i++)
                pieceToCol[distinctPieceIds[i]] = cellCount + i;

            int totalCols = cellCount + distinctPieceIds.Count;
            int maxSolutionDepth = distinctPieceIds.Count;
            int maxNodes = 400000;

            var solver = new AlgorithmXSolver<Placement>(totalCols, totalCols, maxNodes, maxSolutionDepth);
            foreach (var id in distinctPieceIds)
            {
                var orientations = shapes[id];
                for (int o = 0; o < orientations.Count; o++)
                {
                    var pts = orientations[o];
                    int maxr = pts.Max(p => p.r), maxc = pts.Max(p => p.c);
                    for (int br = 0; br + maxr < H; br++)
                        for (int bc = 0; bc + maxc < W; bc++)
                        {
                            var cols = new List<int>();
                            bool ok = true;
                            foreach (var (dr, dc) in pts)
                            {
                                int rr = br + dr, cc = bc + dc;
                                if (board[rr, cc] != 'O') { ok = false; break; }
                                cols.Add(cellToCol[(r: rr, c: cc)]);
                            }
                            if (!ok) continue;

                            cols.Add(pieceToCol[id]);
                            var payload = new Placement(id, o, br, bc);
                            solver.AddRow(cols.ToArray(), cols.Count, payload);
                        }
                }
            }

            var stopwatch = Stopwatch.StartNew();
            Placement[] solution = solver.Solve();
            stopwatch.Stop();

            var outp = new char[H, W];
            for (int r = 0; r < H; r++)
                for (int c = 0; c < W; c++)
                    outp[r, c] = board[r, c] == 'O' ? '.' : board[r, c];

            if (solution != null)
            {
                foreach (var placement in solution)
                {
                    var (id, o, br, bc) = (placement.PieceId, placement.OrientationIndex, placement.BoardRow, placement.BoardCol);
                    foreach (var (dr, dc) in shapes[id][o])
                        outp[br + dr, bc + dc] = id;
                }
            }

            Console.Error.WriteLine($"[DEBUG] Time elapsed: {stopwatch.Elapsed.TotalMilliseconds:0.00} ms");
            for (int r = 0; r < H; r++)
            {
                for (int c = 0; c < W; c++)
                    Console.Write(outp[r, c]);
                Console.WriteLine();
            }
        }
    }
}

public static class Polyominoes
{
    public static readonly string[] ShapeMap = {
        "IICBBJJJ",
        "ICCCBLLJ",
        "IICBBLHJ",
        "EIMMLLHH",
        "EEEMMNNN",
        "EGGDAANN",
        "GGDDDAKK",
        "GFFFFAKK"
    };

    public static (int r, int c) Transform((int r, int c) p, bool flip, int rot)
    {
        int r = p.r, c = p.c;
        if (flip) c = -c;
        return rot switch
        {
            90 => (c, -r),
            180 => (-r, -c),
            270 => (-c, r),
            _ => (r, c),
        };
    }
}

public class Placement
{
    public char PieceId { get; }
    public int OrientationIndex { get; }
    public int BoardRow { get; }
    public int BoardCol { get; }

    public Placement(char pieceId, int orientationIndex, int boardRow, int boardCol)
    {
        PieceId = pieceId;
        OrientationIndex = orientationIndex;
        BoardRow = boardRow;
        BoardCol = boardCol;
    }
}

public class AlgorithmXSolver<T> where T : notnull
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
    private readonly T[] _solution;
    private int _solutionDepth;

    public AlgorithmXSolver(int numPrimaryColumns, int numTotalColumns, int maxNodes, int maxSolutionDepth)
    {
        var poolSize = numTotalColumns + 1 + maxNodes;
        _nodes = new DlxNode[poolSize];
        _solution = new T[maxSolutionDepth];
        _header = 0;

        for (int i = 0; i <= numTotalColumns; i++)
        {
            _nodes[i] = new DlxNode { Left = i, Right = i, Up = i, Down = i, ColHeader = i, Size = 0 };
        }

        _nodes[_header].Right = _header;
        _nodes[_header].Left = _header;
        for (int i = 1; i <= numPrimaryColumns; i++)
        {
            _nodes[i].Right = _nodes[_header].Right;
            _nodes[i].Left = _header;
            _nodes[_nodes[_header].Right].Left = i;
            _nodes[_header].Right = i;
        }
        _nodeCount = numTotalColumns + 1;
    }

    public void AddRow(int[] columns, int count, T rowPayload)
    {
        if (count == 0) return;

        int firstNode = -1;
        for (int ci = 0; ci < count; ci++)
        {
            int colHeaderNodeIndex = columns[ci] + 1;
            int newNodeIndex = _nodeCount++;

            _nodes[colHeaderNodeIndex].Size++;
            _nodes[newNodeIndex].RowPayload = rowPayload;
            _nodes[newNodeIndex].ColHeader = colHeaderNodeIndex;

            _nodes[newNodeIndex].Up = _nodes[colHeaderNodeIndex].Up;
            _nodes[newNodeIndex].Down = colHeaderNodeIndex;
            _nodes[_nodes[colHeaderNodeIndex].Up].Down = newNodeIndex;
            _nodes[colHeaderNodeIndex].Up = newNodeIndex;

            if (firstNode == -1)
            {
                firstNode = newNodeIndex;
                _nodes[newNodeIndex].Left = newNodeIndex;
                _nodes[newNodeIndex].Right = newNodeIndex;
            }
            else
            {
                _nodes[newNodeIndex].Left = _nodes[firstNode].Left;
                _nodes[newNodeIndex].Right = firstNode;
                _nodes[_nodes[firstNode].Left].Right = newNodeIndex;
                _nodes[firstNode].Left = newNodeIndex;
            }
        }
    }

    public T[] Solve()
    {
        return EnumerateSolutions().FirstOrDefault();
    }

    private IEnumerable<T[]> EnumerateSolutions()
    {
        if (_nodes[_header].Right == _header)
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
            _solution[_solutionDepth++] = _nodes[r_node].RowPayload;
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
        for (int c_header = _nodes[_header].Right; c_header != _header; c_header = _nodes[c_header].Right)
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
