using System;
using System.Collections.Generic;
using System.Linq;

class MC5999
{
    record Instr(char Prefix, string Op, string[] Args);

    static void Main()
    {
        int k = int.Parse(Console.ReadLine());
        var inputs = Console.ReadLine().Split().Select(int.Parse).ToList();
        int inputPtr = 0;
        int n = int.Parse(Console.ReadLine());
        var raw = Enumerable.Range(0, n)
                            .Select(_ => Console.ReadLine().Trim())
                            .Where(s => !string.IsNullOrEmpty(s) && !s.StartsWith("#"))
                            .ToList();

        var labels = new Dictionary<string, int>();
        var temp = new List<(string Code, char Prefix)>();
        foreach (var line in raw)
        {
            char p = "+-@".Contains(line[0]) ? line[0] : '\0';
            var code = (p == '\0') ? line : line[1..].TrimStart();
            while (code.Contains(':'))
            {
                var parts = code.Split(':', 2);
                labels[parts[0]] = temp.Count;
                code = parts[1].TrimStart();
            }
            if (code.Length > 0)
                temp.Add((code, p));
        }
        var instrs = temp.Select(t => {
            var tok = t.Code.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new Instr(t.Prefix, tok[0], tok[1..]);
        }).ToList();

        var regs = new Dictionary<string, int> { ["acc"] = 0, ["dat"] = 0, ["x0"] = 0, ["x1"] = 0 };
        var outputs = new List<int>();
        bool execPlus = false, execMinus = false;
        var executedOnce = new HashSet<int>();

        int Clamp(int v) => Math.Clamp(v, -999, 999);
        int GetVal(string t)
            => int.TryParse(t, out var v) ? v
               : t == "x0" ? inputs[inputPtr++] : regs[t];
        void SetVal(string t, int v)
        {
            if (t == "x1") outputs.Add(v);
            else if (regs.ContainsKey(t)) regs[t] = Clamp(v);
        }

        int ip = 0;
        var ops = new Dictionary<string, Action<Instr>>
        {
            ["mov"] = i => SetVal(i.Args[1], GetVal(i.Args[0])), 
            ["add"] = i => regs["acc"] = Clamp(regs["acc"] + GetVal(i.Args[0])), 
            ["sub"] = i => regs["acc"] = Clamp(regs["acc"] - GetVal(i.Args[0])), 
            ["mul"] = i => regs["acc"] = Clamp(regs["acc"] * GetVal(i.Args[0])), 
            ["not"] = i => regs["acc"] = regs["acc"] == 0 ? 100 : 0,
            ["dgt"] = i => regs["acc"] = (int)(Math.Abs(regs["acc"]) / Math.Pow(10, GetVal(i.Args[0]))) % 10,
            ["dst"] = i => {
                int pos = GetVal(i.Args[0]), d = GetVal(i.Args[1]);
                int v = regs["acc"], p10 = (int)Math.Pow(10, pos);
                int curr = (Math.Abs(v) / p10) % 10;
                int abs = Math.Abs(v) - curr * p10 + d * p10;
                regs["acc"] = v < 0 ? -abs : abs;
            },
            ["jmp"] = i => ip = labels[i.Args[0]],
            ["teq"] = i => { bool r = GetVal(i.Args[0]) == GetVal(i.Args[1]); execPlus = r; execMinus = !r; },
            ["tgt"] = i => { bool r = GetVal(i.Args[0]) > GetVal(i.Args[1]); execPlus = r; execMinus = !r; },
            ["tlt"] = i => { bool r = GetVal(i.Args[0]) < GetVal(i.Args[1]); execPlus = r; execMinus = !r; },
            ["tcp"] = i => {
                int a = GetVal(i.Args[0]), b = GetVal(i.Args[1]);
                if (a > b) { execPlus = true; execMinus = false; }
                else if (a < b) { execPlus = false; execMinus = true; }
                else { execPlus = false; execMinus = false; }
            }
        };

        while (ip < instrs.Count)
        {
            var inst = instrs[ip];
            ip++;
            bool can = inst.Prefix switch
            {
                '+' => execPlus,
                '-' => execMinus,
                '@' => !executedOnce.Contains(ip - 1),
                _   => true
            };
            if (!can) continue;
            if (inst.Prefix == '@') executedOnce.Add(ip - 1);
            if (ops.TryGetValue(inst.Op, out var act))
                act(inst);
        }

        Console.WriteLine(string.Join(' ', outputs));
    }
}