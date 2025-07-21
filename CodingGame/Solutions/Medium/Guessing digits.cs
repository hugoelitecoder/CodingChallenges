using System;
using System.Linq;
using System.Collections.Generic;

class Solution {
    static void Main() {
        var x = int.Parse(Console.ReadLine());
        var y = int.Parse(Console.ReadLine());
        var pairs = Enumerable.Range(1, 9)
                              .SelectMany(a => Enumerable.Range(a, 10 - a)
                                                         .Select(b => (a, b)))
                              .ToList();
        string[] name = { "SARAH", "BURT" };
        int[] target = { y, x };
        for (int t = 1; t < 100; t++) {
            int idx = t % 2;
            if (pairs.Count(p => (idx == 1 ? p.a + p.b : p.a * p.b) == target[idx]) == 1) {
                var p = pairs.First(p => (idx == 1 ? p.a + p.b : p.a * p.b) == target[idx]);
                Console.WriteLine($"({p.a},{p.b}) {name[idx]} {(t + 1) / 2}");
                return;
            }
            var uniq = pairs.GroupBy(p => idx == 1 ? p.a + p.b : p.a * p.b)
                            .Where(g => g.Count() == 1)
                            .Select(g => g.Key)
                            .ToHashSet();
            if (uniq.Count == 0) break;
            pairs.RemoveAll(p => uniq.Contains(idx == 1 ? p.a + p.b : p.a * p.b));
        }
        Console.WriteLine("IMPOSSIBLE");
    }
}
