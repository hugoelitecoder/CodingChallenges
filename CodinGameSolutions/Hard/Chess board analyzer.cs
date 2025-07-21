using System;

class Solution
{
    static void Main(string[] args)
    {
        var grid = new char[8, 8];
        for (var i = 0; i < 8; i++)
        {
            var boardRow = Console.ReadLine();
            for (var j = 0; j < 8; j++)
            {
                grid[i, j] = boardRow[j];
            }
        }
        var board = new ChessBoard(grid);
        var winner = board.GetWinner();
        Console.WriteLine(winner);
    }
}

class ChessBoard
{
    private readonly char[,] _grid;
    private (int r, int c) _whiteKingPos;
    private (int r, int c) _blackKingPos;

    public ChessBoard(char[,] grid)
    {
        _grid = grid;
        FindKings();
    }

    public string GetWinner()
    {
        if (IsKingInCheckmate(true))
        {
            return "B";
        }
        if (IsKingInCheckmate(false))
        {
            return "W";
        }
        return "N";
    }

    private void FindKings()
    {
        for (var r = 0; r < 8; r++)
        {
            for (var c = 0; c < 8; c++)
            {
                if (_grid[r, c] == 'K')
                {
                    _whiteKingPos = (r, c);
                }
                else if (_grid[r, c] == 'k')
                {
                    _blackKingPos = (r, c);
                }
            }
        }
    }

    private bool IsKingInCheckmate(bool isWhiteKing)
    {
        var kingPos = isWhiteKing ? _whiteKingPos : _blackKingPos;
        var kingChar = isWhiteKing ? 'K' : 'k';
        var isAttackedByWhite = !isWhiteKing;
        var attackerColor = isAttackedByWhite ? "White" : "Black";
        Console.Error.WriteLine($"\n--- Checking if {kingChar} at ({kingPos.r},{kingPos.c}) is in mate by {attackerColor} ---");

        var inCheck = IsSquareAttacked(kingPos.r, kingPos.c, isAttackedByWhite, null);
        Console.Error.WriteLine($"Is king currently in check? {inCheck}");
        if (!inCheck)
        {
            Console.Error.WriteLine("Result: King not in check, so not mate.");
            return false;
        }

        Console.Error.WriteLine("King is in check. Checking escape squares:");
        for (var dr = -1; dr <= 1; dr++)
        {
            for (var dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                var newR = kingPos.r + dr;
                var newC = kingPos.c + dc;

                Console.Error.Write($"  - Move to ({newR},{newC}): ");
                if (!IsValidSquare(newR, newC))
                {
                    Console.Error.WriteLine("Off board.");
                    continue;
                }
                var destPiece = _grid[newR, newC];
                var isFriendly = isWhiteKing ? char.IsUpper(destPiece) : char.IsLower(destPiece);
                if (isFriendly && destPiece != '.')
                {
                    Console.Error.WriteLine($"Blocked by friendly piece '{destPiece}'.");
                    continue;
                }

                if (!IsSquareAttacked(newR, newC, isAttackedByWhite, kingPos))
                {
                    Console.Error.WriteLine($"SAFE. King can escape.");
                    Console.Error.WriteLine("Result: Not mate, safe square found.");
                    return false;
                }
                else
                {
                    Console.Error.WriteLine($"ATTACKED.");
                }
            }
        }
        Console.Error.WriteLine("Result: No safe escape squares found. It's CHECKMATE.");
        return true;
    }

    private bool IsSquareAttacked(int r, int c, bool byWhite, (int r, int c)? ignorePos)
    {
        if (byWhite)
        {
            if (IsValidSquare(r + 1, c - 1) && _grid[r + 1, c - 1] == 'P') return true;
            if (IsValidSquare(r + 1, c + 1) && _grid[r + 1, c + 1] == 'P') return true;
        }
        else
        {
            if (IsValidSquare(r - 1, c - 1) && _grid[r - 1, c - 1] == 'p') return true;
            if (IsValidSquare(r - 1, c + 1) && _grid[r - 1, c + 1] == 'p') return true;
        }
        var knightMoves = new[] { (-2, -1), (-2, 1), (-1, -2), (-1, 2), (1, -2), (1, 2), (2, -1), (2, 1) };
        var enemyKnight = byWhite ? 'N' : 'n';
        foreach (var (dr, dc) in knightMoves)
        {
            var newR = r + dr;
            var newC = c + dc;
            if (IsValidSquare(newR, newC) && _grid[newR, newC] == enemyKnight) return true;
        }
        var enemyKing = byWhite ? 'K' : 'k';
        for (var dr = -1; dr <= 1; dr++)
        {
            for (var dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                var newR = r + dr;
                var newC = c + dc;
                if (IsValidSquare(newR, newC) && _grid[newR, newC] == enemyKing) return true;
            }
        }
        var straightDirs = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
        var enemyRook = byWhite ? 'R' : 'r';
        var enemyQueen = byWhite ? 'Q' : 'q';
        foreach (var (dr, dc) in straightDirs)
        {
            if (CheckLine(r, c, dr, dc, enemyRook, enemyQueen, ignorePos)) return true;
        }
        var diagDirs = new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) };
        var enemyBishop = byWhite ? 'B' : 'b';
        foreach (var (dr, dc) in diagDirs)
        {
            if (CheckLine(r, c, dr, dc, enemyBishop, enemyQueen, ignorePos)) return true;
        }
        return false;
    }

    private bool CheckLine(int startR, int startC, int dr, int dc, char slider1, char slider2, (int r, int c)? ignorePos)
    {
        var r = startR + dr;
        var c = startC + dc;
        while (IsValidSquare(r, c))
        {
            if (ignorePos.HasValue && r == ignorePos.Value.r && c == ignorePos.Value.c)
            {
                r += dr;
                c += dc;
                continue;
            }
            var piece = _grid[r, c];
            if (piece != '.')
            {
                return piece == slider1 || piece == slider2;
            }
            r += dr;
            c += dc;
        }
        return false;
    }

    private bool IsValidSquare(int r, int c)
    {
        return r >= 0 && r < 8 && c >= 0 && c < 8;
    }
}
