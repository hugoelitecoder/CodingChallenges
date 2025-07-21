using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    public static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var values = Console.ReadLine().Split(' ').Select(int.Parse).ToList();
        var height = new List<int>(values);
        var grid = new List<StringBuilder>();
        var maxLen = 0;
        var curHeight = 0;
        var finalHeight = -1;
        var minHeight = Math.Min(values.Min(), 0);
        if (minHeight < 0)
        {
            var offset = -minHeight + 1;
            for (var i = 0; i < height.Count; i++)
                height[i] += offset;
            if (values.Last() < 0)
                height.Add(offset);
            if (values.First() < 0)
                height.Insert(0, offset);
            curHeight = offset;
            finalHeight = -minHeight;
            for (var i = 0; i < offset + 1; i++)
                grid.Add(new StringBuilder());
        }
        while (height.Count > 0)
        {
            var cur = height[0];
            height.RemoveAt(0);
            for (var i = curHeight; i < cur; i++)
            {
                EnsureRow(grid, i);
                AppendSymbol(grid[i], '/', ref maxLen);
            }
            curHeight = cur - 1;
            var nextHeight = height.Count == 0
                ? finalHeight
                : Math.Min(height[0] - 2, cur - 2);
            for (var i = curHeight; i > nextHeight; i--)
            {
                EnsureRow(grid, i);
                AppendSymbol(grid[i], '\\', ref maxLen);
            }
            curHeight = nextHeight + 1;
        }
        for (var i = grid.Count - 1; i >= 0; i--)
            Console.WriteLine(grid[i].ToString().TrimEnd());
    }
    private static void EnsureRow(List<StringBuilder> grid, int row)
    {
        while (grid.Count <= row)
            grid.Add(new StringBuilder());
    }
    private static void AppendSymbol(StringBuilder sb, char symbol, ref int maxLen)
    {
        var pad = maxLen - sb.Length;
        if (pad > 0)
            sb.Append(' ', pad);
        sb.Append(symbol);
        maxLen++;
    }
}
