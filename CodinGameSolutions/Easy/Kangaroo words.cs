using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static bool IsJoey(string kangaroo, string joey)
    {
        int i = 0;
        foreach (char c in kangaroo)
        {
            if (i < joey.Length && c == joey[i]) i++;
        }
        return i == joey.Length;
    }

    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var result = new SortedDictionary<string, SortedSet<string>>();

        for (int n = 0; n < N; n++)
        {
            var words = Console.ReadLine().Split(',').Select(w => w.Trim()).Distinct().ToList();
            foreach (var w in words)
            {
                foreach (var j in words)
                {
                    if (w != j && IsJoey(w, j))
                    {
                        if (!result.ContainsKey(w))
                            result[w] = new SortedSet<string>();
                        result[w].Add(j);
                    }
                }
            }
        }

        if (result.Count == 0)
        {
            Console.WriteLine("NONE");
        }
        else
        {
            foreach (var kv in result)
                Console.WriteLine($"{kv.Key}: {string.Join(", ", kv.Value)}");
        }
    }
}
