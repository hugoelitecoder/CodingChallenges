using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    public static void Main(string[] args)
    {
        var size = int.Parse(Console.ReadLine());
        var moveCount = int.Parse(Console.ReadLine());
        var board = new GoBoard(size);
        for (var i = 0; i < size; i++)
        {
            board.ReadRow(i, Console.ReadLine());
        }
        var moves = new List<Move>();
        for (var i = 0; i < moveCount; i++)
        {
            moves.Add(Move.Parse(Console.ReadLine()));
        }

        var result = board.PlayMoves(moves);
        Console.WriteLine(result);
    }
}

public class GoBoard
{
    private readonly int _size;
    private char[,] _board;
    private readonly HashSet<string> _history = new HashSet<string>();
    private static readonly (int, int)[] _directions = { (-1,0),(1,0),(0,-1),(0,1) };

    public GoBoard(int size)
    {
        _size = size;
        _board = new char[_size, _size];
    }

    public void ReadRow(int row, string line)
    {
        for (var col = 0; col < _size; col++)
            _board[row, col] = line[col];
    }

    public string PlayMoves(List<Move> moves)
    {
        _history.Add(GetBoardString());
        foreach (var move in moves)
        {
            if (!IsValidMove(move))
                return "NOT_VALID";
            ExecuteMove(move);
            var boardStr = GetBoardString();
            if (_history.Contains(boardStr))
                return "NOT_VALID";
            _history.Add(boardStr);
        }
        return ToString();
    }

    private bool IsValidMove(Move move)
    {
        if (!InBounds(move.Row, move.Col)) return false;
        if (_board[move.Row, move.Col] != '.') return false;
        var snapshot = (char[,])_board.Clone();
        _board[move.Row, move.Col] = move.Color;
        var captured = TryCapture(move.OpponentColor());
        var hasLiberty = HasLiberty(move.Row, move.Col, move.Color, new bool[_size, _size]);
        var valid = hasLiberty || captured > 0;
        _board = snapshot; 
        return valid;
    }

    private void ExecuteMove(Move move)
    {
        _board[move.Row, move.Col] = move.Color;
        TryCapture(move.OpponentColor());
        if (!HasLiberty(move.Row, move.Col, move.Color, new bool[_size, _size]))
            RemoveGroup(move.Row, move.Col, move.Color, new bool[_size, _size]);
    }

    private int TryCapture(char enemyColor)
    {
        var total = 0;
        var visited = new bool[_size, _size];
        for (var r = 0; r < _size; r++)
        {
            for (var c = 0; c < _size; c++)
            {
                if (_board[r, c] == enemyColor && !visited[r, c])
                {
                    if (!HasLiberty(r, c, enemyColor, visited: new bool[_size, _size]))
                        total += RemoveGroup(r, c, enemyColor, new bool[_size, _size]);
                }
            }
        }
        return total;
    }

    private bool HasLiberty(int row, int col, char color, bool[,] visited)
    {
        var stack = new Stack<(int,int)>();
        stack.Push((row, col));
        visited[row, col] = true;
        while (stack.Count > 0)
        {
            var (r, c) = stack.Pop();
            foreach (var (dr, dc) in _directions)
            {
                var nr = r + dr;
                var nc = c + dc;
                if (InBounds(nr, nc))
                {
                    if (_board[nr, nc] == '.') return true;
                    if (_board[nr, nc] == color && !visited[nr, nc])
                    {
                        visited[nr, nc] = true;
                        stack.Push((nr, nc));
                    }
                }
            }
        }
        return false;
    }

    private int RemoveGroup(int row, int col, char color, bool[,] visited)
    {
        var stack = new Stack<(int,int)>();
        stack.Push((row, col));
        visited[row, col] = true;
        var count = 0;
        while (stack.Count > 0)
        {
            var (r, c) = stack.Pop();
            _board[r, c] = '.';
            count++;
            foreach (var (dr, dc) in _directions)
            {
                var nr = r + dr;
                var nc = c + dc;
                if (InBounds(nr, nc) && _board[nr, nc] == color && !visited[nr, nc])
                {
                    visited[nr, nc] = true;
                    stack.Push((nr, nc));
                }
            }
        }
        return count;
    }

    private bool InBounds(int r, int c)
    {
        return 0 <= r && r < _size && 0 <= c && c < _size;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < _size; i++)
        {
            for (var j = 0; j < _size; j++)
                sb.Append(_board[i, j]);
            sb.Append('\n');
        }
        return sb.ToString().TrimEnd();
    }

    private string GetBoardString()
    {
        var sb = new StringBuilder(_size * _size);
        for (var i = 0; i < _size; i++)
            for (var j = 0; j < _size; j++)
                sb.Append(_board[i, j]);
        return sb.ToString();
    }
}

public class Move
{
    public char Color { get; }
    public int Row { get; }
    public int Col { get; }
    public Move(char color, int row, int col)
    {
        Color = color;
        Row = row;
        Col = col;
    }
    public static Move Parse(string line)
    {
        var parts = line.Split();
        return new Move(parts[0][0], int.Parse(parts[1]), int.Parse(parts[2]));
    }
    public char OpponentColor()
    {
        return Color == 'B' ? 'W' : 'B';
    }
}
