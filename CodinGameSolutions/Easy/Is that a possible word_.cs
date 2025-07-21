using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        var alphabet = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToHashSet();
        var states = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var transitions = new Dictionary<string, Dictionary<char, string>>();
        foreach (var st in states) transitions[st] = new Dictionary<char, string>();

        var T = int.Parse(Console.ReadLine());
        for (var i = 0; i < T; i++)
        {
            var parts = Console.ReadLine().Split();
            transitions[parts[0]][parts[1][0]] = parts[2];
        }

        var start = Console.ReadLine().Trim();
        var accept = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        var W = int.Parse(Console.ReadLine());
        for (var i = 0; i < W; i++)
        {
            var word = Console.ReadLine().Trim();
            var state = start;
            var valid = true;

            foreach (var c in word)
            {
                if (!alphabet.Contains(c) || !transitions[state].TryGetValue(c, out state))
                {
                    valid = false;
                    break;
                }
            }

            Console.WriteLine(valid && accept.Contains(state) ? "true" : "false");
        }
    }
}
