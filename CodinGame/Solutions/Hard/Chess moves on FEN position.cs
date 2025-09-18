using System;
using System.Collections.Generic;
using System.Text;

class Solution
{
    static void Main()
    {
        var fen = Console.ReadLine();
        var n = int.Parse(Console.ReadLine());
        var moves = new List<string>();
        for (int i = 0; i < n; i++) moves.Add(Console.ReadLine());

        var game = new ChessGame(fen);
        game.ApplyMoves(moves);
        Console.WriteLine(game.ToFEN());
    }
}

class ChessGame
{
    private readonly char[][] board;

    public ChessGame(string fen)
    {
        board = FENParser.Parse(fen);
    }

    public void ApplyMoves(List<string> moves)
    {
        foreach (var move in moves)
            Move.Apply(board, move);
    }

    public string ToFEN()
    {
        return FENParser.ToFEN(board);
    }
}

static class FENParser
{
    public static char[][] Parse(string fen)
    {
        var board = new char[8][];
        var ranks = fen.Split('/');
        for (int r = 0; r < 8; r++)
        {
            var row = new List<char>();
            foreach (var ch in ranks[r])
            {
                if (char.IsDigit(ch))
                {
                    for (int i = 0; i < ch - '0'; i++)
                        row.Add('.');
                }
                else
                    row.Add(ch);
            }
            board[r] = row.ToArray();
        }
        return board;
    }

    public static string ToFEN(char[][] board)
    {
        var result = new List<string>();
        foreach (var row in board)
        {
            int count = 0;
            var line = new StringBuilder();
            foreach (var ch in row)
            {
                if (ch == '.') count++;
                else
                {
                    if (count > 0) line.Append(count);
                    line.Append(ch);
                    count = 0;
                }
            }
            if (count > 0) line.Append(count);
            result.Add(line.ToString());
        }
        return string.Join("/", result);
    }
}

static class Move
{
    public static void Apply(char[][] board, string move)
    {
        int r1 = 8 - (move[1] - '0'), c1 = move[0] - 'a';
        int r2 = 8 - (move[3] - '0'), c2 = move[2] - 'a';
        char piece = board[r1][c1];

        if (piece == 'P' && c1 != c2 && board[r2][c2] == '.')
            board[r2 + 1][c2] = '.';
        if (piece == 'p' && c1 != c2 && board[r2][c2] == '.')
            board[r2 - 1][c2] = '.';

        board[r1][c1] = '.';

        if (move.Length == 5)
            board[r2][c2] = move[4];
        else
            board[r2][c2] = piece;

        if (char.ToLower(piece) == 'k' && Math.Abs(c2 - c1) == 2)
        {
            if (c2 == 6)
            {
                board[r1][7] = '.';
                board[r1][5] = piece == 'K' ? 'R' : 'r';
            }
            else if (c2 == 2)
            {
                board[r1][0] = '.';
                board[r1][3] = piece == 'K' ? 'R' : 'r';
            }
        }
    }
}
