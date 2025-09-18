using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        var counts = Console.ReadLine().Split(' ')
            .Select(int.Parse)
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        var sd = new SortedDictionary<int, long>(counts);
        if (sd.Any()) {
            int k = sd.Keys.First();
            int max = sd.Keys.Last();
            for (; k <= max; k++) {
                if (!sd.TryGetValue(k, out var c)) continue;
                long pairs = c / 2;
                sd[k] = c % 2;
                if (pairs > 0) {
                    int nxt = k + 1;
                    if (sd.ContainsKey(nxt)) sd[nxt] += pairs;
                    else sd[nxt] = pairs;
                    if (nxt > max) max = nxt;
                }
            }
        }

        Console.WriteLine(sd.Values.Sum());
    }
}
