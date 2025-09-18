using System;
using System.Linq;
using System.Collections.Generic;

enum Operand { Add, Sub }

class Program
{
    static readonly Dictionary<char, Operand> operandLookup = new() { ['+'] = Operand.Add, ['-'] = Operand.Sub };
    static readonly Dictionary<Operand, string> commandLookup = new() { [Operand.Add] = "ADD", [Operand.Sub] = "SUB" };

    static void Main()
    {
        var parts = (Console.ReadLine() ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var operations = new List<(Operand type, int value)>();
        int idx = 0;

        // Handle leading number as implicit ADD
        if (parts.Length > 0 && char.IsDigit(parts[0][0]))
        {
            operations.Add((Operand.Add, int.Parse(parts[0])));
            idx = 1;
        }

        // Parse remaining sign-value pairs
        for (; idx + 1 < parts.Length; idx += 2)
        {
            if (!operandLookup.TryGetValue(parts[idx][0], out var op))
                continue;
            operations.Add((op, int.Parse(parts[idx + 1])));
        }

        // Group and output instructions
        foreach (var group in operations.GroupBy(op => op))
        {
            int count = group.Count();
            if (count > 1)
                Console.WriteLine($"REPEAT {count}");

            var (type, value) = group.Key;
            Console.WriteLine($"{commandLookup[type]} cgx {value}");
        }

        Console.WriteLine("EXIT");
    }
}