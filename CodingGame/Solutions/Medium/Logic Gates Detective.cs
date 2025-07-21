using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Solution
{
    struct Rule { public string A, B, C, Op; }

    static void Main()
    {
        var line1 = Console.ReadLine();
        var line2 = Console.ReadLine();
        var line3 = Console.ReadLine();
        int n = int.Parse(line3);
        var ruleLines = new string[n];
        for (int i = 0; i < n; i++)
        {
            ruleLines[i] = Console.ReadLine();
        }

        var v = line1
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(tok => tok.Split(':'))
            .ToDictionary(p => p[0], p => int.Parse(p[1]));

        var outputs = line2
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var rules = ruleLines
            .Select(row => {
                var p = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return new Rule { A = p[0], Op = p[1], B = p[2], C = p[4] };
            })
            .ToList();

        Func<int,int,string,int> eval = (a,b,op) =>
            op == "and" ? (a & b)
          : op == "or"  ? (a | b)
          :                 (a ^ b);

        bool changed;
        do
        {
            changed = false;
            foreach (var r in rules)
            {
                bool hasA = v.ContainsKey(r.A);
                bool hasB = v.ContainsKey(r.B);
                bool hasC = v.ContainsKey(r.C);
                if (hasA && hasB && !hasC)
                {
                    v[r.C] = eval(v[r.A], v[r.B], r.Op);
                    changed = true;
                }
                else if (!hasC && hasA && !hasB)
                {
                    if (r.Op == "or" && v[r.A] == 1)
                    {
                        v[r.C] = 1; changed = true;
                    }
                    else if (r.Op == "and" && v[r.A] == 0)
                    {
                        v[r.C] = 0; changed = true;
                    }
                }
                else if (!hasC && !hasA && hasB)
                {
                    if (r.Op == "or" && v[r.B] == 1)
                    {
                        v[r.C] = 1; changed = true;
                    }
                    else if (r.Op == "and" && v[r.B] == 0)
                    {
                        v[r.C] = 0; changed = true;
                    }
                }
                else if (hasC)
                {
                    int cv = v[r.C];
                    if (r.Op == "or")
                    {
                        if (cv == 0)
                        {
                            if (!hasA) { v[r.A] = 0; changed = true; }
                            if (!hasB) { v[r.B] = 0; changed = true; }
                        }
                        else
                        {
                            if (hasA && v[r.A] == 0 && !hasB) { v[r.B] = 1; changed = true; }
                            if (hasB && v[r.B] == 0 && !hasA) { v[r.A] = 1; changed = true; }
                        }
                    }
                    else if (r.Op == "and")
                    {
                        if (cv == 1)
                        {
                            if (!hasA) { v[r.A] = 1; changed = true; }
                            if (!hasB) { v[r.B] = 1; changed = true; }
                        }
                        else
                        {
                            if (hasA && v[r.A] == 1 && !hasB) { v[r.B] = 0; changed = true; }
                            if (hasB && v[r.B] == 1 && !hasA) { v[r.A] = 0; changed = true; }
                        }
                    }
                    else // xor
                    {
                        if (hasA && !hasB) { v[r.B] = v[r.A] ^ cv; changed = true; }
                        else if (hasB && !hasA) { v[r.A] = v[r.B] ^ cv; changed = true; }
                    }
                }
            }
        } while (changed);

        var sb = new StringBuilder();
        foreach (var o in outputs)
            sb.Append(v.TryGetValue(o, out int val) ? val : 0);
        Console.WriteLine(sb);
    }
}
