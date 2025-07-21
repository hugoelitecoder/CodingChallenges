using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static readonly int[] drs = { -1, -1, -1,  0,  0,  1,  1,  1 };
    static readonly int[] dcs = { -1,  0,  1, -1,  1, -1,  0,  1 };

    static void Main()
    {
        char[][] board = new char[10][];
        board[0] = Enumerable.Repeat('.', 10).ToArray();
        for (int i = 1; i <= 8; i++)
            board[i] = ("." + Console.ReadLine() + ".").ToCharArray();
        board[9] = Enumerable.Repeat('.', 10).ToArray();

        var input = Console.ReadLine().Split();
        char turn = input[0][0], opponent = turn == 'B' ? 'W' : 'B';
        int col = input[1][0] - 'a' + 1, row = input[1][1] - '1' + 1;

        if (board[row][col] != '-') { Console.WriteLine("NOPE"); return; }
        var flips = Enumerable.Range(0, 8)
                              .SelectMany(i => GetFlips(row, col, drs[i], dcs[i], board, turn, opponent))
                              .ToList();

        if (!flips.Any()) { Console.WriteLine("NULL"); return; }

        board[row][col] = turn;
        flips.ForEach(p => board[p.r][p.c] = turn);

        int wCount = board.Skip(1).Take(8).Sum(r => r.Count(c => c == 'W'));
        int bCount = board.Skip(1).Take(8).Sum(r => r.Count(c => c == 'B'));
        Console.WriteLine($"{wCount} {bCount}");
    }

    static IEnumerable<(int r, int c)> GetFlips(
        int row, int col, int dr, int dc,
        char[][] board, char turn, char opponent)
    {
        var line = new List<(int, int)>();
        int nr = row + dr, nc = col + dc;
        while (board[nr][nc] == opponent)
        {
            line.Add((nr, nc));
            nr += dr; nc += dc;
        }
        return line.Count > 0 && 
               board[nr][nc] == turn ? line : Enumerable.Empty<(int, int)>();
    }
}
