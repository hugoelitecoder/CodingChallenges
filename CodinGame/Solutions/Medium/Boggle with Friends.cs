using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    const int N = 4;
    static char[,] board = new char[N, N];
    static readonly (int dr, int dc)[] Dirs = {
        (-1,-1), (-1,0), (-1,1), (0,-1), (0,1), (1,-1), (1,0), (1,1)
    };

    static int Score(string w) => w.Length switch {
        3 or 4 => 1,
        5      => 2,
        6      => 3,
        7      => 5,
        _      => 11
    };

    static bool ExistsOnBoard(string w)
    {
        bool[,] seen = new bool[N, N];

        bool Dfs(int r, int c, int idx)
        {
            if (board[r, c] != w[idx]) return false;
            if (idx == w.Length - 1) return true;
            seen[r, c] = true;
            foreach (var (dr, dc) in Dirs)
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= N || nc < 0 || nc >= N) continue;
                if (!seen[nr, nc] && Dfs(nr, nc, idx + 1))
                {
                    seen[r, c] = false;
                    return true;
                }
            }
            seen[r, c] = false;
            return false;
        }

        for (int r = 0; r < N; r++)
            for (int c = 0; c < N; c++)
                if (board[r, c] == w[0] && Dfs(r, c, 0))
                    return true;
        return false;
    }

    class Friend
    {
        public string Name;
        public List<string> Raw;
        public List<string> Valid;
        public int Score;
    }

    static void Main()
    {
        for (int r = 0; r < N; r++)
        {
            var row = Console.ReadLine().Split();
            for (int c = 0; c < N; c++)
                board[r, c] = row[c][0];
        }

        int P = int.Parse(Console.ReadLine());
        var players = new List<Friend>();
        for (int i = 0; i < P; i++)
        {
            var line = Console.ReadLine().Split(' ', 3);
            players.Add(new Friend {
                Name = line[0],
                Raw  = line[2].Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList()
            });
        }

        var freq = new Dictionary<string,int>(StringComparer.Ordinal);
        foreach (var f in players)
        {
            f.Raw = f.Raw.Distinct().ToList();
            foreach (var w in f.Raw)
                freq[w] = freq.GetValueOrDefault(w,0) + 1;
        }

        foreach (var f in players)
        {
            f.Valid = f.Raw
                .Where(w => w.Length >= 3 && freq[w] == 1 && ExistsOnBoard(w))
                .ToList();
            f.Score = f.Valid.Sum(Score);
        }

        var win = players.OrderByDescending(f=>f.Score).First();

        Console.WriteLine($"{win.Name} is the winner!\n");
        Console.WriteLine("===Each Player's Score===");
        players.ForEach(f => Console.WriteLine($"{f.Name} {f.Score}"));
        Console.WriteLine("\n===Each Scoring Player's Scoring Words===");
        foreach (var f in players.Where(f=>f.Score>0))
        {
            Console.WriteLine(f.Name);
            foreach (var w in f.Valid)
                Console.WriteLine($"{Score(w)} {w}");
        }
    }
}
