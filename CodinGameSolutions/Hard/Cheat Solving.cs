using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        var grid = new List<string>();
        for (var i = 0; i < 11; i++) grid.Add(Console.ReadLine());
        var pattern = RubiksCube.BuildPattern();
        var cube = RubiksCube.FromInput(grid, pattern);
        Console.WriteLine(cube.IsRestickerable() && cube.IsSolvable() ? "SOLVABLE" : "UNSOLVABLE");
    }
}

class RubiksCube
{
    readonly List<(string piece, char sticker)> _mapping;
    readonly List<string> _pieceNames;

    RubiksCube(List<(string piece, char sticker)> mapping, List<string> pieceNames)
    {
        _mapping = mapping;
        _pieceNames = pieceNames;
    }

    static readonly Dictionary<string, string[,]> Faces = new()
    {
        ["U"] = new[,] { { "ULB", "UB", "UBR" }, { "UL", "U", "UR" }, { "UFL", "UF", "URF" } },
        ["L"] = new[,] { { "LBU", "LU", "LUF" }, { "LB", "L", "LF" }, { "LDB", "LD", "LFD" } },
        ["F"] = new[,] { { "FLU", "FU", "FUR" }, { "FL", "F", "FR" }, { "FDL", "FD", "FRD" } },
        ["R"] = new[,] { { "RFU", "RU", "RUB" }, { "RF", "R", "RB" }, { "RDF", "RD", "RBD" } },
        ["B"] = new[,] { { "BRU", "BU", "BUL" }, { "BR", "B", "BL" }, { "BDR", "BD", "BLD" } },
        ["D"] = new[,] { { "DLF", "DF", "DFR" }, { "DL", "D", "DR" }, { "DBL", "DB", "DRB" } }
    };

    public static string BuildPattern()
    {
        var lines = new List<string>();
        for (var i = 0; i < 3; i++)
            lines.Add("             " + string.Join(" ", Enumerable.Range(0, 3).Select(j => Faces["U"][i, j])));
        for (var i = 0; i < 3; i++)
            lines.Add(string.Join(" ",
                Enumerable.Range(0, 3).Select(j => Faces["L"][i, j])) + " " +
                string.Join(" ", Enumerable.Range(0, 3).Select(j => Faces["F"][i, j])) + " " +
                string.Join(" ", Enumerable.Range(0, 3).Select(j => Faces["R"][i, j])) + " " +
                string.Join(" ", Enumerable.Range(0, 3).Select(j => Faces["B"][i, j]))
            );
        for (var i = 0; i < 3; i++)
            lines.Add("             " + string.Join(" ", Enumerable.Range(0, 3).Select(j => Faces["D"][i, j])));
        return string.Join(" ", lines);
    }

    public static RubiksCube FromInput(List<string> grid, string pattern)
    {
        var stickers = grid.SelectMany(line => line.Where(char.IsLetter)).ToArray();
        var pieceNames = pattern.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).ToList();
        var mapping = pieceNames.Zip(stickers, (name, sticker) => (name, sticker)).ToList();
        return new RubiksCube(mapping, pieceNames);
    }

    public bool IsRestickerable()
    {
        var counts = _mapping.GroupBy(x => x.sticker).Select(g => g.Count()).OrderBy(x => x).ToList();
        return counts.Count == 6 && counts.All(x => x == 9);
    }

    public bool IsSolvable()
    {
        var ps = _pieceNames.OrderBy(x => x).ToList();
        var mapped = _pieceNames.Select(PieceAt).ToList();
        var sortedMapped = mapped.OrderBy(x => x, StringComparer.Ordinal).ToList();
        return ps.SequenceEqual(sortedMapped);
    }

    string PieceAt(string p)
    {
        var rots = Rotations(p);
        return string.Concat(rots.Select(rot => _mapping.First(x => x.piece == rot).sticker));
    }

    static IEnumerable<string> Rotations(string s)
    {
        var cur = s;
        for (var i = 0; i < s.Length; i++)
        {
            yield return cur;
            cur = cur.Substring(1) + cur[0];
        }
    }
}
