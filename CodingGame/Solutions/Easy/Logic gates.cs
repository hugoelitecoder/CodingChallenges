using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main(string[] args) {
        int n = int.Parse(Console.ReadLine());
        int m = int.Parse(Console.ReadLine());

        var signals = new Dictionary<string, string>();
        for (int i = 0; i < n; i++) {
            var input = Console.ReadLine().Split();
            signals[input[0]] = input[1];
        }

        var gates = new List<(string name, string type, string in1, string in2)>();
        for (int i = 0; i < m; i++) {
            var gate = Console.ReadLine().Split();
            gates.Add((gate[0], gate[1], gate[2], gate[3]));
        }

        foreach (var (name, type, in1, in2) in gates) {
            var s1 = signals[in1];
            var s2 = signals[in2];
            var result = new StringBuilder();

            for (int i = 0; i < s1.Length; i++) {
                bool a = s1[i] == '-';
                bool b = s2[i] == '-';
                bool res = type switch {
                    "AND" => a && b,
                    "OR" => a || b,
                    "XOR" => a ^ b,
                    "NAND" => !(a && b),
                    "NOR" => !(a || b),
                    "NXOR" => !(a ^ b),
                    _ => false
                };
                result.Append(res ? '-' : '_');
            }

            signals[name] = result.ToString();
            Console.WriteLine($"{name} {result}");
        }
    }
}
