using System;

class Solution {
    static void Main() {
        long n = long.Parse(Console.ReadLine()) + 1;
        while (true) {
            int s = Convert.ToString(n, 2).Length;
            bool changed = false;
            for (int i = s - 1; i >= 0; i--) {
                int b0 = (int)((n >> i) & 1);
                int b1 = (int)((n >> (i + 1)) & 1);
                int b2 = (int)((n >> (i + 2)) & 1);
                if (b0 == b1 && b1 == b2) {
                    long mask = (1L << (s + 1)) - (1L << i);
                    n &= mask;
                    if (((n >> i) & 1) == 0) n += 1L << i;
                    else n += 1L << (i + 1);
                    changed = true;
                    break;
                }
            }
            if (!changed) break;
        }
        Console.WriteLine(n);
    }
}
