using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        var path = Console.ReadLine() ?? string.Empty;
        Bounds bounds = new BoundingBox().Calculate(path);
        int width = (bounds.MaxH - bounds.MinH + 1) * 3;
        int height = (bounds.MaxV - bounds.MinV + 1) * 2;
        int startH = -bounds.MinH * 3;
        int startV = -bounds.MinV * 2;
        List<StringBuilder> canvas = new Canvas().Create(width, height);
        DrawingData drawing = DrawingData.Prepare();
        new SnakeDrawer().Draw(path, canvas, drawing, startH, startV);
        foreach (var row in canvas)
            Console.WriteLine(row.ToString().TrimEnd());
    }
}

class Bounds
{
    public int MinH { get; }
    public int MaxH { get; }
    public int MinV { get; }
    public int MaxV { get; }

    public Bounds(int minH, int maxH, int minV, int maxV)
    {
        MinH = minH;
        MaxH = maxH;
        MinV = minV;
        MaxV = maxV;
    }
}

class BoundingBox
{
    public Bounds Calculate(string path)
    {
        int h = 0, v = 0;
        int minH = 0, maxH = 0, minV = 0, maxV = 0;
        foreach (char c in path)
        {
            switch (c)
            {
                case 'U': v--; minV = Math.Min(minV, v); break;
                case 'D': v++; maxV = Math.Max(maxV, v); break;
                case 'L': h--; minH = Math.Min(minH, h); break;
                case 'R': h++; maxH = Math.Max(maxH, h); break;
            }
        }
        return new Bounds(minH, maxH, minV, maxV);
    }
}

class DrawingData
{
    public List<char> Chars { get; }
    public int[] Moves { get; }
    public Dictionary<string, Direction> Directions { get; }

    private DrawingData(List<char> chars, int[] moves, Dictionary<string, Direction> dirs)
    {
        Chars = chars;
        Moves = moves;
        Directions = dirs;
    }

    public static DrawingData Prepare()
    {
        var chars = new List<char>
        {
            '+','-','+','|',' ','|','-','-','+','-','-','+',
            '|',' ','|','+','-','+','+','-','-','+',
            '-','-','|',' ','+','+','-','-','-','-',
            '+','+',' ','|','+',' ','|','-','-','+',
            '+','-','-','|',' ','+','|',' ','|','|',' ',
            '|','-','-','-','-','-','-'
        };
        int[] moves = { 0,1,2,0,1,2,  0,0,0,1,1,1 };
        string[] keys = { "U","R","D","L","_U","_R","_D","_L","UU","UR","UL","RU","RR","RD","DR","DD","DL","LU","LD","LL" };
        int[,] vals = {
            {12,0,-2},{18,3,0},{0,0,2},{6,-3,0},{0,0,0},{6,0,0},{12,0,0},{18,0,0},
            {48,0,-2},{42,3,0},{30,-3,0},{36,0,-2},{54,3,0},{30,0,2},{24,3,0},{48,0,2},{36,-3,0},{24,0,-2},{42,0,2},{54,-3,0}
        };
        var dirMap = new Dictionary<string, Direction>();
        for (int i = 0; i < keys.Length; i++)
            dirMap[keys[i]] = new Direction(vals[i,0], vals[i,1], vals[i,2]);
        return new DrawingData(chars, moves, dirMap);
    }
}

class Direction
{
    public int CharIndex { get; }
    public int DeltaH { get; }
    public int DeltaV { get; }

    public Direction(int charIndex, int dh, int dv)
    {
        CharIndex = charIndex;
        DeltaH = dh;
        DeltaV = dv;
    }
}

class Canvas
{
    public List<StringBuilder> Create(int width, int height)
    {
        string blank = new string(' ', width);
        return Enumerable.Range(0, height)
                         .Select(_ => new StringBuilder(blank))
                         .ToList();
    }
}

class SnakeDrawer
{
    public void Draw(string path, List<StringBuilder> canvas, DrawingData drawing, int startH, int startV)
    {
        int h = startH;
        int v = startV;
        for (int i = 0; i <= path.Length; i++)
        {
            string key = i == 0 ? path.Substring(0,1)
                : i == path.Length ? "_" + path.Last()
                : $"{path[i-1]}{path[i]}";
            Direction dir = drawing.Directions[key];
            for (int j = 0; j < 6; j++)
                canvas[v + drawing.Moves[j+6]][h + drawing.Moves[j]] = drawing.Chars[dir.CharIndex + j];
            h += dir.DeltaH;
            v += dir.DeltaV;
        }
    }
}
