using System;
using System.Linq;

class Solution {
    static void Main() {
        var wh = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int w = wh[0], h = wh[1];
        string bs = Console.ReadLine();
        var map = new char[h][];
        for (int y = 0; y < h; y++)
            map[y] = Console.ReadLine().ToCharArray();

        if (bs == "0") {
            Print(map);
            return;
        }

        int t = GetTumbles(bs);
        for (int i = 0; i <= t; i++)
            map = Tumble(map);

        Print(map);
    }

    static int GetTumbles(string bs) {
        int[] mom = { 1, 1 };
        int drv = 0, t = 1;
        for (int i = bs.Length - 1; i >= 0; i--) {
            int o = bs[i] - '0';
            for (int b = 0; b < 3; b++) {
                if ((o & 1) != 0) t += mom[drv] % 2;
                mom[drv ^ 1] += mom[drv] % 2;
                o >>= 1;
                drv ^= 1;
            }
        }
        return t;
    }

    static char[][] Tumble(char[][] m) {
        int h = m.Length, w = m[0].Length;
        var r = new char[w][];
        for (int x = 0; x < w; x++) {
            r[x] = new char[h];
            for (int y = 0; y < h; y++)
                r[x][y] = m[y][x];
            int cnt = r[x].Count(c => c == '#');
            int gap = h - cnt;
            for (int y = 0; y < h; y++)
                r[x][y] = y < gap ? '.' : '#';
        }
        return r;
    }

    static void Print(char[][] m) {
        foreach (var row in m)
            Console.WriteLine(new string(row));
    }
}
