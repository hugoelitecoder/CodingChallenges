using System;
using System.Linq;
using System.Collections.Generic;

class Player
{
    private const int SIZE_MIN = 1;
    private const int SIZE_MAX = 3;
    private const int MAX_CANDIDATES = 50;
    private const double EPSILON = 1e-9;
    private const int INF_SCORE = int.MaxValue;
    private static readonly int[] COSTS = { 600, 1200, 2100 };
    private static readonly string[] VEHICLES = { "J", "H", "C" };
    private static readonly double MIN_SCORE = 1.0 / COSTS[0];

    class Drop
    {
        private readonly int _x, _y, _size;

        public Drop(int x, int y, int size)
        {
            _x = x;
            _y = y;
            _size = size;
        }

        public double Score(bool[,] grid)
        {
            int n = grid.GetLength(0);
            if (_x + _size > n || _y + _size > n)
                return 0;

            int fires = 0;
            for (int i = 0; i < _size; i++)
                for (int j = 0; j < _size; j++)
                    if (grid[_x + i, _y + j])
                        fires++;

            double baseScore = (double)fires / COSTS[_size - SIZE_MIN];
            return _size == SIZE_MIN ? baseScore + EPSILON : baseScore;
        }

        public int Cost => COSTS[_size - SIZE_MIN];

        public void Apply(bool[,] grid)
        {
            for (int i = 0; i < _size; i++)
                for (int j = 0; j < _size; j++)
                    grid[_x + i, _y + j] = false;
        }

        public override string ToString() => $"{VEHICLES[_size - SIZE_MIN]} {_x} {_y}";
    }

    class Board
    {
        private bool[,] _grid;

        public Board(bool[,] grid) => _grid = grid;

        private List<Drop> GenerateDrops()
        {
            var drops = new List<Drop>();
            int n = _grid.GetLength(0);
            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                    for (int size = SIZE_MIN; size <= SIZE_MAX; size++)
                    {
                        var d = new Drop(x, y, size);
                        if (d.Score(_grid) > MIN_SCORE)
                            drops.Add(d);
                    }
            return drops;
        }

        public Drop Solve()
        {
            var initialDrops = GenerateDrops();
            var candidates = initialDrops
                .OrderByDescending(d => d.Score(_grid))
                .Take(MAX_CANDIDATES)
                .ToList();

            var backup = (bool[,])_grid.Clone();
            int bestScore = INF_SCORE;
            Drop bestDrop = null;

            foreach (var drop in candidates)
            {
                drop.Apply(_grid);
                int totalCost = SolveGreedy(drop.Cost);
                Array.Copy(backup, _grid, backup.Length);

                if (totalCost < bestScore)
                {
                    bestScore = totalCost;
                    bestDrop = drop;
                }
            }

            return bestDrop;
        }

        private int SolveGreedy(int startCost)
        {
            int cost = startCost;
            var drops = GenerateDrops();

            while (drops.Count > 0)
            {
                Drop best = null;
                double bestScoreVal = 0;

                for (int i = 0; i < drops.Count; i++)
                {
                    var d = drops[i];
                    double sc = d.Score(_grid);
                    if (sc <= MIN_SCORE)
                    {
                        drops.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (sc > bestScoreVal)
                    {
                        bestScoreVal = sc;
                        best = d;
                    }
                }

                if (best == null) break;
                best.Apply(_grid);
                cost += best.Cost;
                drops.Remove(best);
            }

            return cost;
        }
    }

    static void Main(string[] args)
    {
        int length = int.Parse(Console.ReadLine());
        int water = int.Parse(Console.ReadLine());

        while (true)
        {
            bool[,] grid = new bool[length, length];
            int fireCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < fireCount; i++)
            {
                var parts = Console.ReadLine().Split(' ');
                grid[int.Parse(parts[0]), int.Parse(parts[1])] = true;
            }

            var board = new Board(grid);
            Console.WriteLine(board.Solve());
        }
    }
}