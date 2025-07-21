using System;
using System.Collections.Generic;

class Solution {

    static void Main() {
        int size = int.Parse(Console.ReadLine());
        var lines = new string[size];
        for (int i = 0; i < size; i++)
            lines[i] = Console.ReadLine().PadRight(size);

        int n = (size + 1) / 2;
        var grid    = new int[n, n];
        var rowUsed = new bool[n, n + 1];
        var colUsed = new bool[n, n + 1];
        var hor     = new int[n, n - 1];
        var ver     = new int[n - 1, n];

        for (int i = 0; i < n; i++) {
            var rowLine = lines[2 * i];
            for (int j = 0; j < n; j++) {
                int v = rowLine[2 * j] - '0';
                if (v > 0) {
                    grid[i, j] = v;
                    rowUsed[i, v] = colUsed[j, v] = true;
                }
                if (j < n - 1) {
                    char c = rowLine[2 * j + 1];
                    if (c == '<') hor[i, j] = -1;
                    else if (c == '>') hor[i, j] = 1;
                }
            }
        }
        for (int i = 0; i < n - 1; i++) {
            var colLine = lines[2 * i + 1];
            for (int j = 0; j < n; j++) {
                char c = colLine[2 * j];
                if (c == '^') ver[i, j] = -1;
                else if (c == 'v') ver[i, j] = 1;
            }
        }

        DFS(grid, rowUsed, colUsed, hor, ver, n);

        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++)
                Console.Write(grid[i, j]);
            Console.WriteLine();
        }
    }

    static bool DFS(int[,] grid, bool[,] rowUsed, bool[,] colUsed,
                    int[,] hor, int[,] ver, int n) {
        int br = -1, bc = -1, bestCount = int.MaxValue;
        List<int> best = null;

        for (int r = 0; r < n; r++) {
            for (int c = 0; c < n; c++) {
                if (grid[r, c] != 0) continue;
                var cand = new List<int>();
                for (int v = 1; v <= n; v++) {
                    if (rowUsed[r, v] || colUsed[c, v]) continue;
                    bool ok = true; int u;

                    if (ok && c > 0 && hor[r, c - 1] != 0) {
                        u = grid[r, c - 1];
                        if (u != 0 && (hor[r, c - 1] < 0 ? !(u < v) : !(u > v)))
                            ok = false;
                    }
                    if (ok && c < n - 1 && hor[r, c] != 0) {
                        u = grid[r, c + 1];
                        if (u != 0 && (hor[r, c] < 0 ? !(v < u) : !(v > u)))
                            ok = false;
                    }
                    if (ok && r > 0 && ver[r - 1, c] != 0) {
                        u = grid[r - 1, c];
                        if (u != 0 && (ver[r - 1, c] < 0 ? !(u < v) : !(u > v)))
                            ok = false;
                    }
                    if (ok && r < n - 1 && ver[r, c] != 0) {
                        u = grid[r + 1, c];
                        if (u != 0 && (ver[r, c] < 0 ? !(v < u) : !(v > u)))
                            ok = false;
                    }
                    if (ok) cand.Add(v);
                }

                if (cand.Count == 0) return false;
                if (cand.Count < bestCount) {
                    bestCount = cand.Count;
                    br = r; bc = c;
                    best = cand;
                }
            }
        }

        if (br == -1) return true;

        foreach (int v in best) {
            grid[br, bc]    = v;
            rowUsed[br, v]  = true;
            colUsed[bc, v]  = true;
            if (DFS(grid, rowUsed, colUsed, hor, ver, n))
                return true;
            rowUsed[br, v]  = false;
            colUsed[bc, v]  = false;
            grid[br, bc]    = 0;
        }
        return false;
    }
}
