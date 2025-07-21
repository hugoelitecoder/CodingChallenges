using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        string X = Console.ReadLine();
        string Y = Console.ReadLine();

        var dict = new Dictionary<char, HashSet<char>>();
        int n = X.Length;
        for (int i = 0; i < n; i++)
        {
            char xi = X[i];
            char yi = Y[i];

            if (!dict.TryGetValue(xi, out var set))
            {
                set = new HashSet<char>();
                dict[xi] = set;
            }
            set.Add(yi);
        }

        if (dict.Values.Any(s => s.Count > 1))
        {
            Console.WriteLine("CAN'T");
            return;
        }

        var keysToPrint = new List<char>();
        foreach (var kv in dict)
        {
            kv.Value.Remove(kv.Key);
            if (kv.Value.Count == 1)
                keysToPrint.Add(kv.Key);
        }

        if (keysToPrint.Count == 0)
        {
            Console.WriteLine("NONE");
        }
        else
        {
            foreach (char k in keysToPrint)
            {
                char to = dict[k].First(); 
                Console.WriteLine($"{k}->{to}");
            }
        }
    }
}
