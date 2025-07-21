using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Program
{
    const string Labels = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    class Area { public int X, Y, Size; }
    struct Rect { public int r1, c1, r2, c2; public Rect(int a,int b,int c,int d){r1=a;c1=b;r2=c;c2=d;} }

    class DLX
    {
        public class Node
        {
            public Node L, R, U, D;
            public Column C;
            public int RowID;
        }
        public class Column : Node
        {
            public int Size;
            public int Name;
        }

        private Column header;
        private List<Column> columns = new List<Column>();
        private List<Node> solution = new List<Node>();
        public List<List<int>> AllSolutions = new List<List<int>>();

        public DLX(int columnCount)
        {
            header = new Column { Name = -1 };
            header.L = header.R = header;
            for (int i = 0; i < columnCount; i++)
            {
                var c = new Column { Name = i };
                c.U = c.D = c;
                c.R = header;
                c.L = header.L;
                header.L.R = c;
                header.L = c;
                columns.Add(c);
            }
        }

        public void AddRow(int rowID, IEnumerable<int> colIndices)
        {
            Node first = null;
            foreach (var ci in colIndices)
            {
                var col = columns[ci];
                var node = new Node { C = col, RowID = rowID };
                // insert into column
                node.D = col;
                node.U = col.U;
                col.U.D = node;
                col.U = node;
                col.Size++;
                if (first == null)
                {
                    first = node;
                    node.L = node.R = node;
                }
                else
                {
                    // insert into row
                    node.R = first;
                    node.L = first.L;
                    first.L.R = node;
                    first.L = node;
                }
            }
        }

        void Cover(Column c)
        {
            c.R.L = c.L; c.L.R = c.R;
            for (var i = c.D; i != c; i = i.D)
                for (var j = i.R; j != i; j = j.R)
                {
                    j.D.U = j.U;
                    j.U.D = j.D;
                    j.C.Size--;
                }
        }

        void Uncover(Column c)
        {
            for (var i = c.U; i != c; i = i.U)
                for (var j = i.L; j != i; j = j.L)
                {
                    j.C.Size++;
                    j.D.U = j;
                    j.U.D = j;
                }
            c.R.L = c;
            c.L.R = c;
        }

        public void Search()
        {
            if (header.R == header)
            {
                AllSolutions.Add(solution.Select(n => n.RowID).ToList());
                return;
            }
            // choose column with smallest size
            Column c = null;
            int min = int.MaxValue;
            for (var j = header.R; j != header; j = j.R)
            {
                var col = (Column)j;
                if (col.Size < min)
                {
                    min = col.Size; c = col;
                    if (min == 0) break;
                }
            }
            if (c == null || c.Size == 0) return;
            Cover(c);
            for (var r = c.D; r != c; r = r.D)
            {
                solution.Add(r);
                for (var j = r.R; j != r; j = j.R)
                    Cover(j.C);
                Search();
                solution.RemoveAt(solution.Count - 1);
                for (var j = r.L; j != r; j = j.L)
                    Uncover(j.C);
            }
            Uncover(c);
        }
    }

    static void Main()
    {
        var wh = Console.ReadLine().Split();
        int W = int.Parse(wh[0]), H = int.Parse(wh[1]);

        var areas = new List<Area>();
        var hasClue = new bool[H, W];
        for (int r = 0; r < H; r++)
        {
            var parts = Console.ReadLine().Split();
            for (int c = 0; c < W; c++)
            {
                int v = int.Parse(parts[c]);
                if (v > 0)
                {
                    areas.Add(new Area { X = c, Y = r, Size = v });
                    hasClue[r, c] = true;
                }
            }
        }

        int N = areas.Count;
        var candidates = new List<(int clueIdx, Rect rect)>();
        for (int i = 0; i < N; i++)
        {
            var a = areas[i];
            for (int r1 = 0; r1 < H; r1++)
            for (int c1 = 0; c1 < W; c1++)
            for (int r2 = r1; r2 < H; r2++)
            for (int c2 = c1; c2 < W; c2++)
            {
                if ((r2 - r1 + 1) * (c2 - c1 + 1) != a.Size) continue;
                bool ok = false, bad = false;
                for (int rr = r1; rr <= r2 && !bad; rr++)
                    for (int cc = c1; cc <= c2; cc++)
                    {
                        if (hasClue[rr, cc])
                        {
                            if (rr == a.Y && cc == a.X) ok = true;
                            else { bad = true; break; }
                        }
                    }
                if (ok && !bad)
                    candidates.Add((i, new Rect(r1, c1, r2, c2)));
            }
        }

        int C = N + W * H;
        var dlx = new DLX(C);
        for (int rid = 0; rid < candidates.Count; rid++)
        {
            var (ci, rc) = candidates[rid];
            var cols = new List<int> { ci };
            for (int rr = rc.r1; rr <= rc.r2; rr++)
                for (int cc = rc.c1; cc <= rc.c2; cc++)
                    cols.Add(N + (rr * W + cc));
            dlx.AddRow(rid, cols);
        }

        dlx.Search();
        var outputs = new List<string>();
        foreach (var sol in dlx.AllSolutions)
        {
            var cover = new int[H, W];
            foreach (var rid in sol)
            {
                var (ci, rc) = candidates[rid];
                for (int rr = rc.r1; rr <= rc.r2; rr++)
                    for (int cc = rc.c1; cc <= rc.c2; cc++)
                        cover[rr, cc] = ci;
            }
            var labelMap = new Dictionary<int, char>();
            char next = 'A';
            var sb = new StringBuilder(W * H);
            for (int r = 0; r < H; r++)
                for (int c = 0; c < W; c++)
                {
                    int idx = cover[r, c];
                    if (!labelMap.TryGetValue(idx, out char ch))
                    {
                        ch = next;
                        labelMap[idx] = ch;
                        next = (next == 'Z' ? 'a' : (char)(next + 1));
                    }
                    sb.Append(ch);
                }
            outputs.Add(sb.ToString());
        }

        outputs.Sort(StringComparer.Ordinal);
        Console.WriteLine(outputs.Count);
        var best = outputs[0];
        for (int r = 0; r < H; r++)
            Console.WriteLine(best.Substring(r * W, W));
    }
}
