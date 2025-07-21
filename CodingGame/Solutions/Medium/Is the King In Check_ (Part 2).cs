using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        const int Size = 8;
        char[,] rawBoard = new char[Size, Size];
        for (int y = 0; y < Size; y++)
        {
            var tokens = Console.ReadLine().Split(' ');
            for (int x = 0; x < Size; x++)
                rawBoard[x, y] = tokens[x][0];
        }

        var board = new ChessBoard(rawBoard);
        bool inCheck = board.IsInCheck();

        Console.WriteLine(inCheck ? "Check" : "No Check");
    }
}

class ChessBoard
{
    private struct Piece { public char Type; public int X, Y; }

    private const int Size = 8;
    private static readonly (int dx, int dy)[] KnightMoves =
    {
        (2,1),(1,2),(-1,2),(-2,1),(-2,-1),(-1,-2),(1,-2),(2,-1)
    };
    private static readonly (int dx, int dy)[] RookDirs =
    {
        (1,0),(-1,0),(0,1),(0,-1)
    };
    private static readonly (int dx, int dy)[] BishopDirs =
    {
        (1,1),(1,-1),(-1,1),(-1,-1)
    };
    private static readonly (int dx, int dy)[] QueenDirs =
    {
         (1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)
    };

    private int kingX, kingY;
    private ulong occupancy;
    private List<Piece> enemies = new List<Piece>(2);

    public ChessBoard(char[,] rawBoard)
    {
        occupancy = 0UL;
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                char c = rawBoard[x, y];
                if (c == 'k')
                {
                    kingX = x; kingY = y;
                    occupancy |= 1UL << (y * Size + x);
                }
                else if (c == 'B' || c == 'R' || c == 'Q' || c == 'N')
                {
                    enemies.Add(new Piece { Type = c, X = x, Y = y });
                    occupancy |= 1UL << (y * Size + x);
                }
            }
        }
    }

    public bool IsInCheck()
    {
        foreach (var p in enemies)
            if (AttacksKing(p))
                return true;
        return false;
    }

    private bool AttacksKing(Piece p)
    {
        switch (p.Type)
        {
            case 'N': return KnightAttacks(p);
            case 'B': return SlidingAttacks(p, BishopDirs);
            case 'R': return SlidingAttacks(p, RookDirs);
            case 'Q': return SlidingAttacks(p, QueenDirs);
            default: return false;
        }
    }

    private bool KnightAttacks(Piece p)
    {
        foreach (var (dx, dy) in KnightMoves)
            if (p.X + dx == kingX && p.Y + dy == kingY)
                return true;
        return false;
    }

    private bool SlidingAttacks(Piece p, (int dx, int dy)[] dirs)
    {
        foreach (var (dx, dy) in dirs)
            if (ScanRay(p, dx, dy))
                return true;
        return false;
    }

    private bool ScanRay(Piece p, int dx, int dy)
    {
        int x = p.X + dx, y = p.Y + dy;
        while (x >= 0 && x < Size && y >= 0 && y < Size)
        {
            int idx = y * Size + x;
            if (((occupancy >> idx) & 1UL) != 0)
            {
                return (x == kingX && y == kingY);
            }
            x += dx; y += dy;
        }
        return false;
    }
}