using System;
using System.Linq;

class Solution {
    static void Main() {
        var line = Console.ReadLine() ?? string.Empty;
        var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) {
            Console.WriteLine(0);
            return;
        }

        long[] n = tokens.Select(long.Parse).ToArray();
        int size = n.Length;
        long next;

        if (size == 1) {
            next = n[0];
        }
        else if (size == 2) {
            long diff = n[1] - n[0];
            next = n[1] + diff;
        }
        else {
            long x = n[0], y = n[1], z = n[2];
            long denom = y - x;
            long a, b;
            if (denom == 0) {
                a = 1;
                b = y - x;
            } else {
                a = (z - y) / denom;
                b = y - a * x;
            }
            next = a * n[size - 1] + b;
        }

        Console.WriteLine(next);
    }
}
