using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution {
    static void Main(string[] args) {
        int n = int.Parse(Console.ReadLine());
        var ops = new (string op, string a1, string a2)[n];
        for (int i = 0; i < n; i++) {
            var line = Console.ReadLine().Split();
            ops[i] = (line[0], line[1], line[2]);
        }

        var memo = new int?[n];
        for (int i = 0; i < n; i++)
            Console.WriteLine(Eval(i, ops, memo));
    }

    static int Eval(int i, (string op, string a1, string a2)[] ops, int?[] memo) {
        if (memo[i].HasValue) return memo[i].Value;
        var (op, a1, a2) = ops[i];
        int v1 = ParseArg(a1, ops, memo);
        int v2 = ParseArg(a2, ops, memo);

        int res = op switch {
            "VALUE" => v1,
            "ADD" => v1 + v2,
            "SUB" => v1 - v2,
            "MULT" => v1 * v2,
            _ => 0
        };
        return (memo[i] = res).Value;
    }

    static int ParseArg(string arg, (string op, string a1, string a2)[] ops, int?[] memo) {
        if (arg == "_") return 0;
        if (arg.StartsWith("$")) return Eval(int.Parse(arg[1..]), ops, memo);
        return int.Parse(arg);
    }
}
