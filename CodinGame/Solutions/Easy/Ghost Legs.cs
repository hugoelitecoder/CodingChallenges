using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main(string[] args) {
        var input = Console.ReadLine().Split();
        int W = int.Parse(input[0]);
        int H = int.Parse(input[1]);

        var lines = new string[H];
        for (int i = 0; i < H; i++)
            lines[i] = Console.ReadLine();

        var topLine = lines[0];
        var bottomLine = lines[H - 1];

        var topToColumn = new List<(char label, int col)>();
        for (int i = 0; i < W; i++)
            if (lines[1][i] == '|')
                topToColumn.Add((topLine[i], i));

        foreach (var (label, colStart) in topToColumn) {
            int col = colStart;
            for (int row = 1; row < H - 1; row++) {
                if (col > 0 && lines[row][col - 1] == '-') col -= 3;
                else if (col + 1 < W && lines[row][col + 1] == '-') col += 3;
            }
            char bottom = bottomLine[col];
            Console.WriteLine($"{label}{bottom}");
        }
    }
}
