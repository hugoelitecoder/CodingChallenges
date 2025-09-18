using System;
using System.Linq;
using System.Collections.Generic;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        var deck = Console.ReadLine()
                        .Split(' ')
                        .ToList();

        for (int shuffle = 0; shuffle < N; shuffle++)
        {
            int m = deck.Count;
            int half = (m + 1) / 2;
            var first = deck.Take(half).ToList();
            var second = deck.Skip(half).ToList();

            var merged = new List<string>(m);
            for (int i = 0; i < half; i++)
            {
                merged.Add(first[i]);
                if (i < second.Count)
                    merged.Add(second[i]);
            }
            deck = merged;
        }

        Console.WriteLine(string.Join(" ", deck));
    }
}
