using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    static void Main()
    {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int width  = dims[0];
        int height = dims[1];

        int[][] initial = new int[height][];
        for (int y = 0; y < height; y++)
        {
            var line = Console.ReadLine().PadRight(width);
            initial[y] = new int[width];
            for (int x = 0; x < width; x++)
                initial[y][x] = (line[x] == '#') ? 1 : 0;
        }

        var game = new GameOfLife(initial);
        var deadRow = new string(' ', width);
        var deadKey = string.Concat(Enumerable.Repeat(deadRow + "\n", height));

        var history = new List<string>();

        while (true)
        {
            string key = Serialize(game);
            if (key == deadKey)
            {
                Console.WriteLine("Death");
                return;
            }

            int idx = history.IndexOf(key);
            if (idx != -1)
            {
                if (idx == history.Count - 1)
                    Console.WriteLine("Still");
                else
                    Console.WriteLine("Oscillator " + (history.Count - idx));
                return;
            }

            history.Add(key);
            game.Advance();
        }
    }

    static string Serialize(GameOfLife game)
    {
        var sb = new StringBuilder();
        for (int r = 0; r < game.Rows; r++)
        {
            for (int c = 0; c < game.Cols; c++)
                sb.Append(game.IsAlive(r, c) ? '#' : ' ');
            sb.Append('\n');
        }
        return sb.ToString();
    }
}

public class GameOfLife {
    private int[,] _board;
    private readonly int _rows;
    private readonly int _cols;
    private static readonly (int dr, int dc)[] _neighbors = {
        (-1, -1), (-1, 0), (-1, 1),
        ( 0, -1),          ( 0, 1),
        ( 1, -1), ( 1, 0), ( 1, 1)
    };

    public int Rows => _rows;
    public int Cols => _cols;

    public GameOfLife(int[][] initial) {
        _rows = initial.Length;
        _cols = initial[0].Length;
        _board = new int[_rows, _cols];
        for (int r = 0; r < _rows; r++)
            for (int c = 0; c < _cols; c++)
                _board[r, c] = initial[r][c];
    }

    public void Advance() {
        var next = new int[_rows, _cols];
        for (int r = 0; r < _rows; r++)
            for (int c = 0; c < _cols; c++)
                next[r, c] = ShouldLive(r, c) ? 1 : 0;
        _board = next;
    }

    public bool IsAlive(int r, int c) {
        if (!InBounds(r, c)) throw new ArgumentOutOfRangeException();
        return _board[r, c] == 1;
    }

    public int GetLiveNeighborCount(int r, int c) {
        if (!InBounds(r, c)) throw new ArgumentOutOfRangeException();
        int count = 0;
        foreach (var (dr, dc) in _neighbors) {
            int nr = r + dr, nc = c + dc;
            if (InBounds(nr, nc) && _board[nr, nc] == 1)
                count++;
        }
        return count;
    }

    private bool ShouldLive(int r, int c) {
        int live = GetLiveNeighborCount(r, c);
        bool alive = _board[r, c] == 1;
        return live == 3 || (alive && live == 2);
    }

    private bool InBounds(int r, int c)
        => r >= 0 && r < _rows && c >= 0 && c < _cols;
}