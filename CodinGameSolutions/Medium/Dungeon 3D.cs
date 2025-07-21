using System;
using System.Collections.Generic;

class Solution {
    
    static void Main() {
        var parts = Console.ReadLine().Split();
        int L = int.Parse(parts[0]),
            R = int.Parse(parts[1]),
            C = int.Parse(parts[2]);
        int ln = int.Parse(Console.ReadLine());

        var grid = new char[L, R, C];
        int sl = 0, sr = 0, sc = 0;
        int tl = 0, tr = 0, tc = 0;
        int level = 0, row = 0;
        for (int i = 0; i < ln; i++) {
            string line = Console.ReadLine();
            if (line.Length == 0) continue;
            for (int col = 0; col < C; col++) {
                char ch = line[col];
                grid[level, row, col] = ch;
                if (ch == 'A') { sl = level; sr = row; sc = col; }
                if (ch == 'S') { tl = level; tr = row; tc = col; }
            }
            if (++row == R) { level++; row = 0; }
        }

        var dist = new int[L, R, C];
        for (int l = 0; l < L; l++)
            for (int r = 0; r < R; r++)
                for (int c = 0; c < C; c++)
                    dist[l, r, c] = -1;

        var q = new Queue<(int l,int r,int c)>();
        dist[sl, sr, sc] = 0;
        q.Enqueue((sl, sr, sc));

        int[] dl = { 0, 0, 0, 0, 1, -1 };
        int[] dr = { 1, 0, -1, 0, 0,  0 };
        int[] dc = { 0, 1,  0, -1, 0, 0 };

        while (q.Count > 0) {
            var (cl, cr, cc) = q.Dequeue();
            if (cl == tl && cr == tr && cc == tc) break;
            for (int d = 0; d < 6; d++) {
                int nl = cl + dl[d],
                    nr = cr + dr[d],
                    nc = cc + dc[d];
                if (nl < 0 || nl >= L || nr < 0 || nr >= R || nc < 0 || nc >= C)
                    continue;
                if (dist[nl, nr, nc] != -1) continue;
                if (grid[nl, nr, nc] == '#') continue;
                dist[nl, nr, nc] = dist[cl, cr, cc] + 1;
                q.Enqueue((nl, nr, nc));
            }
        }

        int answer = dist[tl, tr, tc];
        Console.WriteLine(answer >= 0 ? answer.ToString() : "NO PATH");
    }
}
