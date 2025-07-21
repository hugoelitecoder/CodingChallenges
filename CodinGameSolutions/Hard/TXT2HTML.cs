using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var n = int.Parse(Console.ReadLine());
        var raw = new List<string>();
        for (var i = 0; i < n; i++) raw.Add(Console.ReadLine());
        var table = new TextTable(raw);
        Console.WriteLine(table.ToHtml());
    }
}

class TextTable
{
    private List<string> _raw;
    private List<int> _rows;
    private List<int> _cols;

    public TextTable(List<string> raw)
    {
        _raw = raw;
        _rows = new List<int>();
        _cols = new List<int>();
        for (var i = 0; i < _raw.Count; i++)
        {
            var line = _raw[i];
            if (i == 0)
                for (var j = 0; j < line.Length; j++)
                    if (line[j] == '+') _cols.Add(j);
            if (line.Length > 0 && line[0] == '+') _rows.Add(i);
        }
    }

    public string ToHtml()
    {
        var html = new List<string> { "<table>" };
        for (var i = 0; i < _rows.Count - 1; i++)
        {
            var row = "<tr>";
            for (var j = 0; j < _cols.Count - 1; j++)
                row += "<td>" + GetCell(_rows[i], _rows[i + 1], _cols[j], _cols[j + 1]) + "</td>";
            row += "</tr>";
            html.Add(row);
        }
        html.Add("</table>");
        return string.Join("\n", html);
    }

    private string GetCell(int rowStart, int rowEnd, int colStart, int colEnd)
    {
        var result = "";
        var space = "";
        for (var x = rowStart + 1; x < rowEnd; x++)
        {
            if (result != "") space = " ";
            for (var y = colStart + 1; y < colEnd && y < _raw[x].Length; y++)
            {
                if (_raw[x][y] != ' ')
                {
                    result += space + _raw[x][y];
                    space = "";
                }
            }
        }
        return result;
    }
}
