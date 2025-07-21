using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    const int Size = 9;

    static void Main()
    {
        var cells = new int[Size, Size];
        for (int r = 0; r < Size; r++)
        {
            var parts = Console.ReadLine().Split();
            for (int c = 0; c < Size; c++)
                cells[r, c] = int.Parse(parts[c]);
        }

        var board = new Board(cells);
        var hints = board.FindSwapHints();

        Console.WriteLine(hints.Count);
        foreach (var h in hints)
            Console.WriteLine($"{h.R1} {h.C1} {h.R2} {h.C2}");
    }
}

public struct SwapHint : IEquatable<SwapHint>
{
    public int R1 { get; }
    public int C1 { get; }
    public int R2 { get; }
    public int C2 { get; }

    public SwapHint(int r1, int c1, int r2, int c2)
    {
        R1 = r1; C1 = c1;
        R2 = r2; C2 = c2;
    }

    public bool Equals(SwapHint other) =>
        R1 == other.R1 && C1 == other.C1 &&
        R2 == other.R2 && C2 == other.C2;

    public override bool Equals(object obj) =>
        obj is SwapHint other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(R1, C1, R2, C2);
}

public class Board
{
    public const int Size = 9;
    private readonly int[,] _cells;

    // (match1 dr, dc), (match2 dr, dc), (swap dr, dc)
    private static readonly (int m1r,int m1c,
                             int m2r,int m2c,
                             int sr, int sc)[]
        MatchPatterns = {
        (-1,-1, -1,-2, -1, 0),
        (-2, 0, -3, 0, -1, 0),
        (-1,-1, -1, 1, -1, 0),
        (-1, 1, -1, 2, -1, 0),
        ( 0,-2,  0,-3,  0,-1),
        (-1,-1, -2,-1,  0,-1),
        (-1,-1,  1,-1,  0,-1),
        ( 1,-1,  2,-1,  0,-1),
        (-1, 1, -2, 1,  0, 1),
        (-1, 1,  1, 1,  0, 1),
        ( 1, 1,  2, 1,  0, 1),
        ( 0, 2,  0, 3,  0, 1),
        ( 1,-1,  1,-2,  1, 0),
        ( 2, 0,  3, 0,  1, 0),
        ( 1,-1,  1, 1,  1, 0),
        ( 1, 1,  1, 2,  1, 0)
    };

    public Board(int[,] cells)
    {
        _cells = cells;
    }

    public List<SwapHint> FindSwapHints()
    {
        var hints = new List<SwapHint>();

        for (int r = 0; r < Size; r++)
        for (int c = 0; c < Size; c++)
        {
            int orig = _cells[r, c];
            foreach (var v in MatchPatterns)
            {
                int r1 = r + v.m1r, c1 = c + v.m1c;
                int r2 = r + v.m2r, c2 = c + v.m2c;
                int rs = r + v.sr,  cs = c + v.sc;

                if (!InBounds(r1, c1) ||
                    !InBounds(r2, c2) ||
                    !InBounds(rs, cs))
                    continue;

                if (_cells[rs, cs] == orig) 
                    continue;

                if (_cells[r1, c1] == orig &&
                    _cells[r2, c2] == orig)
                {
                    var hint = (r < rs || (r == rs && c < cs))
                        ? new SwapHint(r, c, rs, cs)
                        : new SwapHint(rs, cs, r, c);
                    hints.Add(hint);
                }
            }
        }

        return hints
            .Distinct()
            .OrderBy(h => h.R1).ThenBy(h => h.C1)
            .ThenBy(h => h.R2).ThenBy(h => h.C2)
            .ToList();
    }

    private static bool InBounds(int r, int c) =>
        r >= 0 && r < Size && c >= 0 && c < Size;
}
