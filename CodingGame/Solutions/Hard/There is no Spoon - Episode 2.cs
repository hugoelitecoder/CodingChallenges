using System;

public class Program
{
    struct Bridge { public int x, y, x2, y2, count; public bool isGuess; }
    enum Dir { Up, Down, Right, Left }

    static int w, h;
    static char[][] grid;
    static Node[,] nodes;
    static int[][] hCount, vCount, grp;
    static Bridge[] bridges;
    static int bCount, curX, curY, rolledBack;
    static readonly (int dx, int dy)[] offs = { (0, -1), (0, 1), (1, 0), (-1, 0) };

    public static void Main()
    {
        w = int.Parse(Console.ReadLine());
        h = int.Parse(Console.ReadLine());

        grid   = new char[h][];
        nodes  = new Node[h, w];
        hCount = new int[h][];
        vCount = new int[h][];
        grp    = new int[h][];

        for (int y = 0; y < h; y++)
        {
            grid[y] = Console.ReadLine().ToCharArray();
            hCount[y] = new int[w];
            vCount[y] = new int[w];
            grp[y]    = new int[w];
            for (int x = 0; x < w; x++)
            {
                char c = grid[y][x];
                if (c >= '1' && c <= '8')
                    nodes[y, x] = new Node(x, y, c);
            }
        }

        bridges = new Bridge[w * h * 4];
        Init();
        while (true)
        {
            Process();
            if (NextNode(ref curX, ref curY))
            {
                if (!TryGuess()) Rollback();
            }
            else if (Connected()) break;
            else Rollback();
        }

        for (int i = 0; i < bCount; i++)
        {
            var b = bridges[i];
            Console.WriteLine($"{b.x} {b.y} {b.x2} {b.y2} {b.count}");
        }
    }

    static void OrderPts(ref int x1, ref int y1, ref int x2, ref int y2)
    {
        if (x1 > x2 || (x1 == x2 && y1 > y2))
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
        }
    }

    static void Swap(ref int a, ref int b) { int t = a; a = b; b = t; }

    static bool NextNode(ref int x, ref int y)
    {
        for (int yy = y; yy < h; yy++)
            for (int xx = yy == y ? x : 0; xx < w; xx++)
                if (nodes[yy, xx] != null && nodes[yy, xx].Remaining > 0)
                {
                    x = xx; y = yy;
                    return true;
                }
        return false;
    }

    static bool Neighbor(int x, int y, Dir d, out int nx, out int ny)
    {
        var (dx, dy) = offs[(int)d];
        nx = x + dx; ny = y + dy;
        while (nx >= 0 && nx < w && ny >= 0 && ny < h)
        {
            if (grid[ny][nx] >= '0' && grid[ny][nx] <= '8')
                return true;
            nx += dx; ny += dy;
        }
        nx = ny = -1;
        return false;
    }

    static bool CanPlace(int x1, int y1, int x2, int y2)
    {
        if (grid[y1][x1] == '0' || grid[y2][x2] == '0') return false;
        if (y1 == y2)
        {
            int mi = Math.Min(x1, x2), ma = Math.Max(x1, x2);
            if (hCount[y1][mi] < 2)
            {
                int blk = 0;
                for (int i = mi + 1; i < ma; i++)
                    if (Neighbor(i, y1, Dir.Up, out var ux, out var uy))
                        blk += vCount[uy][ux];
                return blk == 0;
            }
        }
        else if (x1 == x2)
        {
            int mi = Math.Min(y1, y2), ma = Math.Max(y1, y2);
            if (vCount[mi][x1] < 2)
            {
                int blk = 0;
                for (int i = mi + 1; i < ma; i++)
                    if (Neighbor(x1, i, Dir.Left, out var lx, out var ly))
                        blk += hCount[ly][lx];
                return blk == 0;
            }
        }
        return false;
    }

    static void Update(int x1, int y1, int x2, int y2, int d)
    {
        grid[y1][x1] = (char)(grid[y1][x1] - d);
        grid[y2][x2] = (char)(grid[y2][x2] - d);
        if (y1 == y2) hCount[y1][Math.Min(x1, x2)] += d;
        if (x1 == x2) vCount[Math.Min(y1, y2)][x1] += d;
        nodes[y1, x1]?.ApplyBridge(d);
        nodes[y2, x2]?.ApplyBridge(d);
    }

    static void Record(int x1, int y1, int x2, int y2, int d, bool g)
    {
        rolledBack = 0;
        bridges[bCount++] = new Bridge { x = x1, y = y1, x2 = x2, y2 = y2, count = d, isGuess = g };
    }

    static void AddBridge(int x1, int y1, int x2, int y2, int d, bool g)
    {
        OrderPts(ref x1, ref y1, ref x2, ref y2);
        Update(x1, y1, x2, y2, d);
        Record(x1, y1, x2, y2, d, g);
    }

    static void Rollback()
    {
        Bridge b;
        do {
            b = bridges[--bCount];
            Update(b.x, b.y, b.x2, b.y2, -b.count);
        } while (!b.isGuess);
        curX = b.x; curY = b.y; rolledBack = 1;
    }

    static bool TryGuess()
    {
        if (rolledBack != 0 && bridges[bCount].y2 > bridges[bCount].y) return false;
        if (rolledBack == 0 && Neighbor(curX, curY, Dir.Right, out var nx, out var ny) && CanPlace(curX, curY, nx, ny))
        {
            AddBridge(curX, curY, nx, ny, 1, true);
            return true;
        }
        if (Neighbor(curX, curY, Dir.Down, out nx, out ny) && CanPlace(curX, curY, nx, ny))
        {
            AddBridge(curX, curY, nx, ny, 1, true);
            return true;
        }
        return false;
    }

    static int MaxGroup()
    {
        int m = 0;
        for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
                if (grp[r][c] > m) m = grp[r][c];
        return m;
    }

    static bool Connected()
    {
        for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++) grp[r][c] = 0;
        for (int i = 0; i < bCount; i++)
        {
            var b = bridges[i];
            int g1 = grp[b.y][b.x], g2 = grp[b.y2][b.x2];
            if (g1 == 0 && g2 == 0)
            {
                int g = MaxGroup() + 1;
                grp[b.y][b.x] = grp[b.y2][b.x2] = g;
            }
            else if (g1 == 0) grp[b.y][b.x] = g2;
            else if (g2 == 0) grp[b.y2][b.x2] = g1;
            else if (g1 != g2)
            {
                int mx = Math.Max(g1, g2), mn = Math.Min(g1, g2);
                for (int rr = 0; rr < h; rr++)
                    for (int cc = 0; cc < w; cc++)
                        if (grp[rr][cc] == mx) grp[rr][cc] = mn;
            }
        }
        return MaxGroup() == 1;
    }

    static void Init()
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (grid[y][x] > '0' && grid[y][x] <= '8')
                {
                    int total = 0, one = 0, mul = 0, sx = 0, sy = 0;
                    for (int i = 0; i < offs.Length; i++)
                    {
                        if (Neighbor(x, y, (Dir)i, out var nx, out var ny))
                        {
                            total++;
                            if (grid[ny][nx] <= '2') one++;
                            if (grid[ny][nx] > '1') { mul++; sx = nx; sy = ny; }
                        }
                    }
                    if (mul == 1) InitAdd(x, y, sx, sy);
                    if (grid[y][x] == '2' && total == 2 && one == 2)
                        for (int i = 0; i < offs.Length; i++)
                            if (Neighbor(x, y, (Dir)i, out var nx, out var ny))
                                InitAdd(x, y, nx, ny);
                }
        for (int i = 0; i < bCount; i++)
        {
            var b = bridges[i];
            Update(b.x, b.y, b.x2, b.y2, b.count);
        }
    }

    static void InitAdd(int x1, int y1, int x2, int y2)
    {
        OrderPts(ref x1, ref y1, ref x2, ref y2);
        for (int i = 0; i < bCount; i++)
            if (bridges[i].x == x1 && bridges[i].y == y1 && bridges[i].x2 == x2 && bridges[i].y2 == y2)
                return;
        Record(x1, y1, x2, y2, 1, false);
    }

    static void Process()
    {
        int ch;
        do
        {
            ch = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int need = grid[y][x] - '0';
                    if (need > 0 && need <= 8)
                    {
                        int tot = 0;
                        int[] mLink = new int[4];
                        for (int i = 0; i < offs.Length; i++)
                        {
                            if (Neighbor(x, y, (Dir)i, out var nx, out var ny) && CanPlace(x, y, nx, ny))
                            {
                                int m1 = 2;
                                if (y == ny) m1 -= hCount[y][Math.Min(x, nx)];
                                else m1 -= vCount[Math.Min(y, ny)][x];
                                int m2 = grid[ny][nx] > '1' ? 2 : 1;
                                mLink[i] = Math.Min(m1, m2);
                                tot += mLink[i];
                            }
                        }
                        if (need > tot) { Rollback(); y = h; x = w; }
                        else if (need == tot)
                        {
                            for (int i = 0; i < offs.Length; i++) if (mLink[i] > 0)
                            {
                                Neighbor(x, y, (Dir)i, out var nx, out var ny);
                                AddBridge(x, y, nx, ny, mLink[i], false);
                            }
                            ch++; curX = curY = 0;
                        }
                        else if (need == tot - 1)
                        {
                            for (int i = 0; i < offs.Length; i++) if (mLink[i] == 2)
                            {
                                Neighbor(x, y, (Dir)i, out var nx, out var ny);
                                AddBridge(x, y, nx, ny, 1, false);
                                ch++; curX = curY = 0;
                            }
                        }
                    }
                }
        } while (ch > 0);
    }
}

public class Node
{
    public int X { get; }
    public int Y { get; }
    public int Req { get; }
    public int Remaining { get; private set; }

    public Node(int x, int y, char c)
    {
        X = x; Y = y; Req = c - '0'; Remaining = Req;
    }

    public void ApplyBridge(int d)
    {
        Remaining -= d;
        if (Remaining < 0) throw new InvalidOperationException();
    }
}