using System;
using System.Collections.Generic;
class Solution
{
    public static void Main(string[] args)
    {
        var rook = Console.ReadLine();
        var n = int.Parse(Console.ReadLine());
        var board = new int[9,9];
        for (var i = 1; i <= 8; i++)
            for (var j = 1; j <= 8; j++)
                board[i, j] = -1;
        for (var i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split();
            var color = int.Parse(parts[0]);
            var p = Parse(parts[1]);
            board[p.file, p.rank] = color;
        }
        var rp = Parse(rook);
        var moves = new List<string>();
        var dirs = new[] { (-1, 0), (1, 0), (0, 1), (0, -1) };
        foreach (var (df, dr) in dirs)
        {
            var f = rp.file + df;
            var r = rp.rank + dr;
            while (f >= 1 && f <= 8 && r >= 1 && r <= 8)
            {
                var occ = board[f, r];
                var target = PosToString(f, r);
                if (occ == -1)
                {
                    moves.Add($"R{rook}-{target}");
                }
                else if (occ == 1)
                {
                    moves.Add($"R{rook}x{target}");
                    break;
                }
                else
                {
                    break;
                }
                f += df;
                r += dr;
            }
        }
        moves.Sort();
        foreach (var mv in moves)
            Console.WriteLine(mv);
    }
    private static (int file, int rank) Parse(string s)
    {
        return (s[0] - 'a' + 1, s[1] - '0');
    }
    private static string PosToString(int file, int rank)
    {
        return $"{(char)('a' + file - 1)}{rank}";
    }
}
