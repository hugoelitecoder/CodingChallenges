using System;
using System.Collections.Generic;
class Solution {
    static void Main() {
        long ii = long.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());
        var orig = new char[n, n];
        for (int i = 0; i < n; i++) {
            var line = Console.ReadLine();
            for (int j = 0; j < n; j++) orig[i, j] = line[j];
        }
        var G = BuildRotations(orig, n);
        var (o, idx) = Sim(ii, n, G);
        Console.Write(G[o][idx / n, idx % n]);
    }
    const int R = 4;
    static char[][,] BuildRotations(char[,] a, int n) {
        var g = new char[R][,]; g[0] = a;
        for (int i = 1; i < R; i++) g[i] = RotateCw(g[i - 1], n);
        return g;
    }
    static char[,] RotateCw(char[,] a, int n) {
        var b = new char[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                b[j, n - 1 - i] = a[i, j];
        return b;
    }
    static (int, int) Sim(long ii, int n, char[][,] G) {
        int N = n * n, o = 0, idx = 0;
        var seen = new Dictionary<(int, int), long> { [(0, 0)] = 1 };
        var states = new List<(int, int)> { (0, 0) };
        long s = 1;
        while (s < ii) {
            int r = idx / n, c = idx % n;
            char v = G[o][r, c];
            if (v == '#') o = (o + 1) & 3;
            else if (v == '@') o = (o + 3) & 3;
            v = G[o][r, c];
            int t = char.IsLetter(v) ? char.ToUpper(v) - 'A' + 1 : 0;
            idx = (idx + t) % N;
            s++;
            var st = (o, idx);
            if (seen.TryGetValue(st, out var f)) {
                long len = s - f;
                long rem = (ii - s) % len;
                return states[(int)(f + rem - 1)];
            }
            seen[st] = s;
            states.Add(st);
        }
        return (o, idx);
    }
}