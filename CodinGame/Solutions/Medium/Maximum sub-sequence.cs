using System;
using System.Linq;
using System.Collections.Generic;

class Solution {
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        int[] A = Console.ReadLine().Split().Select(int.Parse).ToArray();
        
        var dp = new Dictionary<int, (int length, int start)>();
        foreach (int v in A) {
            int prevLen = dp.TryGetValue(v - 1, out var prev) ? prev.length : 0;
            int startVal = prevLen > 0 ? prev.start : v;
            int newLen = prevLen + 1;

            if (!dp.TryGetValue(v, out var curr) || newLen > curr.length ||
                (newLen == curr.length && startVal < curr.start)) {
                dp[v] = (newLen, startVal);
            }
        }
        var best = dp.Values
            .OrderByDescending(x => x.length)
            .ThenBy(x => x.start)
            .First();

        var result = Enumerable.Range(best.start, best.length);
        Console.WriteLine(string.Join(" ", result));
    }
}