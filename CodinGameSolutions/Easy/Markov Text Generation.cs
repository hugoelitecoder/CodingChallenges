using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static int random_seed = 0;
    static int Pick(int options)
    {
        random_seed += 7;
        return random_seed % options;
    }

    public static void Main()
    {
        var text = Console.ReadLine().Split(' ');
        var depth = int.Parse(Console.ReadLine());
        var length = int.Parse(Console.ReadLine());
        var seed = Console.ReadLine().Split(' ');

        var map = new Dictionary<string, List<string>>();
        for (var i = 0; i + depth < text.Length; i++)
        {
            var key = string.Join(' ', text.Skip(i).Take(depth));
            var next = text[i + depth];
            if (!map.ContainsKey(key)) map[key] = new List<string>();
            map[key].Add(next);
        }

        var output = new List<string>(seed);
        var state = string.Join(' ', seed);
        for (var i = seed.Length; i < length; i++)
        {
            if (!map.TryGetValue(state, out var options)) break;
            var choice = options[Pick(options.Count)];
            output.Add(choice);
            var parts = state.Split(' ').ToList();
            parts.RemoveAt(0);
            parts.Add(choice);
            state = string.Join(' ', parts);
        }

        Console.WriteLine(string.Join(' ', output));
    }
}
