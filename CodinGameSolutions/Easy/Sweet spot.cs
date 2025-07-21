using System;
using System.Collections.Generic;

class Solution {
    static int N;
    static char[][] grid;
    static int[,] wave;
    static bool[,] explodedB;
    static readonly Dictionary<char, List<(int dr,int dc,int v)>> patterns = new() {
        ['A'] = BuildA(),
        ['H'] = BuildH(),
        ['B'] = BuildB()
    };

    static void Main() {
        N = int.Parse(Console.ReadLine());
        grid = new char[N][];
        for (int i = 0; i < N; i++) grid[i] = Console.ReadLine().ToCharArray();
        wave = new int[N, N];
        explodedB = new bool[N, N];
        for (int r = 0; r < N; r++)
            for (int c = 0; c < N; c++)
                if (grid[r][c] == 'A' || grid[r][c] == 'H')
                    Explode(r, c, grid[r][c]);
        for (int r = 0; r < N; r++) {
            for (int c = 0; c < N; c++) {
                char ch = grid[r][c];
                if (ch == 'A' || ch == 'H' || ch == 'B')
                    Console.Write(ch);
                else {
                    int orig = ch - '0';
                    Console.Write((char)('0' + Math.Max(orig, wave[r, c])));
                }
            }
            Console.WriteLine();
        }
    }

    static void Explode(int r, int c, char type) {
        foreach (var (dr, dc, v) in patterns[type]) {
            int rr = r + dr, cc = c + dc;
            if (rr < 0 || rr >= N || cc < 0 || cc >= N) continue;
            if (v > wave[rr, cc]) wave[rr, cc] = v;
            if (grid[rr][cc] == 'B' && !explodedB[rr, cc]) {
                explodedB[rr, cc] = true;
                Explode(rr, cc, 'B');
            }
        }
    }

    static List<(int,int,int)> BuildA() {
        var L = new List<(int,int,int)>();
        for (int dr = -4; dr <= 4; dr++)
            for (int dc = -4; dc <= 4; dc++)
                if (!(dr == 0 && dc == 0)) {
                    int v = 4 - Math.Max(Math.Abs(dr), Math.Abs(dc));
                    if (v > 0) L.Add((dr, dc, v));
                }
        return L;
    }

    static List<(int,int,int)> BuildH() {
        var L = new List<(int,int,int)>();
        for (int dr = -3; dr <= 3; dr++)
            for (int dc = -3; dc <= 3; dc++)
                if (!(dr == 0 && dc == 0)) L.Add((dr, dc, 5));
        return L;
    }

    static List<(int,int,int)> BuildB() {
        var L = new List<(int,int,int)>();
        for (int d = 1; d <= 3; d++) {
            int v = 4 - d;
            L.Add(( d,  0, v)); L.Add((-d,  0, v));
            L.Add(( 0,  d, v)); L.Add(( 0, -d, v));
        }
        return L;
    }
}
