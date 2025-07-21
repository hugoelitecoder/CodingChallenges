using System;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        string input = Console.ReadLine();
        var left = new Stack<char>();
        var right = new Stack<char>();

        foreach (char c in input)
        {
            if (c == '<' && left.Count > 0)
                right.Push(left.Pop());
            else if (c == '>' && right.Count > 0)
                left.Push(right.Pop());
            else if (c == '-' && left.Count > 0)
                left.Pop();
            else if (c != '<' && c != '>' && c != '-')
                left.Push(c);
        }

        var result = new List<char>(left);
        result.Reverse();
        Console.WriteLine(new string(result.ToArray()) + string.Concat(right));
    }
}
