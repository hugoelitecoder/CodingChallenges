using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var expr = Console.ReadLine();
        var stack = new Stack<char>();
        var match = new Dictionary<char, char> { { ')', '(' }, { ']', '[' }, { '}', '{' } };

        foreach (var c in expr)
        {
            if (match.ContainsValue(c)) stack.Push(c);
            else if (match.ContainsKey(c))
            {
                if (stack.Count == 0 || stack.Pop() != match[c])
                {
                    Console.WriteLine("false");
                    return;
                }
            }
        }

        Console.WriteLine(stack.Count == 0 ? "true" : "false");
    }
}
