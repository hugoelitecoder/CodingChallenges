using System;

class Solution {
    
    static void Main() {
        int N = int.Parse(Console.ReadLine());
        while (N-- > 0) {
            var parts = Console.ReadLine().Split();
            long st = long.Parse(parts[0]), ed = long.Parse(parts[1]);
            long total = SumDigits(st, ed);
            long half = total / 2;
            long low = st, high = ed;
            while (low < high) {
                long mid = (low + high + 1) >> 1;
                if (SumDigits(st, mid) <= half) low = mid;
                else high = mid - 1;
            }
            Console.WriteLine(low);
        }
    }

    static long SumDigits(long a, long b) {
        if (a > b) return 0;
        long res = 0;
        long pow10 = 1;
        for (int digits = 1; pow10 <= b; digits++, pow10 *= 10) {
            long start = Math.Max(a, pow10);
            long end = Math.Min(b, pow10 * 10 - 1);
            if (end >= start) {
                res += (end - start + 1) * digits;
            }
        }
        return res;
    }
}
