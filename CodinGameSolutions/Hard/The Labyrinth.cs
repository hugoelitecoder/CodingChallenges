using System;
using System.Collections.Generic;

class Player
{
  
    static void Main()
    {
        var h = Console.ReadLine().Split();
        int R = int.Parse(h[0]), C = int.Parse(h[1]), A = int.Parse(h[2]);
        string mode = "SEARCH";
        while (true)
        {
            var p = Console.ReadLine().Split();
            int KR = int.Parse(p[0]), KC = int.Parse(p[1]);
            var maze = new char[R][];
            (int r, int c)? control = null;
            for (int i = 0; i < R; i++)
            {
                maze[i] = Console.ReadLine().ToCharArray();
                var j = Array.IndexOf(maze[i], 'C');
                if (j >= 0) control = (i, j);
            }
            if (mode == "SEARCH" && control.HasValue)
            {
                var info = BFS(maze, "T", "?#", control.Value.r, control.Value.c);
                if (info.HasValue && info.Value.Dist <= A) mode = "ACTIVATE";
            }
            if (mode == "ACTIVATE" && maze[KR][KC] == 'C') mode = "ESCAPE";
            var wb = plan[mode];
            var mv = BFS(maze, wb.want, wb.bad, KR, KC);
            Console.WriteLine(mv.HasValue ? mv.Value.InitDir : "UP");
        }
    }

    struct State
    {
        public int R, C, Dist;
        public string InitDir;
        public State(int r, int c, string dir, int d) { R = r; C = c; InitDir = dir; Dist = d; }
    }

    static readonly (int dr, int dc)[] dirs = { (-1, 0), (1, 0), (0, -1), (0, 1) };
    static readonly Dictionary<string, (string want, string bad)> plan = new()
    {
        ["SEARCH"]   = ("?", "#C"),
        ["ACTIVATE"] = ("C", "#?"),
        ["ESCAPE"]   = ("T", "#?")
    };

    static State? BFS(char[][] m, string want, string bad, int sr, int sc)
    {
        var q = new Queue<State>();
        var seen = new bool[m.Length * m[0].Length];
        foreach (var d in dirs)
        {
            int nr = sr + d.dr, nc = sc + d.dc;
            if (In(nr, nc, m)) q.Enqueue(new State(nr, nc, Dir(d.dr, d.dc), 1));
        }
        while (q.Count > 0)
        {
            var s = q.Dequeue();
            int idx = s.R * m[0].Length + s.C;
            if (seen[idx] || bad.Contains(m[s.R][s.C])) continue;
            if (want.Contains(m[s.R][s.C])) return s;
            seen[idx] = true;
            foreach (var d in dirs)
            {
                int nr = s.R + d.dr, nc = s.C + d.dc;
                if (In(nr, nc, m)) q.Enqueue(new State(nr, nc, s.InitDir, s.Dist + 1));
            }
        }
        return null;
    }

    static bool In(int r, int c, char[][] m) => r >= 0 && r < m.Length && c >= 0 && c < m[0].Length;
    static string Dir(int dr, int dc) => dr == -1 ? "UP" : dr == 1 ? "DOWN" : dc == -1 ? "LEFT" : "RIGHT";
}
