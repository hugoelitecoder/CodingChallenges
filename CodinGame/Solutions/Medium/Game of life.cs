using System;
using System.Linq;

class Solution {
    static void Main() {
        var dims = Console.ReadLine().Split().Select(int.Parse).ToArray();
        int cols = dims[0], rows = dims[1];

        var initial = Enumerable.Range(0, rows)
            .Select(_ => Console.ReadLine().Trim())
            .Select(line => line.Select(ch => ch - '0').ToArray())
            .ToArray();

        var game = new GameOfLife(initial);
        game.Advance();

        for (int r = 0; r < game.Rows; r++) {
            Console.WriteLine(string.Concat(
                Enumerable.Range(0, game.Cols)
                    .Select(c => game.IsAlive(r, c) ? '1' : '0')
            ));
        }
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