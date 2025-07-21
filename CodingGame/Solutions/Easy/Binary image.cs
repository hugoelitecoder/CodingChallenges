using System;
using System.Collections.Generic;
using System.Text;
class Solution
{
    public static void Main(string[] args)
    {
        var h = int.Parse(Console.ReadLine());
        var decodedRows = new List<string>();
        var width = -1;
        for (var i = 0; i < h; i++)
        {
            var tokens = Console.ReadLine().Split();
            var runs = new int[tokens.Length];
            for (var j = 0; j < tokens.Length; j++) runs[j] = int.Parse(tokens[j]);
            var row = DecodeLine(runs);
            if (i == 0) width = row.Length; else if (row.Length != width) { Console.WriteLine("INVALID"); return; }
            decodedRows.Add(row);
        }
        foreach (var row in decodedRows) Console.WriteLine(row);
    }
    private static string DecodeLine(int[] runs)
    {
        var sb = new StringBuilder();
        var isWhite = true;
        for (var i = 0; i < runs.Length; i++)
        {
            var count = runs[i];
            for (var j = 0; j < count; j++) sb.Append(isWhite ? '.' : 'O');
            isWhite = !isWhite;
        }
        return sb.ToString();
    }
}
