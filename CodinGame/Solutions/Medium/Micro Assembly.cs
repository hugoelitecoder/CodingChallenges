using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var regsInput = Console.ReadLine().Split(' ');
        var regs = new Dictionary<string, int>
        {
            ["a"] = int.Parse(regsInput[0]),
            ["b"] = int.Parse(regsInput[1]),
            ["c"] = int.Parse(regsInput[2]),
            ["d"] = int.Parse(regsInput[3]),
        };

        int n = int.Parse(Console.ReadLine());
        var prog = new string[n][];
        for (int i = 0; i < n; i++)
            prog[i] = Console.ReadLine().Split(' ');

        int pc = 0;
        while (pc >= 0 && pc < n)
        {
            var inst = prog[pc];
            switch (inst[0])
            {
                case "MOV":
                    regs[inst[1]] = Eval(inst[2], regs);
                    pc++;
                    break;

                case "ADD":
                    regs[inst[1]] = Eval(inst[2], regs) + Eval(inst[3], regs);
                    pc++;
                    break;

                case "SUB":
                    regs[inst[1]] = Eval(inst[2], regs) - Eval(inst[3], regs);
                    pc++;
                    break;

                case "JNE":
                    int target = int.Parse(inst[1]);
                    if (Eval(inst[2], regs) != Eval(inst[3], regs))
                        pc = target;
                    else
                        pc++;
                    break;

                default:
                    pc++;
                    break;
            }
        }
        Console.WriteLine($"{regs["a"]} {regs["b"]} {regs["c"]} {regs["d"]}");
    }

    static int Eval(string token, Dictionary<string, int> regs)
    {
        return regs.ContainsKey(token)
            ? regs[token]
            : int.Parse(token);
    }
}
