using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var pieces = Console.ReadLine();
        var before = new string[8];
        var after = new string[8];
        var i = 0;

        while (i < 8)
        {
            before[i] = Console.ReadLine();
            i++;
        }

        i = 0;
        while (i < 8)
        {
            after[i] = Console.ReadLine();
            i++;
        }

        var result = ChessBoardMoveSolver.Solve(pieces, before, after);

        Console.Error.WriteLine("[DEBUG] Pieces=" + pieces);
        Console.Error.WriteLine("[DEBUG] Move=" + result.Move);
        Console.Error.WriteLine("[DEBUG] Knight=" + result.IsKnight);

        Console.WriteLine(result.Move);
        Console.WriteLine(result.IsKnight ? "Knight" : "Other");
    }
}

readonly struct ChessBoardMoveResult
{
    public readonly string Move;
    public readonly bool IsKnight;

    public ChessBoardMoveResult(string move, bool isKnight)
    {
        Move = move;
        IsKnight = isKnight;
    }
}

static class ChessBoardMoveSolver
{
    public static ChessBoardMoveResult Solve(string pieces, string[] before, string[] after)
    {
        var allPieces = BuildAllPieces(pieces);
        var startFile = 'a';
        var startRank = '8';
        var endFile = 'a';
        var endRank = '8';
        var taken = false;
        var i = 0;

        while (i < 8)
        {
            var j = 0;
            while (j < 8)
            {
                var beforeCell = before[i][j];
                var afterCell = after[i][j];

                if (afterCell != beforeCell)
                {
                    if (!allPieces.Contains(afterCell))
                    {
                        if (allPieces.Contains(beforeCell))
                        {
                            startFile = (char)('a' + j);
                            startRank = (char)('8' - i);
                        }
                    }
                    else
                    {
                        endFile = (char)('a' + j);
                        endRank = (char)('8' - i);
                        taken = allPieces.Contains(beforeCell);
                    }
                }

                j++;
            }

            i++;
        }

        var move = new string(new[] { startFile, startRank }) + (taken ? "x" : "-") + new string(new[] { endFile, endRank });
        var dh = startFile - endFile;
        if (dh < 0)
        {
            dh = -dh;
        }

        var dv = startRank - endRank;
        if (dv < 0)
        {
            dv = -dv;
        }

        var isKnight = (dh == 2 && dv == 1) || (dh == 1 && dv == 2);

        return new ChessBoardMoveResult(move, isKnight);
    }

    private static HashSet<char> BuildAllPieces(string pieces)
    {
        var allPieces = new HashSet<char>();
        var i = 0;

        while (i < pieces.Length)
        {
            var upper = pieces[i];
            var lower = char.ToLowerInvariant(upper);
            allPieces.Add(upper);
            allPieces.Add(lower);
            i++;
        }

        return allPieces;
    }
}