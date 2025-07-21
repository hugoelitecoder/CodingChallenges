using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var N = Console.ReadLine();
        var C = Console.ReadLine();
        var S = int.Parse(Console.ReadLine());
        var printer = new BigNumberPrinter(C, S);
        var lines = printer.Render(N);
        foreach (var line in lines) Console.WriteLine(line);
    }
}

class BigNumberPrinter
{
    private readonly char _symbol;
    private readonly int _size;
    private static readonly int[][] DigitSegments = new int[][]
    {
        // Segments: 0 top, 1 topright, 2 bottomright, 3 bottom, 4 bottomleft, 5 topleft, 6 middle
        new[]{1,1,1,1,1,1,0}, // 0
        new[]{0,1,1,0,0,0,0}, // 1
        new[]{1,1,0,1,1,0,1}, // 2
        new[]{1,1,1,1,0,0,1}, // 3
        new[]{0,1,1,0,0,1,1}, // 4
        new[]{1,0,1,1,0,1,1}, // 5
        new[]{1,0,1,1,1,1,1}, // 6
        new[]{1,1,1,0,0,0,0}, // 7
        new[]{1,1,1,1,1,1,1}, // 8
        new[]{1,1,1,1,0,1,1}  // 9
    };

    public BigNumberPrinter(string symbol, int size)
    {
        _symbol = symbol[0];
        _size = size;
    }

    public List<string> Render(string number)
    {
        var lines = new List<string>();
        var digits = new List<int>();
        foreach (var ch in number) digits.Add(ch - '0');
        var h = 2 * _size + 3;
        for (var row = 0; row < h; ++row)
        {
            var parts = new List<string>();
            foreach (var d in digits)
                parts.Add(RenderDigitRow(d, row));
            lines.Add(string.Join(" ", parts).TrimEnd());
        }
        return lines;
    }

    private string RenderDigitRow(int d, int row)
    {
        var seg = DigitSegments[d];
        var w = _size + 2;
        var h = 2 * _size + 3;
        var s = _symbol;
        // Row types:
        if (row == 0) // top
            return " " + (seg[0] == 1 ? new string(s, _size) : new string(' ', _size)) + " ";
        if (row > 0 && row < _size + 1) // upper vertical
            return (seg[5] == 1 ? s.ToString() : " ") + new string(' ', _size) + (seg[1] == 1 ? s.ToString() : " ");
        if (row == _size + 1) // middle
            return " " + (seg[6] == 1 ? new string(s, _size) : new string(' ', _size)) + " ";
        if (row > _size + 1 && row < h - 1) // lower vertical
            return (seg[4] == 1 ? s.ToString() : " ") + new string(' ', _size) + (seg[2] == 1 ? s.ToString() : " ");
        // bottom
        return " " + (seg[3] == 1 ? new string(s, _size) : new string(' ', _size)) + " ";
    }
}
