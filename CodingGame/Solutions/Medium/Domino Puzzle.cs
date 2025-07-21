using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var grid = new Domino[N, N];
        for (int i = 0; i < N; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            for (int j = 0; j < N; j++)
                grid[i, j] = new Domino(parts[j][0]);
        }

        var q = new Queue<(int r, int c, int dr, int dc)>();
        var start = grid[0, 0];
        if (!start.Fallen)
        {
            start.Fallen = true;
            foreach (var (dr, dc) in start.GetInitialFalls())
                q.Enqueue((0, 0, dr, dc));
        }

        while (q.Count > 0)
        {
            var (r, c, drFall, dcFall) = q.Dequeue();
            foreach (var (drOff, dcOff) in grid[r, c].GetHitOffsets(drFall, dcFall))
            {
                int nr = r + drOff, nc = c + dcOff;
                if (nr < 0 || nr >= N || nc < 0 || nc >= N) continue;
                var next = grid[nr, nc];
                if (next.Fallen) continue;
                int drIn = -drOff, dcIn = -dcOff;
                if (!next.CanBeHit(drIn, dcIn)) continue;
                next.Fallen = true;
                q.Enqueue((nr, nc, drOff, dcOff));
            }
        }

        int standing = 0;
        foreach (var d in grid)
            if (!d.Fallen)
                standing++;

        Console.WriteLine(standing);
    }
}

class Domino
{
    public char Symbol;
    public bool Fallen;

    public Domino(char symbol)
    {
        Symbol = symbol;
        Fallen = symbol == '.';
    }

    public List<(int dr, int dc)> GetInitialFalls() => Symbol switch
    {
        '|'  => new() { (0,  1) },
        '-'  => new() { (1,  0) },
        '/'  => new() { (0,  1), (1,  0), (1,  1) },
        '\\' => new() { (0, -1), (1,  0), (1, -1) },
        _    => new()
    };

    public bool CanBeHit(int drIn, int dcIn) => Symbol switch
    {
        '|'  => !((drIn == -1 && dcIn == 0) || (drIn == 1  && dcIn == 0)),
        '-'  => !((drIn == 0  && dcIn == -1) || (drIn == 0  && dcIn == 1)),
        '/'  => !((drIn == -1 && dcIn == 1) || (drIn == 1  && dcIn == -1)),
        '\\' => !((drIn == -1 && dcIn == -1)|| (drIn == 1  && dcIn == 1)),
        _    => false
    };

    public List<(int dr, int dc)> GetHitOffsets(int drFall, int dcFall) => Symbol switch
    {
        '|'  => new() { (0, dcFall > 0 ? 1 : -1) },
        '-'  => new() { (drFall > 0 ? 1 : -1, 0) },
        '/'  => (drFall > 0 || dcFall > 0)
                ? new() { (0,  1), (1,  0), (1,  1) }
                : new() { (0, -1), (-1, 0), (-1, -1) },
        '\\' => (drFall > 0 || dcFall < 0)
                ? new() { (0, -1), (1,  0), (1, -1) }
                : new() { (0,  1), (-1, 0), (-1,  1) },
        _    => new()
    };
}