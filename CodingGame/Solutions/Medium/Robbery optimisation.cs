using System;

class Solution {
    static void Main() {
        int n = int.Parse(Console.ReadLine());
        long best1 = 0, best2 = 0;
        for (int i = 0; i < n; i++) {
            long value = long.Parse(Console.ReadLine());
            long cur = Math.Max(best1, best2 + value);
            best2 = best1;
            best1 = cur;
        }
        Console.WriteLine(best1);
    }
}
