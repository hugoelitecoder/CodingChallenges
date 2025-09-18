using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    public static void Main(string[] args)
    {
        var inFlatStrings = new List<string>();
        for (var i = 0; i < 11; i++)
        {
            inFlatStrings.Add(Console.ReadLine() ?? "");
        }
        var inFlat = inFlatStrings.Select(s => s.ToCharArray()).ToArray();
        var unscrambler = new CubeUnscrambler();
        var outFlat = unscrambler.Solve(inFlat);
        foreach (var row in outFlat)
        {
            Console.WriteLine(new string(row));
        }
    }
}

public class CubeUnscrambler
{
    private static readonly string[] SolvedFlatTemplate =
    {
        "    UUU",
        "    UUU",
        "    UUU",
        "",
        "LLL FFF RRR BBB",
        "LLL FFF RRR BBB",
        "LLL FFF RRR BBB",
        "",
        "    DDD",
        "    DDD",
        "    DDD"
    };
    private static readonly char[][] _solvedFlat = SolvedFlatTemplate.Select(s => s.ToCharArray()).ToArray();

    public char[][] Solve(char[][] inFlat)
    {
        var outFlat = _solvedFlat.Select(r => r.ToArray()).ToArray();
        var inPieces = GetPieces(inFlat);
        var solvedPieces = GetPieces(_solvedFlat);
        var solvedMap = solvedPieces.ToDictionary(
            p => (p.Color, p.ContextKey),
            p => p.Pos2D
        );
        foreach (var inSticker in inPieces)
        {
            var key = (inSticker.Color, inSticker.ContextKey);
            if (solvedMap.TryGetValue(key, out var solvedCoord))
            {
                var (ra, ca) = inSticker.Pos2D;
                var (rb, cb) = solvedCoord;
                outFlat[rb][cb] = _solvedFlat[ra][ca];
            }
        }
        return outFlat;
    }

    private record struct Point3D(int X, int Y, int Z);
    private record struct Point2D(int R, int C);
    private record struct RawSticker(char Color, Point3D Pos3D, Point2D Pos2D);
    private record struct Sticker(char Color, Point2D Pos2D, string ContextKey);

    private List<Sticker> GetPieces(char[][] cube)
    {
        var rawStickers = new List<RawSticker>(54);
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                rawStickers.Add(new RawSticker(cube[i][4 + j], new Point3D(j, 0, 2 - i), new Point2D(i, 4 + j)));
                rawStickers.Add(new RawSticker(cube[4 + i][j], new Point3D(0, i, 2 - j), new Point2D(4 + i, j)));
                rawStickers.Add(new RawSticker(cube[4 + i][4 + j], new Point3D(j, i, 0), new Point2D(4 + i, 4 + j)));
                rawStickers.Add(new RawSticker(cube[4 + i][8 + j], new Point3D(2, i, j), new Point2D(4 + i, 8 + j)));
                rawStickers.Add(new RawSticker(cube[4 + i][12 + j], new Point3D(2 - j, i, 2), new Point2D(4 + i, 12 + j)));
                rawStickers.Add(new RawSticker(cube[8 + i][4 + j], new Point3D(j, 2, i), new Point2D(8 + i, 4 + j)));
            }
        }
        var stickersByCoord3D = new Dictionary<Point3D, List<char>>();
        foreach (var sticker in rawStickers)
        {
            if (!stickersByCoord3D.TryGetValue(sticker.Pos3D, out var colors))
            {
                colors = new List<char>();
                stickersByCoord3D[sticker.Pos3D] = colors;
            }
            colors.Add(sticker.Color);
        }
        var contextMap = new Dictionary<Point3D, string>();
        foreach (var entry in stickersByCoord3D)
        {
            entry.Value.Sort();
            contextMap[entry.Key] = new string(entry.Value.ToArray());
        }
        var pieces = new List<Sticker>(54);
        foreach (var rawSticker in rawStickers)
        {
            pieces.Add(new Sticker(rawSticker.Color, rawSticker.Pos2D, contextMap[rawSticker.Pos3D]));
        }
        return pieces;
    }
}