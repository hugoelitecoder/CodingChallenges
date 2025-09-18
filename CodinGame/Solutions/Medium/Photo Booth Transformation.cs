using System;

class Solution {
    static void Main() {
        int T = int.Parse(Console.ReadLine());
        while (T-- > 0) {
            var parts = Console.ReadLine().Split();
            int W = int.Parse(parts[0]), H = int.Parse(parts[1]);
            long p1 = Order(W - 1), p2 = Order(H - 1);
            long g = Gcd(p1, p2);
            Console.WriteLine(p1 / g * p2);
        }
    }

    static long Order(int m) {
        if (m <= 1) return 1;
        int cur = 2 % m;
        for (int k = 1; ; k++) {
            if (cur == 1) return k;
            cur = (cur * 2) % m;
        }
    }

    static long Gcd(long a, long b) {
        while (b != 0) {
            long t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}
