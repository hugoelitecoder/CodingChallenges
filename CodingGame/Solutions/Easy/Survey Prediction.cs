using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    public static void Main()
    {
        var n = int.Parse(Console.ReadLine());
        var ranges = new Dictionary<string, Dictionary<string, (int min, int max)>>();
        var queries = new List<(int age, string gender)>();
        for (var i = 0; i < n; i++)
        {
            var parts = Console.ReadLine().Split(' ');
            var age = int.Parse(parts[0]);
            var gender = parts[1];
            if (parts.Length == 3)
            {
                var genre = parts[2];
                if (!ranges.ContainsKey(gender))
                    ranges[gender] = new Dictionary<string, (int, int)>();
                if (ranges[gender].TryGetValue(genre, out var range))
                    ranges[gender][genre] = (Math.Min(range.min, age), Math.Max(range.max, age));
                else
                    ranges[gender][genre] = (age, age);
            }
            else
            {
                queries.Add((age, gender));
            }
        }
        foreach (var (age, gender) in queries)
        {
            if (!ranges.ContainsKey(gender))
            {
                Console.WriteLine("None");
                continue;
            }
            var matches = ranges[gender]
                .Where(kv => kv.Value.min <= age && age <= kv.Value.max)
                .Select(kv => kv.Key)
                .ToList();
            Console.WriteLine(matches.Count == 1 ? matches[0] : "None");
        }
    }
}