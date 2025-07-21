using System;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        var expr = Console.ReadLine();
        var solver = new EquationSolver();
        Console.WriteLine(solver.Solve(expr));
    }

    class EquationSolver
    {
        public string Solve(string expr)
        {
            var parts = expr.Split('=');
            var lhs = parts[0].Trim();
            var rhs = int.Parse(parts[1].Trim());
            var missing = new List<int>();
            var chars = lhs.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
                if (chars[i] == '?') missing.Add(i);
            var max = (int)Math.Pow(10, missing.Count);
            for (var n = 0; n < max; n++)
            {
                var digits = n.ToString().PadLeft(missing.Count, '0');
                for (var i = 0; i < missing.Count; i++)
                    chars[missing[i]] = digits[i];
                var candidate = new string(chars);
                if (Valid(candidate, rhs)) return candidate + " = " + rhs;
            }
            return "IMPOSSIBLE";
        }

        bool Valid(string lhs, int rhs)
        {
            var tokens = lhs.Split(' ');
            var op = tokens[1];
            int acc;
            if (!int.TryParse(tokens[0], out acc)) return false;
            for (var i = 2; i < tokens.Length; i += 2)
            {
                int val;
                if (!int.TryParse(tokens[i], out val)) return false;
                if (op == "+") acc += val;
                else if (op == "-") acc -= val;
                else if (op == "*") acc *= val;
                else if (op == "/")
                {
                    if (val == 0 || acc % val != 0) return false;
                    acc /= val;
                }
            }
            return acc == rhs;
        }
    }
}
