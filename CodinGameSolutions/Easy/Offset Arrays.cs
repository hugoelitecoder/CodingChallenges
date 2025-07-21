using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static Dictionary<string, (int offset, List<int> values)> arrays = new();

    static void Main() {
        int n = int.Parse(Console.ReadLine());

        for (int i = 0; i < n; i++) {
            var line = Console.ReadLine();
            ParseArray(line);
        }

        var query = Console.ReadLine();
        Console.WriteLine(Eval(query));
    }

    static void ParseArray(string line) {
        int nameEnd = line.IndexOf('[');
        string name = line[..nameEnd];
        int rangeStart = line.IndexOf('[') + 1;
        int rangeEnd = line.IndexOf(']');
        string[] rangeParts = line[rangeStart..rangeEnd].Split("..");
        int start = int.Parse(rangeParts[0]);
        string[] valuesStr = line[(rangeEnd + 4)..].Split(' ');
        var values = new List<int>();
        foreach (var v in valuesStr)
            values.Add(int.Parse(v));
        arrays[name] = (start, values);
    }

    static int Eval(string expr) {
        expr = expr.Trim();
        if (int.TryParse(expr, out int val))
            return val;

        int bracketStart = expr.IndexOf('[');
        string name = expr[..bracketStart];
        string inside = expr[(bracketStart + 1)..^1];  // Strip brackets
        int index = Eval(inside);

        var (offset, values) = arrays[name];
        return values[index - offset];
    }
}
