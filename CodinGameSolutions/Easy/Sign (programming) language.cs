using System;
using System.Collections.Generic;

class Solution
{
    const string ADD = "/$";
    const string SUB = "//";
    const string MUL_POS = "/**";
    const string MUL_NEG = "/*/";
    const string NOP = "/*$";

    delegate void RefAction(ref int val);

    static void Main()
    {
        string program = Console.ReadLine();
        int reg = 0, i = 0, instructionCount = 0;
        bool counting = false;

        var handlers = new Dictionary<string, Func<string, int, (int nextIndex, RefAction apply)>>()
        {
            [ADD] = (prog, start) => Parse(prog, start + 2, n => (n, (ref int v) => v += n)),
            [SUB] = (prog, start) => Parse(prog, start + 2, n => (n, (ref int v) => v -= n)),
            [MUL_POS] = (prog, start) => Parse(prog, start + 3, n => (n, (ref int v) => v *= (n + 1))),
            [MUL_NEG] = (prog, start) => Parse(prog, start + 3, n => (n, (ref int v) => v *= -n)),
            [NOP] = (prog, start) => (start + 3, (ref int v) => { }) 
        };

        while (i < program.Length)
        {
            if (program[i] == '$')
            {
                if (!counting)
                {
                    counting = true;
                    instructionCount = 0;
                }
                else
                {
                    reg += instructionCount;
                    counting = false;
                }
                i++;
                continue;
            }

            bool matched = false;
            foreach (var kvp in handlers)
            {
                if (program.Substring(i).StartsWith(kvp.Key))
                {
                    var (nextIndex, apply) = kvp.Value(program, i);
                    apply(ref reg);
                    if (counting) instructionCount++;
                    i = nextIndex;
                    matched = true;
                    break;
                }
            }

            if (!matched) i++;
        }

        Console.WriteLine(reg);
    }

    static (int, RefAction) Parse(string program, int start, Func<int, (int, RefAction)> op)
    {
        int end = start;
        while (end < program.Length && program[end] != '/') end++;
        int len = end - start;
        var (consume, apply) = op(len);
        return (end + 1, apply);
    }
}
