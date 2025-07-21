using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        string N = Console.ReadLine().Trim();
        long target = long.Parse(Console.ReadLine());
        var results = new List<string>();
        BuildExpressions(N, target, 0, "", results);
        foreach (var expr in results)
            Console.WriteLine(expr);
    }

    static void BuildExpressions(string N, long target, int pos, string expr, List<string> results)
    {
        int len = N.Length;
        if (pos == len)
        {
            if (!string.IsNullOrEmpty(expr) && Evaluate(expr) == target)
                results.Add(expr);
            return;
        }
        if (pos == 0)
        {
            BuildExpressions(N, target, 1, N[0].ToString(), results);
        }
        else
        {
            char c = N[pos];
            BuildExpressions(N, target, pos + 1, expr + c, results);
            BuildExpressions(N, target, pos + 1, expr + "+" + c, results);
            BuildExpressions(N, target, pos + 1, expr + "-" + c, results);
        }
    }

    static long Evaluate(string expr)
    {
        long total = 0;
        long current = 0;
        int sign = 1;
        for (int i = 0; i < expr.Length; )
        {
            if (expr[i] == '+') { total += sign * current; current = 0; sign = 1; i++; }
            else if (expr[i] == '-') { total += sign * current; current = 0; sign = -1; i++; }
            else
            {
                current = 0;
                while (i < expr.Length && char.IsDigit(expr[i]))
                {
                    current = current * 10 + (expr[i] - '0');
                    i++;
                }
            }
        }
        total += sign * current;
        return total;
    }
}