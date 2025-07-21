using System;
using System.Collections.Generic;

class Solution
{
    struct Watcher { public int x, y, d; public Watcher(int x, int y, int d) { this.x = x; this.y = y; this.d = d; } }

    static void Main()
    {
        int H = int.Parse(Console.ReadLine()), W = int.Parse(Console.ReadLine());
        var grid = new char[H][];
        var watchers = new List<Watcher>();
        var obstacles = new List<(int x, int y)>();
        int youX = -1, youY = -1;

        for (int y = 0; y < H; y++)
        {
            grid[y] = Console.ReadLine().ToCharArray();
            for (int x = 0; x < W; x++)
            {
                char c = grid[y][x];
                if (c == 'Y') { youX = x; youY = y; }
                else if ("^>v<".IndexOf(c) >= 0) { watchers.Add(new Watcher(x, y, "^>v<".IndexOf(c))); obstacles.Add((x, y)); }
                else if (c != '.') obstacles.Add((x, y));
            }
        }

        int count = 0;
        foreach (var w in watchers)
            if (SeesYou(w, youX, youY, H, W, obstacles))
                count++;

        Console.WriteLine(count);
    }

    static bool SeesYou(Watcher w, int youX, int youY, int H, int W, List<(int x, int y)> obs)
    {
        var fan = BuildFan(w, H, W);
        return fan.Contains((youX, youY)) && !BuildHidden(fan, w, H, W, obs).Contains((youX, youY));
    }

    static HashSet<(int x, int y)> BuildFan(Watcher w, int H, int W)
    {
        var f = new HashSet<(int, int)>();
        int sx = w.x, sy = w.y, sz = 3;
        int bd = w.d == 0 ? sy : w.d == 2 ? H - sy : w.d == 1 ? W - sx : sx;

        for (int i = 0; i < bd; i++)
        {
            if (w.d == 0) { sx--; sy--; }
            else if (w.d == 1) { sx++; sy--; }
            else if (w.d == 2) { sx--; sy++; }
            else { sx--; sy--; }

            for (int k = 0; k < sz; k++)
                f.Add(w.d % 2 == 0 ? (sx + k, sy) : (sx, sy + k));

            sz += 2;
        }
        return f;
    }

    static HashSet<(int x, int y)> BuildHidden(HashSet<(int x, int y)> fan, Watcher w, int H, int W, List<(int x, int y)> obs)
    {
        var h = new HashSet<(int, int)>();
        int px = w.x, py = w.y, bd = w.d == 0 ? py : w.d == 2 ? H - py : w.d == 1 ? W - px : px;

        foreach (var (ox, oy) in obs)
        {
            if (!fan.Contains((ox, oy))) continue;
            int inc = 2;

            if (w.d == 0)
            {
                if (ox == px) for (int d = 1; d <= bd; d++) h.Add((ox, oy - d));
                else for (int d = 1; d <= bd; d++, inc++)
                    for (int k = 0; k < inc; k++)
                        h.Add((ox + (ox > px ? k : -k), oy - d));
            }
            else if (w.d == 1)
            {
                if (oy == py) for (int d = 1; d <= bd; d++) h.Add((ox + d, oy));
                else for (int d = 1; d <= bd; d++, inc++)
                    for (int k = 0; k < inc; k++)
                        h.Add((ox + d, oy + (oy > py ? k : -k)));
            }
            else if (w.d == 2)
            {
                if (ox == px) for (int d = 1; d <= bd; d++) h.Add((ox, oy + d));
                else for (int d = 1; d <= bd; d++, inc++)
                    for (int k = 0; k < inc; k++)
                        h.Add((ox + (ox > px ? k : -k), oy + d));
            }
            else
            {
                if (oy == py) for (int d = 1; d <= bd; d++) h.Add((ox - d, oy));
                else for (int d = 1; d <= bd; d++, inc++)
                    for (int k = 0; k < inc; k++)
                        h.Add((ox - d, oy + (oy > py ? k : -k)));
            }
        }
        return h;
    }
}
