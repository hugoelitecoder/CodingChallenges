using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main(string[] args) {
        int N = int.Parse(Console.ReadLine());
        var resistors = new Dictionary<string, double>();
        for (int i = 0; i < N; i++) {
            var line = Console.ReadLine().Split();
            resistors[line[0]] = double.Parse(line[1]);
        }

        var tokens = Console.ReadLine().Split();
        int index = 0;
        double result = Parse(tokens, resistors, ref index);
        Console.WriteLine($"{Math.Round(result, 1):0.0}");
    }

    static double Parse(string[] tokens, Dictionary<string, double> map, ref int i) {
        if (tokens[i] == "(" || tokens[i] == "[") {
            var isSeries = tokens[i++] == "(";
            var values = new List<double>();
            while (tokens[i] != ")" && tokens[i] != "]") {
                values.Add(Parse(tokens, map, ref i));
            }
            i++; // Skip closing ) or ]
            if (isSeries) return values.Sum();
            else return 1.0 / values.Sum(v => 1.0 / v);
        } else {
            return map[tokens[i++]];
        }
    }
}
