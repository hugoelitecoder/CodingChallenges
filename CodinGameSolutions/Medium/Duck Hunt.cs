using System;
using System.Collections.Generic;
using System.Linq;



class Solution
{
    static void Main(string[] args)
    {
        int w = int.Parse(Console.ReadLine());
        int h = int.Parse(Console.ReadLine());

        var frame1 = new string[h];
        for (int i = 0; i < h; i++)
            frame1[i] = Console.ReadLine();

        var frame2 = new string[h];
        for (int i = 0; i < h; i++)
            frame2[i] = Console.ReadLine();

        var game = new DuckGame(w, h, frame1, frame2);
        var shots = game.GetShots();

        foreach (var (id, x, y) in shots)
            Console.WriteLine($"{id} {x} {y}");
    }
}

class DuckGame
{
    private int _width, _height;
    private string[] _frame1, _frame2;

    private class Duck
    {
        public int Id;
        public int X1, Y1;
        public int Dx, Dy;
        public int LastTurn;
    }

    public DuckGame(int width, int height, string[] frame1, string[] frame2)
    {
        _width = width;
        _height = height;
        _frame1 = frame1;
        _frame2 = frame2;
    }

    public List<(int Id, int X, int Y)> GetShots()
    {
        var ducks = BuildDuckList();
        var shots = new List<(int, int, int)>();
        var alive = new LinkedList<Duck>(ducks);
        int turn = 3;

        while (alive.Count > 0)
        {
            while (alive.Count > 0 && alive.First.Value.LastTurn < turn)
                alive.RemoveFirst();

            if (alive.Count == 0) break;

            var d = alive.First.Value;
            alive.RemoveFirst();

            int sx = d.X1 + (turn - 1) * d.Dx;
            int sy = d.Y1 + (turn - 1) * d.Dy;
            shots.Add((d.Id, sx, sy));

            turn++;
        }

        return shots;
    }

    private List<Duck> BuildDuckList()
    {
        var pos1 = new Dictionary<int, (int x,int y)>();
        var pos2 = new Dictionary<int, (int x,int y)>();

        for (int y = 0; y < _height; y++)
        for (int x = 0; x < _width; x++)
        {
            if (char.IsDigit(_frame1[y][x]))
                pos1[_frame1[y][x] - '0'] = (x, y);
            if (char.IsDigit(_frame2[y][x]))
                pos2[_frame2[y][x] - '0'] = (x, y);
        }

        var list = new List<Duck>();
        foreach (var kv in pos1)
        {
            int id = kv.Key;
            var (x1, y1) = kv.Value;
            var (x2, y2) = pos2[id];

            int dx = x2 - x1;
            int dy = y2 - y1;

            int lastX = ComputeLastTurnForAxis(x1, dx, _width);
            int lastY = ComputeLastTurnForAxis(y1, dy, _height);
            int lastTurn = Math.Min(lastX, lastY);

            if (lastTurn >= 3)
            {
                list.Add(new Duck {
                    Id = id,
                    X1 = x1, Y1 = y1,
                    Dx = dx, Dy = dy,
                    LastTurn = lastTurn
                });
            }
        }

        return list.OrderBy(d => d.LastTurn).ToList();
    }

    private int ComputeLastTurnForAxis(int start, int delta, int limit)
    {
        if (delta == 0)
            return (start >= 0 && start < limit) ? int.MaxValue : 0;

        double bound = delta > 0
            ? (double)(limit - 1 - start) / delta
            : (double)(0 - start) / delta;
        return (int)Math.Floor(bound) + 1;
    }
}