using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    public static void Main()
    {
        int size = int.Parse(Console.ReadLine());
        int angle = int.Parse(Console.ReadLine());

        var matrix = new List<string[]>();
        for (int i = 0; i < size; i++)
            matrix.Add(Console.ReadLine().Split(' '));

        var rotations = 0;
        var tmp = angle;
        while ((tmp -= 90) > 0)
            rotations++;
        Console.Error.WriteLine($"DEBUG: initial angle={angle}, computed rotations={rotations}");
        for (int r = 0; r < rotations; r++)
            matrix = Rotate90(matrix);

        var diamond = new List<List<string>>();
        int dim = size * 2;
        for (int i = -size; i < size; i++)
        {
            var row = new List<string>();
            int y = i, x = 0;
            while (y < dim && x < dim)
            {
                if (y >= 0 && y < size && x >= 0 && x < size)
                    row.Add(matrix[y][x]);
                y++; x++;
            }
            if (row.Count > 0)
                diamond.Add(row);
        }

        foreach (var row in diamond)
        {
            var pad = new string(' ', size - row.Count);
            Console.WriteLine(pad + string.Join(" ", row) + pad);
        }
    }

    private static List<string[]> Rotate90(List<string[]> m)
    {
        int n = m.Count;
        var result = new List<string[]>();
        for (int j = 0; j < n; j++)
        {
            var row = new string[n];
            for (int i = 0; i < n; i++)
                row[i] = m[i][n - 1 - j];
            result.Add(row);
        }
        return result;
    }
    
}
