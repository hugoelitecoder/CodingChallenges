using System;
using System.Collections.Generic;
using System.Linq;

class Solution {
    
    static Dictionary<long,int> cache = new() { [1] = 1 };
    static int Collatz(long n) {
        if (cache.TryGetValue(n, out var c)) return c;
        cache[n] = Collatz(n % 2 == 0 ? n / 2 : 3 * n + 1) + 1;
        return cache[n];
    }

    static void Main() {
        int N = int.Parse(Console.ReadLine());
        while (N-- > 0) {
            var p = Console.ReadLine()
                           .Split()
                           .Select(int.Parse)
                           .OrderBy(x => x)
                           .ToArray();
            int lo = p[0], hi = p[1];
            var best = Enumerable
                .Range(lo, hi - lo + 1)
                .MaxBy(i => Collatz(i));
            Console.WriteLine($"{best} {Collatz(best)}");
        }
    }
}
