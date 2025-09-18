using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main(string[] args) {
        var input = Console.ReadLine().Split();
        var direction = input[0];
        var arrowCount = int.Parse(input[1]);
        var height = int.Parse(input[2]);
        var thickness = int.Parse(input[3]);
        var spacing = int.Parse(input[4]);
        var indentStep = int.Parse(input[5]);

        for (int row = 0; row < height; row++) {
            int indent = CalculateIndent(row, height, indentStep, direction);
            string line = BuildArrowLine(indent, arrowCount, thickness, spacing, direction);
            Console.WriteLine(line);
        }
    }

    static int CalculateIndent(int row, int height, int step, string direction) {
        int half = height / 2;
        return direction == "right"
            ? (row <= half ? row : height - 1 - row) * step
            : Math.Abs(half - row) * step;
    }

    static string BuildArrowLine(int indent, int count, int thickness, int spacing, string direction) {
        char arrowChar = direction == "left" ? '<' : '>';
        string arrow = new string(arrowChar, thickness);
        string spaceBetween = new string(' ', spacing);
        string arrowGroup = string.Join(spaceBetween, Enumerable.Repeat(arrow, count));
        return new string(' ', indent) + arrowGroup;
    }
}

